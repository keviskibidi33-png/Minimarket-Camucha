using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Orders.Commands;

public class RejectOrderCommandHandler : IRequestHandler<RejectOrderCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPdfService _pdfService;
    private readonly ILogger<RejectOrderCommandHandler> _logger;

    public RejectOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IPdfService pdfService,
        ILogger<RejectOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _pdfService = pdfService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RejectOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return Result<bool>.Failure("El motivo del rechazo es obligatorio");
            }

            var order = await _unitOfWork.WebOrders.GetByIdAsync(request.OrderId, cancellationToken);

            if (order == null)
            {
                return Result<bool>.Failure("Pedido no encontrado");
            }

            if (order.Status != "pending")
            {
                return Result<bool>.Failure("Solo se pueden rechazar pedidos en estado 'Pendiente'");
            }

            // Actualizar estado
            order.Status = "cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Enviar correo de rechazo en segundo plano con PDF adjunto
            // Usar Task.Run sin cancellationToken para asegurar que se ejecute
            _ = Task.Run(async () =>
            {
                string? pdfPath = null;
                try
                {
                    _logger.LogInformation("Iniciando envío de correo de rechazo para pedido {OrderNumber}", order.OrderNumber);
                    
                    // Validar que el email del cliente existe
                    if (string.IsNullOrWhiteSpace(order.CustomerEmail))
                    {
                        _logger.LogWarning("No se puede enviar correo de rechazo: el cliente no tiene email. OrderNumber: {OrderNumber}", order.OrderNumber);
                        return;
                    }

                    // Generar PDF de la boleta (aunque esté rechazado, el cliente debe tener el documento)
                    try
                    {
                        pdfPath = await _pdfService.GenerateWebOrderReceiptAsync(order.Id, "Boleta");
                        _logger.LogInformation("PDF generado para pedido rechazado {OrderNumber}: {PdfPath}", order.OrderNumber, pdfPath);
                    }
                    catch (Exception pdfEx)
                    {
                        _logger.LogWarning(pdfEx, "Error al generar PDF para pedido rechazado {OrderNumber}. Se enviará el correo sin PDF.", order.OrderNumber);
                        pdfPath = null; // Continuar sin PDF
                    }

                    // Enviar correo de rechazo
                    var emailSent = await _emailService.SendOrderRejectionAsync(
                        order.CustomerEmail,
                        order.CustomerName,
                        order.OrderNumber,
                        request.Reason,
                        pdfPath,
                        $"Boleta_{order.OrderNumber}.pdf"
                    );

                    if (emailSent)
                    {
                        _logger.LogInformation("Correo de rechazo enviado exitosamente. OrderNumber: {OrderNumber}, Email: {Email}", order.OrderNumber, order.CustomerEmail);
                    }
                    else
                    {
                        _logger.LogWarning("El método SendOrderRejectionAsync retornó false. OrderNumber: {OrderNumber}, Email: {Email}", order.OrderNumber, order.CustomerEmail);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar correo de rechazo. OrderNumber: {OrderNumber}, Email: {Email}", order.OrderNumber, order.CustomerEmail ?? "N/A");
                }
                finally
                {
                    // Limpiar archivo temporal después de un delay
                    if (!string.IsNullOrEmpty(pdfPath) && System.IO.File.Exists(pdfPath))
                    {
                        _ = Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(_ =>
                        {
                            try
                            {
                                System.IO.File.Delete(pdfPath);
                                _logger.LogInformation("PDF temporal eliminado: {PdfPath}", pdfPath);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "No se pudo eliminar el PDF temporal: {PdfPath}", pdfPath);
                            }
                        });
                    }
                }
            });

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting order");
            return Result<bool>.Failure($"Error al rechazar el pedido: {ex.Message}");
        }
    }
}


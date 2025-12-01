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
            _ = Task.Run(async () =>
            {
                string? pdfPath = null;
                try
                {
                    // Generar PDF de la boleta (aunque esté rechazado, el cliente debe tener el documento)
                    pdfPath = await _pdfService.GenerateWebOrderReceiptAsync(order.Id, "Boleta");
                    _logger.LogInformation("PDF generado para pedido rechazado {OrderNumber}: {PdfPath}", order.OrderNumber, pdfPath);

                    await _emailService.SendOrderRejectionAsync(
                        order.CustomerEmail,
                        order.CustomerName,
                        order.OrderNumber,
                        request.Reason,
                        pdfPath,
                        $"Boleta_{order.OrderNumber}.pdf"
                    );

                    _logger.LogInformation("Order rejection email sent with PDF. OrderNumber: {OrderNumber}", order.OrderNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send order rejection email. OrderNumber: {OrderNumber}", order.OrderNumber);
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
            }, cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting order");
            return Result<bool>.Failure($"Error al rechazar el pedido: {ex.Message}");
        }
    }
}


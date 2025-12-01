using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Orders.Commands;

public class ApproveOrderCommandHandler : IRequestHandler<ApproveOrderCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPdfService _pdfService;
    private readonly ILogger<ApproveOrderCommandHandler> _logger;

    public ApproveOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IPdfService pdfService,
        ILogger<ApproveOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _pdfService = pdfService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ApproveOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _unitOfWork.WebOrders.GetByIdAsync(request.OrderId, cancellationToken);

            if (order == null)
            {
                return Result<bool>.Failure("Pedido no encontrado");
            }

            if (order.Status != "pending")
            {
                return Result<bool>.Failure("Solo se pueden aprobar pedidos en estado 'Pendiente'");
            }

            // Actualizar estado
            order.Status = "confirmed";
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Enviar correos en segundo plano con PDF adjunto
            _ = Task.Run(async () =>
            {
                string? pdfPath = null;
                try
                {
                    // Generar PDF de la boleta
                    pdfPath = await _pdfService.GenerateWebOrderReceiptAsync(order.Id, "Boleta");
                    _logger.LogInformation("PDF generado para pedido {OrderNumber}: {PdfPath}", order.OrderNumber, pdfPath);

                    // Enviar correo de aprobación con PDF adjunto
                    await _emailService.SendOrderApprovalAsync(
                        order.CustomerEmail,
                        order.CustomerName,
                        order.OrderNumber,
                        order.Total,
                        order.PaymentMethod,
                        pdfPath,
                        $"Boleta_{order.OrderNumber}.pdf"
                    );

                    // Si requiere comprobante y tiene comprobante, enviar correo de verificación de pago con PDF
                    if (request.SendPaymentVerifiedEmail && order.RequiresPaymentProof && !string.IsNullOrEmpty(order.PaymentProofUrl))
                    {
                        await _emailService.SendPaymentVerifiedAsync(
                            order.CustomerEmail,
                            order.CustomerName,
                            order.OrderNumber,
                            order.Total,
                            pdfPath,
                            $"Boleta_{order.OrderNumber}.pdf"
                        );
                    }

                    _logger.LogInformation("Order approval emails sent with PDF. OrderNumber: {OrderNumber}", order.OrderNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send order approval emails. OrderNumber: {OrderNumber}", order.OrderNumber);
                }
                finally
                {
                    // Limpiar archivo temporal después de un delay para asegurar que el correo se envió
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
            _logger.LogError(ex, "Error approving order");
            return Result<bool>.Failure($"Error al aprobar el pedido: {ex.Message}");
        }
    }
}


using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sales.Commands;

public class SendSaleReceiptCommandHandler : IRequestHandler<SendSaleReceiptCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfService _pdfService;
    private readonly IEmailService _emailService;

    public SendSaleReceiptCommandHandler(
        IUnitOfWork unitOfWork,
        IPdfService pdfService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _pdfService = pdfService;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(SendSaleReceiptCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var sale = await _unitOfWork.Sales.GetByIdAsync(request.SaleId, cancellationToken);

            if (sale == null)
            {
                return Result<bool>.Failure("Venta no encontrada");
            }

            // Generar PDF
            var pdfPath = await _pdfService.GenerateSaleReceiptAsync(request.SaleId, request.DocumentType);

            // Obtener información del cliente
            var customerName = "Cliente";
            if (sale.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId.Value, cancellationToken);
                if (customer != null)
                {
                    customerName = customer.Name;
                }
            }

            // Enviar correo
            var emailSent = await _emailService.SendSaleReceiptAsync(
                request.Email,
                customerName,
                sale.DocumentNumber,
                pdfPath,
                request.DocumentType
            );

            // Limpiar archivo temporal después de un tiempo (opcional)
            // En producción, podrías guardar los PDFs en un almacenamiento permanente

            if (emailSent)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure("Error al enviar el correo electrónico");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error al procesar el envío: {ex.Message}");
        }
    }
}


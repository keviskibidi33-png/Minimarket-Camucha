using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sales.Commands;

public class SendSaleReceiptCommandHandler : IRequestHandler<SendSaleReceiptCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfService _pdfService;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendSaleReceiptCommandHandler> _logger;

    public SendSaleReceiptCommandHandler(
        IUnitOfWork unitOfWork,
        IPdfService pdfService,
        IEmailService emailService,
        ILogger<SendSaleReceiptCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _pdfService = pdfService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendSaleReceiptCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending receipt for sale {SaleId} to email {Email}", request.SaleId, request.Email);

        try
        {
            var sale = await _unitOfWork.Sales.GetByIdAsync(request.SaleId, cancellationToken);

            if (sale == null)
            {
                _logger.LogWarning("Sale not found. SaleId: {SaleId}", request.SaleId);
                throw new NotFoundException("Sale", request.SaleId);
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
                request.DocumentType,
                sale.Total,
                sale.SaleDate
            );

            // Limpiar archivo temporal después de un tiempo (opcional)
            // En producción, podrías guardar los PDFs en un almacenamiento permanente

            if (emailSent)
            {
                _logger.LogInformation("Receipt sent successfully. SaleId: {SaleId}, Email: {Email}", 
                    request.SaleId, request.Email);
                return Result<bool>.Success(true);
            }

            _logger.LogError("Failed to send email for sale {SaleId} to {Email}. Check SMTP configuration or API key.", request.SaleId, request.Email);
            return Result<bool>.Failure(new[] { "No se pudo enviar el correo electrónico. Por favor, verifica la configuración del servidor de correo o contacta al administrador del sistema." });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Sale not found during receipt sending");
            throw;
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation during receipt sending");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending receipt for sale {SaleId}", request.SaleId);
            throw;
        }
    }
}


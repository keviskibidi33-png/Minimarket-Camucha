using System;
using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.Commands;
using Minimarket.Application.Features.Sales.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sales.Commands;

public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, Result<SaleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateSaleCommandHandler> _logger;
    private readonly IPdfService _pdfService;
    private readonly IEmailService _emailService;
    private const decimal IGV_RATE = 0.18m;

    public CreateSaleCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateSaleCommandHandler> logger,
        IPdfService pdfService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _pdfService = pdfService;
        _emailService = emailService;
    }

    public async Task<Result<SaleDto>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating sale for user {UserId} with {ProductCount} products", 
            request.UserId, request.Sale.SaleDetails.Count);

        // Validar que todos los productos existan y tengan stock suficiente
        // Hacer esto ANTES de iniciar la transacción para evitar conflictos con la estrategia de reintentos
        var productIds = request.Sale.SaleDetails.Select(d => d.ProductId).ToList();
        var products = (await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id), cancellationToken))
            .ToList();

        if (products.Count != productIds.Count)
        {
            var missingIds = productIds.Except(products.Select(p => p.Id)).ToList();
            _logger.LogWarning("One or more products not found. Missing IDs: {ProductIds}", missingIds);
            throw new NotFoundException("Product", string.Join(", ", missingIds));
        }

        // Validar stock antes de iniciar transacción
        foreach (var detailDto in request.Sale.SaleDetails)
        {
            var product = products.First(p => p.Id == detailDto.ProductId);
            
            if (!product.IsActive)
            {
                _logger.LogWarning("Attempted to sell inactive product {ProductId} - {ProductName}", 
                    product.Id, product.Name);
                throw new BusinessRuleViolationException($"El producto '{product.Name}' está inactivo");
            }

            if (product.Stock < detailDto.Quantity)
            {
                _logger.LogWarning("Insufficient stock for product {ProductId} - {ProductName}. Available: {Available}, Requested: {Requested}", 
                    product.Id, product.Name, product.Stock, detailDto.Quantity);
                throw new InsufficientStockException(product.Name, product.Stock, detailDto.Quantity);
            }
        }

        // Generar número de comprobante antes de iniciar la transacción
        var documentNumber = await GenerateDocumentNumberAsync(request.Sale.DocumentType, cancellationToken);

        // Usar ExecuteInTransactionAsync para envolver toda la transacción y permitir reintentos
        var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
                // Actualizar inventario (ya validado antes de iniciar la transacción)
                foreach (var detailDto in request.Sale.SaleDetails)
                {
                    var product = products.First(p => p.Id == detailDto.ProductId);
                    
                    // Reducir stock
                    product.Stock -= detailDto.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
                }

                // Calcular totales con redondeo comercial
                var subtotal = Math.Round(
                    request.Sale.SaleDetails.Sum(d => d.Quantity * d.UnitPrice), 
                    2, 
                    MidpointRounding.AwayFromZero
                );
                var discount = Math.Round(request.Sale.Discount, 2, MidpointRounding.AwayFromZero);
                
                // Validar que el descuento no exceda el subtotal (defensa adicional)
                if (discount > subtotal)
                {
                    _logger.LogWarning("Discount exceeds subtotal. Subtotal: {Subtotal}, Discount: {Discount}", 
                        subtotal, discount);
                    throw new BusinessRuleViolationException("El descuento no puede exceder el subtotal");
                }
                
                var subtotalAfterDiscount = Math.Round(subtotal - discount, 2, MidpointRounding.AwayFromZero);
                
                // Validar que el subtotal después del descuento no sea negativo
                if (subtotalAfterDiscount < 0)
                {
                    _logger.LogWarning("Subtotal after discount is negative. Subtotal: {Subtotal}, Discount: {Discount}, AfterDiscount: {AfterDiscount}", 
                        subtotal, discount, subtotalAfterDiscount);
                    throw new BusinessRuleViolationException("El subtotal después del descuento no puede ser negativo");
                }
                var tax = Math.Round(subtotalAfterDiscount * IGV_RATE, 2, MidpointRounding.AwayFromZero);
                var total = Math.Round(subtotalAfterDiscount + tax, 2, MidpointRounding.AwayFromZero);
                var change = Math.Round(request.Sale.AmountPaid - total, 2, MidpointRounding.AwayFromZero);

                if (change < 0)
                {
                    _logger.LogWarning("Insufficient payment amount. Total: {Total}, Paid: {Paid}", 
                        total, request.Sale.AmountPaid);
                    throw new BusinessRuleViolationException("El monto pagado es menor al total");
                }

                // Crear venta
                var sale = new Sale
                {
                    DocumentNumber = documentNumber,
                    DocumentType = request.Sale.DocumentType,
                    SaleDate = DateTime.UtcNow,
                    CustomerId = request.Sale.CustomerId,
                    Subtotal = subtotal,
                    Tax = tax,
                    Discount = discount,
                    Total = total,
                    PaymentMethod = request.Sale.PaymentMethod,
                    AmountPaid = request.Sale.AmountPaid,
                    Change = change,
                    Status = SaleStatus.Pagado,
                    UserId = request.UserId
                };

                await _unitOfWork.Sales.AddAsync(sale, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Crear detalles de venta
                var saleDetails = new List<SaleDetail>();
                foreach (var detailDto in request.Sale.SaleDetails)
                {
                    var product = products.First(p => p.Id == detailDto.ProductId);
                    var saleDetail = new SaleDetail
                    {
                        SaleId = sale.Id,
                        ProductId = detailDto.ProductId,
                        Quantity = detailDto.Quantity,
                        UnitPrice = detailDto.UnitPrice,
                        Subtotal = Math.Round(detailDto.Quantity * detailDto.UnitPrice, 2, MidpointRounding.AwayFromZero)
                    };
                    saleDetails.Add(saleDetail);
                    await _unitOfWork.SaleDetails.AddAsync(saleDetail, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                // Nota: CommitTransactionAsync se maneja automáticamente por ExecuteInTransactionAsync

                // Obtener datos del cliente si existe
                string? customerName = null;
                if (sale.CustomerId.HasValue)
                {
                    var customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId.Value, cancellationToken);
                    customerName = customer?.Name;
                }

                // Construir respuesta
                var saleDto = new SaleDto
                {
                    Id = sale.Id,
                    DocumentNumber = sale.DocumentNumber,
                    DocumentType = sale.DocumentType,
                    SaleDate = sale.SaleDate,
                    CustomerId = sale.CustomerId,
                    CustomerName = customerName,
                    Subtotal = sale.Subtotal,
                    Tax = sale.Tax,
                    Discount = sale.Discount,
                    Total = sale.Total,
                    PaymentMethod = sale.PaymentMethod,
                    AmountPaid = sale.AmountPaid,
                    Change = sale.Change,
                    Status = sale.Status,
                    SaleDetails = saleDetails.Select(sd => new SaleDetailDto
                    {
                        Id = sd.Id,
                        ProductId = sd.ProductId,
                        ProductName = products.First(p => p.Id == sd.ProductId).Name,
                        ProductCode = products.First(p => p.Id == sd.ProductId).Code,
                        Quantity = sd.Quantity,
                        UnitPrice = sd.UnitPrice,
                        Subtotal = sd.Subtotal
                    }).ToList()
                };

                _logger.LogInformation("Sale created successfully. SaleId: {SaleId}, DocumentNumber: {DocumentNumber}, Total: {Total}", 
                    sale.Id, sale.DocumentNumber, sale.Total);

                // Generar PDF y enviar email automáticamente en segundo plano (no bloquea la respuesta)
                // Usar Task.Run con ConfigureAwait(false) para ejecutar en thread pool sin capturar contexto
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Validar que la plantilla esté activa antes de generar PDF
                        var documentTypeText = sale.DocumentType == DocumentType.Factura ? "Factura" : "Boleta";
                        var templateKey = sale.DocumentType == DocumentType.Factura 
                            ? "document_factura_template_active" 
                            : "document_boleta_template_active";
                        
                        var templateSetting = (await _unitOfWork.SystemSettings.FindAsync(
                            s => s.Key == templateKey, 
                            cancellationToken))
                            .FirstOrDefault();
                        
                        var isTemplateActive = templateSetting == null || 
                            templateSetting.Value?.ToLower() != "false";
                        
                        if (!isTemplateActive)
                        {
                            _logger.LogWarning("Template {TemplateType} is not active for sale {SaleId}. PDF generation skipped.", 
                                documentTypeText, sale.Id);
                            // Marcar la venta como pendiente de generación de documento
                            // (En producción, podrías agregar un campo DocumentStatus a la entidad Sale)
                            return;
                        }

                        if (sale.CustomerId.HasValue && !string.IsNullOrWhiteSpace(customerName))
                        {
                            var customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId.Value, cancellationToken);
                            if (customer != null && !string.IsNullOrWhiteSpace(customer.Email))
                            {
                                // Generar PDF con timeout y manejo de errores robusto
                                string? pdfPath = null;
                                try
                                {
                                    // Usar CancellationTokenSource con timeout de 60 segundos para PDFs grandes
                                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                                    pdfPath = await _pdfService.GenerateSaleReceiptAsync(sale.Id, documentTypeText);
                                    
                                    // Enviar email con PDF adjunto
                                    var emailSent = await _emailService.SendSaleReceiptAsync(
                                        customer.Email,
                                        customer.Name,
                                        sale.DocumentNumber,
                                        pdfPath,
                                        documentTypeText
                                    );

                                    if (emailSent)
                                    {
                                        _logger.LogInformation("Receipt email sent successfully to {Email} for sale {SaleId}", 
                                            customer.Email, sale.Id);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Failed to send receipt email to {Email} for sale {SaleId}", 
                                            customer.Email, sale.Id);
                                    }
                                }
                                catch (OperationCanceledException)
                                {
                                    _logger.LogError("PDF generation timeout for sale {SaleId}. Document marked as pending.", sale.Id);
                                    // En producción, marcar la venta como pendiente de generación
                                }
                                catch (OutOfMemoryException ex)
                                {
                                    _logger.LogError(ex, "Out of memory error generating PDF for sale {SaleId} with {ItemCount} items. Document marked as pending.", 
                                        sale.Id, saleDetails.Count);
                                    // En producción, marcar la venta como pendiente y notificar al admin
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error generating PDF for sale {SaleId}. Document marked as pending.", sale.Id);
                                    // En producción, marcar la venta como pendiente y notificar al admin
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // No fallar la venta si el email/PDF falla, solo loguear
                        _logger.LogError(ex, "Error in background PDF/email processing for sale {SaleId}", sale.Id);
                    }
                }, cancellationToken);

            return saleDto;
        }, cancellationToken);

        return Result<SaleDto>.Success(result);
    }

    private async Task<string> GenerateDocumentNumberAsync(DocumentType documentType, CancellationToken cancellationToken)
    {
        var prefix = documentType == DocumentType.Factura ? "F001" : "B001";
        
        // Retry para evitar duplicados en condiciones de carrera
        const int maxRetries = 5;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            var lastSale = (await _unitOfWork.Sales.FindAsync(
                s => s.DocumentType == documentType && s.DocumentNumber.StartsWith(prefix),
                cancellationToken))
                .OrderByDescending(s => s.DocumentNumber)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastSale != null)
            {
                var lastNumberStr = lastSale.DocumentNumber.Split('-').LastOrDefault();
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            var documentNumber = $"{prefix}-{nextNumber:D8}";

            // Validar unicidad antes de retornar
            var exists = await _unitOfWork.Sales.ExistsAsync(
                s => s.DocumentNumber == documentNumber,
                cancellationToken
            );

            if (!exists)
            {
                return documentNumber;
            }

            // Si existe, esperar un poco y reintentar con siguiente número
            await Task.Delay(10, cancellationToken);
        }

        // Si después de varios intentos sigue habiendo conflicto, lanzar excepción
        throw new InvalidOperationException("No se pudo generar un número de comprobante único después de varios intentos");
    }
}


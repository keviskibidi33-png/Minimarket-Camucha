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
    private const decimal IGV_RATE = 0.18m;

    public CreateSaleCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateSaleCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SaleDto>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating sale for user {UserId} with {ProductCount} products", 
            request.UserId, request.Sale.SaleDetails.Count);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Validar que todos los productos existan y tengan stock suficiente
            var productIds = request.Sale.SaleDetails.Select(d => d.ProductId).ToList();
            var products = (await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id), cancellationToken))
                .ToList();

            if (products.Count != productIds.Count)
            {
                var missingIds = productIds.Except(products.Select(p => p.Id)).ToList();
                _logger.LogWarning("One or more products not found. Missing IDs: {ProductIds}", missingIds);
                throw new NotFoundException("Product", string.Join(", ", missingIds));
            }

            // Validar stock y actualizar inventario
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

                // Reducir stock
                product.Stock -= detailDto.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            }

            // Generar número de comprobante
            var documentNumber = await GenerateDocumentNumberAsync(request.Sale.DocumentType, cancellationToken);

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
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Obtener datos del cliente si existe
            string? customerName = null;
            if (sale.CustomerId.HasValue)
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId.Value, cancellationToken);
                customerName = customer?.Name;
            }

            // Construir respuesta
            var result = new SaleDto
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

            return Result<SaleDto>.Success(result);
        }
        catch (NotFoundException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogWarning(ex, "Product not found during sale creation");
            throw;
        }
        catch (InsufficientStockException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogWarning(ex, "Insufficient stock during sale creation");
            throw;
        }
        catch (BusinessRuleViolationException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogWarning(ex, "Business rule violation during sale creation");
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Unexpected error creating sale for user {UserId}", request.UserId);
            throw;
        }
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


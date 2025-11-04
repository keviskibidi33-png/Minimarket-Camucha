using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.Queries;
using Minimarket.Application.Features.Sales.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sales.Queries;

public class GetAllSalesQueryHandler : IRequestHandler<GetAllSalesQuery, Result<PagedResult<SaleDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSalesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<SaleDto>>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
    {
        var allSales = await _unitOfWork.Sales.GetAllAsync(cancellationToken);
        var allCustomers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
        var allSaleDetails = await _unitOfWork.SaleDetails.GetAllAsync(cancellationToken);
        var allProducts = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        
        var customersDict = allCustomers.ToDictionary(c => c.Id, c => c.Name);
        var productsDict = allProducts.ToDictionary(p => p.Id, p => new { p.Name, p.Code });
        
        // Agrupar detalles por venta
        var saleDetailsDict = allSaleDetails
            .GroupBy(sd => sd.SaleId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Aplicar filtros en memoria
        var filteredSales = allSales.AsEnumerable();

        if (request.StartDate.HasValue)
        {
            filteredSales = filteredSales.Where(s => s.SaleDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            filteredSales = filteredSales.Where(s => s.SaleDate <= request.EndDate.Value);
        }

        if (request.CustomerId.HasValue)
        {
            filteredSales = filteredSales.Where(s => s.CustomerId == request.CustomerId.Value);
        }

        if (request.UserId.HasValue)
        {
            filteredSales = filteredSales.Where(s => s.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            filteredSales = filteredSales.Where(s => s.DocumentNumber.Contains(request.DocumentNumber));
        }

        // Ordenar
        var sortedSales = filteredSales.OrderByDescending(s => s.SaleDate).ToList();

        var totalCount = sortedSales.Count;

        // Aplicar paginación
        var sales = sortedSales
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sale => new SaleDto
            {
                Id = sale.Id,
                DocumentNumber = sale.DocumentNumber,
                DocumentType = sale.DocumentType,
                SaleDate = sale.SaleDate,
                CustomerId = sale.CustomerId,
                CustomerName = sale.CustomerId.HasValue && customersDict.TryGetValue(sale.CustomerId.Value, out var customerName) 
                    ? customerName 
                    : null,
                Subtotal = sale.Subtotal,
                Tax = sale.Tax,
                Discount = sale.Discount,
                Total = sale.Total,
                PaymentMethod = sale.PaymentMethod,
                AmountPaid = sale.AmountPaid,
                Change = sale.Change,
                Status = sale.Status,
                CancellationReason = sale.CancellationReason,
                SaleDetails = saleDetailsDict.TryGetValue(sale.Id, out var details)
                    ? details.Select(sd => new SaleDetailDto
                    {
                        Id = sd.Id,
                        ProductId = sd.ProductId,
                        ProductName = productsDict.TryGetValue(sd.ProductId, out var product) 
                            ? product.Name 
                            : "Producto eliminado",
                        ProductCode = productsDict.TryGetValue(sd.ProductId, out var productInfo) 
                            ? productInfo.Code 
                            : "",
                        Quantity = sd.Quantity,
                        UnitPrice = sd.UnitPrice,
                        Subtotal = sd.Subtotal
                    }).ToList()
                    : new List<SaleDetailDto>()
            })
            .ToList();

        var pagedResult = PagedResult<SaleDto>.Create(sales, totalCount, request.Page, request.PageSize);

        return Result<PagedResult<SaleDto>>.Success(pagedResult);
    }
}


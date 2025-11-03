using MediatR;
using Microsoft.EntityFrameworkCore;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.Queries;
using Minimarket.Application.Features.Sales.DTOs;
using Minimarket.Infrastructure.Data;

namespace Minimarket.Application.Features.Sales.Queries;

public class GetAllSalesQueryHandler : IRequestHandler<GetAllSalesQuery, Result<PagedResult<SaleDto>>>
{
    private readonly MinimarketDbContext _context;

    public GetAllSalesQueryHandler(MinimarketDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<SaleDto>>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
            .AsQueryable();

        if (request.StartDate.HasValue)
        {
            query = query.Where(s => s.SaleDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(s => s.SaleDate <= request.EndDate.Value);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(s => s.CustomerId == request.CustomerId.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(s => s.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            query = query.Where(s => s.DocumentNumber.Contains(request.DocumentNumber));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sale => new SaleDto
            {
                Id = sale.Id,
                DocumentNumber = sale.DocumentNumber,
                DocumentType = sale.DocumentType,
                SaleDate = sale.SaleDate,
                CustomerId = sale.CustomerId,
                CustomerName = sale.Customer != null ? sale.Customer.Name : null,
                Subtotal = sale.Subtotal,
                Tax = sale.Tax,
                Discount = sale.Discount,
                Total = sale.Total,
                PaymentMethod = sale.PaymentMethod,
                AmountPaid = sale.AmountPaid,
                Change = sale.Change,
                Status = sale.Status,
                CancellationReason = sale.CancellationReason,
                SaleDetails = sale.SaleDetails.Select(sd => new SaleDetailDto
                {
                    Id = sd.Id,
                    ProductId = sd.ProductId,
                    ProductName = sd.Product != null ? sd.Product.Name : "Producto eliminado",
                    ProductCode = sd.Product != null ? sd.Product.Code : "",
                    Quantity = sd.Quantity,
                    UnitPrice = sd.UnitPrice,
                    Subtotal = sd.Subtotal
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        var pagedResult = PagedResult<SaleDto>.Create(sales, totalCount, request.Page, request.PageSize);

        return Result<PagedResult<SaleDto>>.Success(pagedResult);
    }
}


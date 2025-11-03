using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Sales.Queries;
using Minimarket.Application.Features.Sales.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Sales.Queries;

public class GetSaleByIdQueryHandler : IRequestHandler<GetSaleByIdQuery, Result<SaleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSaleByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SaleDto>> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(request.Id, cancellationToken);

        if (sale == null)
        {
            return Result<SaleDto>.Failure("Venta no encontrada");
        }

        var customer = sale.CustomerId.HasValue
            ? await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId.Value, cancellationToken)
            : null;

        var saleDetails = (await _unitOfWork.SaleDetails.FindAsync(sd => sd.SaleId == sale.Id, cancellationToken)).ToList();
        var productIds = saleDetails.Select(sd => sd.ProductId).ToList();
        var products = productIds.Any()
            ? (await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id), cancellationToken)).ToList()
            : new List<Domain.Entities.Product>();

        var result = new SaleDto
        {
            Id = sale.Id,
            DocumentNumber = sale.DocumentNumber,
            DocumentType = sale.DocumentType,
            SaleDate = sale.SaleDate,
            CustomerId = sale.CustomerId,
            CustomerName = customer?.Name,
            Subtotal = sale.Subtotal,
            Tax = sale.Tax,
            Discount = sale.Discount,
            Total = sale.Total,
            PaymentMethod = sale.PaymentMethod,
            AmountPaid = sale.AmountPaid,
            Change = sale.Change,
            Status = sale.Status,
            CancellationReason = sale.CancellationReason,
            SaleDetails = saleDetails.Select(sd => new SaleDetailDto
            {
                Id = sd.Id,
                ProductId = sd.ProductId,
                ProductName = products.FirstOrDefault(p => p.Id == sd.ProductId)?.Name ?? "Producto eliminado",
                ProductCode = products.FirstOrDefault(p => p.Id == sd.ProductId)?.Code ?? "",
                Quantity = sd.Quantity,
                UnitPrice = sd.UnitPrice,
                Subtotal = sd.Subtotal
            }).ToList()
        };

        return Result<SaleDto>.Success(result);
    }
}


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
        // OPTIMIZACIÓN: Usar método optimizado del repositorio que aplica filtros y paginación en la BD
        // Esto evita cargar todos los registros en memoria (problema N+1 resuelto)
        var (sales, totalCount) = await _unitOfWork.SaleRepository.GetPagedSalesAsync(
            startDate: request.StartDate,
            endDate: request.EndDate,
            customerId: request.CustomerId,
            userId: request.UserId,
            documentNumber: request.DocumentNumber,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken
        );

        // Mapear a DTOs (los datos ya vienen con Eager Loading, no hay N+1)
        var saleDtos = sales.Select(sale => new SaleDto
        {
            Id = sale.Id,
            DocumentNumber = sale.DocumentNumber,
            DocumentType = sale.DocumentType,
            SaleDate = sale.SaleDate,
            CustomerId = sale.CustomerId,
            CustomerName = sale.Customer?.Name,
            CustomerDocumentType = sale.Customer?.DocumentType,
            CustomerDocumentNumber = sale.Customer?.DocumentNumber,
            CustomerAddress = sale.Customer?.Address,
            CustomerEmail = sale.Customer?.Email,
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
                ProductName = sd.Product?.Name ?? "Producto eliminado",
                ProductCode = sd.Product?.Code ?? "",
                Quantity = sd.Quantity,
                UnitPrice = sd.UnitPrice,
                Subtotal = sd.Subtotal
            }).ToList()
        }).ToList();

        var pagedResult = PagedResult<SaleDto>.Create(saleDtos, totalCount, request.Page, request.PageSize);

        return Result<PagedResult<SaleDto>>.Success(pagedResult);
    }
}


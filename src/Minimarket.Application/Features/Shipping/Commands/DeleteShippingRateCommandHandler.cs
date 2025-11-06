using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Shipping.Commands;

public class DeleteShippingRateCommandHandler : IRequestHandler<DeleteShippingRateCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteShippingRateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteShippingRateCommand request, CancellationToken cancellationToken)
    {
        var shippingRate = await _unitOfWork.ShippingRates.GetByIdAsync(request.Id, cancellationToken);
        
        if (shippingRate == null)
        {
            throw new NotFoundException($"Shipping rate with ID {request.Id} not found");
        }

        await _unitOfWork.ShippingRates.DeleteAsync(shippingRate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}


using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Orders.Commands;

public class UpdatePaymentProofCommandHandler : IRequestHandler<UpdatePaymentProofCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePaymentProofCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(UpdatePaymentProofCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.WebOrders.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result<bool>.Failure("Pedido no encontrado");
        }

        order.PaymentProofUrl = request.PaymentProofUrl;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}


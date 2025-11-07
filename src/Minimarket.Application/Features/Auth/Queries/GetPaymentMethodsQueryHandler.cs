using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Auth.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Queries;

public class GetPaymentMethodsQueryHandler : IRequestHandler<GetPaymentMethodsQuery, Result<List<PaymentMethodResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPaymentMethodsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<PaymentMethodResponse>>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        var paymentMethods = await _unitOfWork.UserPaymentMethods
            .FindAsync(upm => upm.UserId == request.UserId, cancellationToken);
        
        var response = paymentMethods.Select(pm => new PaymentMethodResponse
        {
            Id = pm.Id,
            CardHolderName = pm.CardHolderName,
            CardNumberMasked = pm.CardNumberMasked,
            CardType = pm.CardType,
            ExpiryMonth = pm.ExpiryMonth,
            ExpiryYear = pm.ExpiryYear,
            IsDefault = pm.IsDefault,
            Last4Digits = pm.Last4Digits
        }).ToList();
        
        return Result<List<PaymentMethodResponse>>.Success(response);
    }
}


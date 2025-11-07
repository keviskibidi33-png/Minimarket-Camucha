using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.PaymentMethods.Queries;

public class GetPaymentMethodSettingsQueryHandler : IRequestHandler<GetPaymentMethodSettingsQuery, Result<List<PaymentMethodSettingsDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPaymentMethodSettingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<PaymentMethodSettingsDto>>> Handle(GetPaymentMethodSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _unitOfWork.PaymentMethodSettings
            .GetAllAsync(cancellationToken);

        var settingsList = settings
            .OrderBy(pms => pms.DisplayOrder)
            .ThenBy(pms => pms.Name)
            .Select(pms => new PaymentMethodSettingsDto
            {
                Id = pms.Id,
                PaymentMethodId = pms.PaymentMethodId,
                Name = pms.Name,
                IsEnabled = pms.IsEnabled,
                RequiresCardDetails = pms.RequiresCardDetails,
                Description = pms.Description,
                DisplayOrder = pms.DisplayOrder
            })
            .ToList();

        return Result<List<PaymentMethodSettingsDto>>.Success(settingsList);
    }
}


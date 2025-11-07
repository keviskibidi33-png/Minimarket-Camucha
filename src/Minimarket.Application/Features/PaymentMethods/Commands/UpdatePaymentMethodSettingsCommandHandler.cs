using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.PaymentMethods.Queries;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.PaymentMethods.Commands;

public class UpdatePaymentMethodSettingsCommandHandler : IRequestHandler<UpdatePaymentMethodSettingsCommand, Result<PaymentMethodSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePaymentMethodSettingsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PaymentMethodSettingsDto>> Handle(UpdatePaymentMethodSettingsCommand request, CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.PaymentMethodSettings
            .FirstOrDefaultAsync(pms => pms.Id == request.Id, cancellationToken);

        if (setting == null)
        {
            return Result<PaymentMethodSettingsDto>.Failure("Configuración de método de pago no encontrada");
        }

        setting.IsEnabled = request.IsEnabled;
        setting.DisplayOrder = request.DisplayOrder;
        if (request.Description != null)
        {
            setting.Description = request.Description;
        }

        await _unitOfWork.PaymentMethodSettings.UpdateAsync(setting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PaymentMethodSettingsDto>.Success(new PaymentMethodSettingsDto
        {
            Id = setting.Id,
            PaymentMethodId = setting.PaymentMethodId,
            Name = setting.Name,
            IsEnabled = setting.IsEnabled,
            RequiresCardDetails = setting.RequiresCardDetails,
            Description = setting.Description,
            DisplayOrder = setting.DisplayOrder
        });
    }
}


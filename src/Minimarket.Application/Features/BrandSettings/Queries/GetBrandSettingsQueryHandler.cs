using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.BrandSettings.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.BrandSettings.Queries;

public class GetBrandSettingsQueryHandler : IRequestHandler<GetBrandSettingsQuery, Result<BrandSettingsDto?>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBrandSettingsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BrandSettingsDto?>> Handle(GetBrandSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _unitOfWork.BrandSettings.GetAllAsync(cancellationToken);
        var brandSettings = settings.FirstOrDefault();

        if (brandSettings == null)
        {
            return Result<BrandSettingsDto?>.Success(null);
        }

        var dto = new BrandSettingsDto
        {
            Id = brandSettings.Id,
            LogoUrl = brandSettings.LogoUrl,
            LogoEmoji = brandSettings.LogoEmoji,
            StoreName = brandSettings.StoreName,
            FaviconUrl = brandSettings.FaviconUrl,
            PrimaryColor = brandSettings.PrimaryColor,
            SecondaryColor = brandSettings.SecondaryColor,
            ButtonColor = brandSettings.ButtonColor,
            TextColor = brandSettings.TextColor,
            HoverColor = brandSettings.HoverColor,
            Description = brandSettings.Description,
            Slogan = brandSettings.Slogan,
            Phone = brandSettings.Phone,
            WhatsAppPhone = brandSettings.WhatsAppPhone,
            Email = brandSettings.Email,
            Address = brandSettings.Address,
            Ruc = brandSettings.Ruc,
            YapePhone = brandSettings.YapePhone,
            PlinPhone = brandSettings.PlinPhone,
            YapeQRUrl = brandSettings.YapeQRUrl,
            PlinQRUrl = brandSettings.PlinQRUrl,
            YapeEnabled = brandSettings.YapeEnabled,
            PlinEnabled = brandSettings.PlinEnabled,
            BankName = brandSettings.BankName,
            BankAccountType = brandSettings.BankAccountType,
            BankAccountNumber = brandSettings.BankAccountNumber,
            BankCCI = brandSettings.BankCCI,
            BankAccountVisible = brandSettings.BankAccountVisible,
            DeliveryType = brandSettings.DeliveryType,
            DeliveryCost = brandSettings.DeliveryCost,
            DeliveryZones = brandSettings.DeliveryZones,
            HomeTitle = brandSettings.HomeTitle,
            HomeSubtitle = brandSettings.HomeSubtitle,
            HomeDescription = brandSettings.HomeDescription,
            HomeBannerImageUrl = brandSettings.HomeBannerImageUrl,
            CreatedAt = brandSettings.CreatedAt,
            UpdatedAt = brandSettings.UpdatedAt
        };

        return Result<BrandSettingsDto?>.Success(dto);
    }
}


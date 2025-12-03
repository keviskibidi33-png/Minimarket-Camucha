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
        try
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
                LogoUrl = brandSettings.LogoUrl ?? string.Empty,
                LogoEmoji = brandSettings.LogoEmoji,
                StoreName = brandSettings.StoreName ?? string.Empty,
                FaviconUrl = brandSettings.FaviconUrl,
                PrimaryColor = brandSettings.PrimaryColor ?? "#4CAF50",
                SecondaryColor = brandSettings.SecondaryColor ?? "#0d7ff2",
                ButtonColor = brandSettings.ButtonColor ?? "#4CAF50",
                TextColor = brandSettings.TextColor ?? "#333333",
                HoverColor = brandSettings.HoverColor ?? "#45a049",
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
                DeliveryType = brandSettings.DeliveryType ?? "Ambos",
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
        catch (Exception ex)
        {
            return Result<BrandSettingsDto?>.Failure($"Error al obtener la configuración de marca: {ex.Message}");
        }
    }
}


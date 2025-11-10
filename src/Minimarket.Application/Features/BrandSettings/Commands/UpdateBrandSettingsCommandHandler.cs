using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.BrandSettings.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.BrandSettings.Commands;

public class UpdateBrandSettingsCommandHandler : IRequestHandler<UpdateBrandSettingsCommand, Result<BrandSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBrandSettingsCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BrandSettingsDto>> Handle(UpdateBrandSettingsCommand request, CancellationToken cancellationToken)
    {
        var existingSettings = await _unitOfWork.BrandSettings.GetAllAsync(cancellationToken);
        var brandSettings = existingSettings.FirstOrDefault();

        if (brandSettings == null)
        {
            // Crear nueva configuración
            brandSettings = new Domain.Entities.BrandSettings
            {
                LogoUrl = request.BrandSettings.LogoUrl,
                StoreName = request.BrandSettings.StoreName,
                FaviconUrl = request.BrandSettings.FaviconUrl,
                PrimaryColor = request.BrandSettings.PrimaryColor,
                SecondaryColor = request.BrandSettings.SecondaryColor,
                ButtonColor = request.BrandSettings.ButtonColor,
                TextColor = request.BrandSettings.TextColor,
                HoverColor = request.BrandSettings.HoverColor,
                Description = request.BrandSettings.Description,
                Slogan = request.BrandSettings.Slogan,
                Phone = request.BrandSettings.Phone,
                Email = request.BrandSettings.Email,
                Address = request.BrandSettings.Address,
                Ruc = request.BrandSettings.Ruc,
                YapePhone = request.BrandSettings.YapePhone,
                PlinPhone = request.BrandSettings.PlinPhone,
                YapeQRUrl = request.BrandSettings.YapeQRUrl,
                PlinQRUrl = request.BrandSettings.PlinQRUrl,
                YapeEnabled = request.BrandSettings.YapeEnabled,
                PlinEnabled = request.BrandSettings.PlinEnabled,
                BankName = request.BrandSettings.BankName,
                BankAccountType = request.BrandSettings.BankAccountType,
                BankAccountNumber = request.BrandSettings.BankAccountNumber,
                BankCCI = request.BrandSettings.BankCCI,
                BankAccountVisible = request.BrandSettings.BankAccountVisible,
                DeliveryType = request.BrandSettings.DeliveryType ?? "Ambos",
                DeliveryCost = request.BrandSettings.DeliveryCost,
                DeliveryZones = request.BrandSettings.DeliveryZones,
                HomeTitle = request.BrandSettings.HomeTitle,
                HomeSubtitle = request.BrandSettings.HomeSubtitle,
                HomeDescription = request.BrandSettings.HomeDescription,
                HomeBannerImageUrl = request.BrandSettings.HomeBannerImageUrl,
                UpdatedBy = request.UpdatedBy
            };

            await _unitOfWork.BrandSettings.AddAsync(brandSettings, cancellationToken);
        }
        else
        {
            // Actualizar configuración existente
            brandSettings.LogoUrl = request.BrandSettings.LogoUrl;
            brandSettings.StoreName = request.BrandSettings.StoreName;
            brandSettings.FaviconUrl = request.BrandSettings.FaviconUrl;
            brandSettings.PrimaryColor = request.BrandSettings.PrimaryColor;
            brandSettings.SecondaryColor = request.BrandSettings.SecondaryColor;
            brandSettings.ButtonColor = request.BrandSettings.ButtonColor;
            brandSettings.TextColor = request.BrandSettings.TextColor;
            brandSettings.HoverColor = request.BrandSettings.HoverColor;
            brandSettings.Description = request.BrandSettings.Description;
            brandSettings.Slogan = request.BrandSettings.Slogan;
            brandSettings.Phone = request.BrandSettings.Phone;
            brandSettings.Email = request.BrandSettings.Email;
            brandSettings.Address = request.BrandSettings.Address;
            brandSettings.Ruc = request.BrandSettings.Ruc;
            brandSettings.YapePhone = request.BrandSettings.YapePhone;
            brandSettings.PlinPhone = request.BrandSettings.PlinPhone;
            brandSettings.YapeQRUrl = request.BrandSettings.YapeQRUrl;
            brandSettings.PlinQRUrl = request.BrandSettings.PlinQRUrl;
            brandSettings.YapeEnabled = request.BrandSettings.YapeEnabled;
            brandSettings.PlinEnabled = request.BrandSettings.PlinEnabled;
            brandSettings.BankName = request.BrandSettings.BankName;
            brandSettings.BankAccountType = request.BrandSettings.BankAccountType;
            brandSettings.BankAccountNumber = request.BrandSettings.BankAccountNumber;
            brandSettings.BankCCI = request.BrandSettings.BankCCI;
            brandSettings.BankAccountVisible = request.BrandSettings.BankAccountVisible;
            brandSettings.DeliveryType = request.BrandSettings.DeliveryType ?? "Ambos";
            brandSettings.DeliveryCost = request.BrandSettings.DeliveryCost;
            brandSettings.DeliveryZones = request.BrandSettings.DeliveryZones;
            brandSettings.HomeTitle = request.BrandSettings.HomeTitle;
            brandSettings.HomeSubtitle = request.BrandSettings.HomeSubtitle;
            brandSettings.HomeDescription = request.BrandSettings.HomeDescription;
            brandSettings.HomeBannerImageUrl = request.BrandSettings.HomeBannerImageUrl;
            brandSettings.UpdatedBy = request.UpdatedBy;

            await _unitOfWork.BrandSettings.UpdateAsync(brandSettings, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new BrandSettingsDto
        {
            Id = brandSettings.Id,
            LogoUrl = brandSettings.LogoUrl,
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

        return Result<BrandSettingsDto>.Success(dto);
    }
}


using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.BrandSettings.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.BrandSettings.Commands;

public class UpdateBrandSettingsCommandHandler : IRequestHandler<UpdateBrandSettingsCommand, Result<BrandSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateBrandSettingsCommandHandler> _logger;

    public UpdateBrandSettingsCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateBrandSettingsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
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
                // LogoUrl es requerido, usar string vacío si viene null o vacío
                LogoUrl = !string.IsNullOrWhiteSpace(request.BrandSettings.LogoUrl) 
                    ? request.BrandSettings.LogoUrl.Trim() 
                    : string.Empty,
                LogoEmoji = request.BrandSettings.LogoEmoji,
                StoreName = request.BrandSettings.StoreName,
                FaviconUrl = request.BrandSettings.FaviconUrl,
                PrimaryColor = request.BrandSettings.PrimaryColor,
                SecondaryColor = request.BrandSettings.SecondaryColor,
                ButtonColor = request.BrandSettings.ButtonColor,
                TextColor = request.BrandSettings.TextColor,
                HoverColor = request.BrandSettings.HoverColor,
                Description = request.BrandSettings.Description,
                Slogan = request.BrandSettings.Slogan,
                Phone = string.IsNullOrWhiteSpace(request.BrandSettings.Phone) ? null : request.BrandSettings.Phone,
                WhatsAppPhone = string.IsNullOrWhiteSpace(request.BrandSettings.WhatsAppPhone) ? null : request.BrandSettings.WhatsAppPhone,
                Email = string.IsNullOrWhiteSpace(request.BrandSettings.Email) ? null : request.BrandSettings.Email,
                Address = string.IsNullOrWhiteSpace(request.BrandSettings.Address) ? null : request.BrandSettings.Address,
                Ruc = string.IsNullOrWhiteSpace(request.BrandSettings.Ruc) ? null : request.BrandSettings.Ruc,
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
            // Log para debugging
            _logger.LogInformation("=== ACTUALIZANDO BRANDSETTINGS ===");
            _logger.LogInformation("StoreName recibido: '{StoreName}'", request.BrandSettings.StoreName);
            _logger.LogInformation("PrimaryColor recibido: '{PrimaryColor}'", request.BrandSettings.PrimaryColor);
            _logger.LogInformation("Ruc recibido: '{Ruc}'", request.BrandSettings.Ruc);
            _logger.LogInformation("Address recibido: '{Address}'", request.BrandSettings.Address);
            _logger.LogInformation("Phone recibido: '{Phone}'", request.BrandSettings.Phone);
            _logger.LogInformation("Email recibido: '{Email}'", request.BrandSettings.Email);
            _logger.LogInformation("LogoUrl recibido: '{LogoUrl}'", request.BrandSettings.LogoUrl);
            _logger.LogInformation("PrimaryColor ANTES de actualizar: '{PrimaryColorBefore}'", brandSettings.PrimaryColor);
            
            // Actualizar configuración existente
            // IMPORTANTE: StoreName es requerido, asegurarse de que no esté vacío
            brandSettings.StoreName = !string.IsNullOrWhiteSpace(request.BrandSettings.StoreName) 
                ? request.BrandSettings.StoreName.Trim() 
                : brandSettings.StoreName; // Mantener el valor existente si viene vacío
            // LogoUrl es requerido en la base de datos, no puede ser null
            // Si viene vacío, mantener el valor existente o usar string vacío
            brandSettings.LogoUrl = !string.IsNullOrWhiteSpace(request.BrandSettings.LogoUrl) 
                ? request.BrandSettings.LogoUrl.Trim() 
                : (brandSettings.LogoUrl ?? string.Empty); // Mantener valor existente o usar string vacío
            brandSettings.LogoEmoji = string.IsNullOrWhiteSpace(request.BrandSettings.LogoEmoji) ? null : request.BrandSettings.LogoEmoji;
            brandSettings.FaviconUrl = string.IsNullOrWhiteSpace(request.BrandSettings.FaviconUrl) ? null : request.BrandSettings.FaviconUrl;
            brandSettings.PrimaryColor = !string.IsNullOrWhiteSpace(request.BrandSettings.PrimaryColor) 
                ? request.BrandSettings.PrimaryColor 
                : brandSettings.PrimaryColor; // Mantener el valor existente si viene vacío
            
            _logger.LogInformation("PrimaryColor DESPUÉS de actualizar: '{PrimaryColorAfter}'", brandSettings.PrimaryColor);
            brandSettings.SecondaryColor = request.BrandSettings.SecondaryColor;
            brandSettings.ButtonColor = request.BrandSettings.ButtonColor;
            brandSettings.TextColor = request.BrandSettings.TextColor;
            brandSettings.HoverColor = request.BrandSettings.HoverColor;
            brandSettings.Description = string.IsNullOrWhiteSpace(request.BrandSettings.Description) ? null : request.BrandSettings.Description;
            brandSettings.Slogan = string.IsNullOrWhiteSpace(request.BrandSettings.Slogan) ? null : request.BrandSettings.Slogan;
            // Convertir strings vacíos y null a null para campos opcionales
            // IMPORTANTE: Guardar el valor tal cual viene si no está vacío, para que persista en la base de datos
            brandSettings.Phone = string.IsNullOrWhiteSpace(request.BrandSettings.Phone) ? null : request.BrandSettings.Phone.Trim();
            brandSettings.WhatsAppPhone = string.IsNullOrWhiteSpace(request.BrandSettings.WhatsAppPhone) ? null : request.BrandSettings.WhatsAppPhone.Trim();
            brandSettings.Email = string.IsNullOrWhiteSpace(request.BrandSettings.Email) ? null : request.BrandSettings.Email.Trim();
            brandSettings.Address = string.IsNullOrWhiteSpace(request.BrandSettings.Address) ? null : request.BrandSettings.Address.Trim();
            brandSettings.Ruc = string.IsNullOrWhiteSpace(request.BrandSettings.Ruc) ? null : request.BrandSettings.Ruc.Trim();
            brandSettings.YapePhone = string.IsNullOrWhiteSpace(request.BrandSettings.YapePhone) ? null : request.BrandSettings.YapePhone.Trim();
            brandSettings.PlinPhone = string.IsNullOrWhiteSpace(request.BrandSettings.PlinPhone) ? null : request.BrandSettings.PlinPhone.Trim();
            brandSettings.YapeQRUrl = string.IsNullOrWhiteSpace(request.BrandSettings.YapeQRUrl) ? null : request.BrandSettings.YapeQRUrl.Trim();
            brandSettings.PlinQRUrl = string.IsNullOrWhiteSpace(request.BrandSettings.PlinQRUrl) ? null : request.BrandSettings.PlinQRUrl.Trim();
            brandSettings.YapeEnabled = request.BrandSettings.YapeEnabled;
            brandSettings.PlinEnabled = request.BrandSettings.PlinEnabled;
            brandSettings.BankName = string.IsNullOrWhiteSpace(request.BrandSettings.BankName) ? null : request.BrandSettings.BankName.Trim();
            brandSettings.BankAccountType = string.IsNullOrWhiteSpace(request.BrandSettings.BankAccountType) ? null : request.BrandSettings.BankAccountType.Trim();
            brandSettings.BankAccountNumber = string.IsNullOrWhiteSpace(request.BrandSettings.BankAccountNumber) ? null : request.BrandSettings.BankAccountNumber.Trim();
            brandSettings.BankCCI = string.IsNullOrWhiteSpace(request.BrandSettings.BankCCI) ? null : request.BrandSettings.BankCCI.Trim();
            brandSettings.BankAccountVisible = request.BrandSettings.BankAccountVisible;
            brandSettings.DeliveryType = request.BrandSettings.DeliveryType ?? "Ambos";
            brandSettings.DeliveryCost = request.BrandSettings.DeliveryCost;
            brandSettings.DeliveryZones = string.IsNullOrWhiteSpace(request.BrandSettings.DeliveryZones) ? null : request.BrandSettings.DeliveryZones.Trim();
            brandSettings.HomeTitle = string.IsNullOrWhiteSpace(request.BrandSettings.HomeTitle) ? null : request.BrandSettings.HomeTitle.Trim();
            brandSettings.HomeSubtitle = string.IsNullOrWhiteSpace(request.BrandSettings.HomeSubtitle) ? null : request.BrandSettings.HomeSubtitle.Trim();
            brandSettings.HomeDescription = string.IsNullOrWhiteSpace(request.BrandSettings.HomeDescription) ? null : request.BrandSettings.HomeDescription.Trim();
            brandSettings.HomeBannerImageUrl = string.IsNullOrWhiteSpace(request.BrandSettings.HomeBannerImageUrl) ? null : request.BrandSettings.HomeBannerImageUrl.Trim();
            brandSettings.UpdatedBy = request.UpdatedBy;

            await _unitOfWork.BrandSettings.UpdateAsync(brandSettings, cancellationToken);
            
            // Log después de actualizar pero antes de guardar
            _logger.LogInformation("=== VALORES DESPUÉS DE ACTUALIZAR (antes de SaveChanges) ===");
            _logger.LogInformation("StoreName: '{StoreName}'", brandSettings.StoreName);
            _logger.LogInformation("PrimaryColor: '{PrimaryColor}'", brandSettings.PrimaryColor);
        }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Log después de guardar en la base de datos
            _logger.LogInformation("=== VALORES DESPUÉS DE SaveChanges ===");
            _logger.LogInformation("StoreName guardado: '{StoreName}' (IsNullOrWhiteSpace: {IsEmpty})", 
                brandSettings.StoreName, 
                string.IsNullOrWhiteSpace(brandSettings.StoreName));
            _logger.LogInformation("PrimaryColor guardado: '{PrimaryColor}'", brandSettings.PrimaryColor);
            _logger.LogInformation("Ruc guardado: '{Ruc}' (IsNullOrWhiteSpace: {IsEmpty})", 
                brandSettings.Ruc ?? "NULL", 
                string.IsNullOrWhiteSpace(brandSettings.Ruc));
            _logger.LogInformation("Address guardado: '{Address}' (IsNullOrWhiteSpace: {IsEmpty})", 
                brandSettings.Address ?? "NULL", 
                string.IsNullOrWhiteSpace(brandSettings.Address));
            _logger.LogInformation("Phone guardado: '{Phone}' (IsNullOrWhiteSpace: {IsEmpty})", 
                brandSettings.Phone ?? "NULL", 
                string.IsNullOrWhiteSpace(brandSettings.Phone));
            _logger.LogInformation("Email guardado: '{Email}' (IsNullOrWhiteSpace: {IsEmpty})", 
                brandSettings.Email ?? "NULL", 
                string.IsNullOrWhiteSpace(brandSettings.Email));
            _logger.LogInformation("LogoUrl guardado: '{LogoUrl}' (IsNullOrWhiteSpace: {IsEmpty})", 
                brandSettings.LogoUrl ?? "NULL", 
                string.IsNullOrWhiteSpace(brandSettings.LogoUrl));

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

        return Result<BrandSettingsDto>.Success(dto);
    }
}


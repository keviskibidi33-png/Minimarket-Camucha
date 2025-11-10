using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Commands;

public class AdminSetupCommandHandler : IRequestHandler<AdminSetupCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<AdminSetupCommandHandler> _logger;

    public AdminSetupCommandHandler(
        IUnitOfWork unitOfWork,
        UserManager<IdentityUser<Guid>> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<AdminSetupCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(AdminSetupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Actualizar o crear BrandSettings
            var brandSettings = await _unitOfWork.BrandSettings.FirstOrDefaultAsync(bs => true, cancellationToken);
            if (brandSettings == null)
            {
                brandSettings = new Domain.Entities.BrandSettings
                {
                    StoreName = request.StoreName,
                    LogoUrl = request.LogoUrl ?? string.Empty,
                    FaviconUrl = request.FaviconUrl,
                    PrimaryColor = request.PrimaryColor,
                    SecondaryColor = request.SecondaryColor,
                    ButtonColor = request.PrimaryColor,
                    TextColor = "#333333",
                    HoverColor = request.PrimaryColor,
                    Description = request.Description,
                    Slogan = request.Slogan,
                    Phone = request.Phone,
                    Email = request.Email,
                    Ruc = request.Ruc,
                    // Información de Pago
                    YapePhone = request.YapePhone,
                    PlinPhone = request.PlinPhone,
                    YapeQRUrl = request.YapeQRUrl,
                    PlinQRUrl = request.PlinQRUrl,
                    YapeEnabled = !string.IsNullOrEmpty(request.YapePhone) || !string.IsNullOrEmpty(request.YapeQRUrl),
                    PlinEnabled = !string.IsNullOrEmpty(request.PlinPhone) || !string.IsNullOrEmpty(request.PlinQRUrl),
                    // Cuenta bancaria
                    BankName = request.BankName,
                    BankAccountType = request.BankAccountType,
                    BankAccountNumber = request.BankAccountNumber,
                    BankCCI = request.BankCCI,
                    BankAccountVisible = !string.IsNullOrEmpty(request.BankAccountNumber),
                    // Opciones de envío
                    DeliveryType = request.DeliveryType ?? "Ambos",
                    DeliveryCost = request.DeliveryCost,
                    DeliveryZones = request.DeliveryZones,
                    // Personalización de página
                    HomeTitle = request.HomeTitle,
                    HomeSubtitle = request.HomeSubtitle,
                    HomeDescription = request.HomeDescription,
                    HomeBannerImageUrl = request.HomeBannerImageUrl,
                    UpdatedBy = request.UserId
                };
                await _unitOfWork.BrandSettings.AddAsync(brandSettings, cancellationToken);
            }
            else
            {
                brandSettings.StoreName = request.StoreName;
                if (!string.IsNullOrEmpty(request.LogoUrl))
                    brandSettings.LogoUrl = request.LogoUrl;
                if (!string.IsNullOrEmpty(request.FaviconUrl))
                    brandSettings.FaviconUrl = request.FaviconUrl;
                brandSettings.PrimaryColor = request.PrimaryColor;
                brandSettings.SecondaryColor = request.SecondaryColor;
                brandSettings.ButtonColor = request.PrimaryColor;
                brandSettings.HoverColor = request.PrimaryColor;
                brandSettings.Description = request.Description;
                brandSettings.Slogan = request.Slogan;
                brandSettings.Phone = request.Phone;
                brandSettings.Email = request.Email;
                brandSettings.Ruc = request.Ruc;
                // Información de Pago
                brandSettings.YapePhone = request.YapePhone;
                brandSettings.PlinPhone = request.PlinPhone;
                if (!string.IsNullOrEmpty(request.YapeQRUrl))
                    brandSettings.YapeQRUrl = request.YapeQRUrl;
                if (!string.IsNullOrEmpty(request.PlinQRUrl))
                    brandSettings.PlinQRUrl = request.PlinQRUrl;
                brandSettings.YapeEnabled = !string.IsNullOrEmpty(request.YapePhone) || !string.IsNullOrEmpty(request.YapeQRUrl);
                brandSettings.PlinEnabled = !string.IsNullOrEmpty(request.PlinPhone) || !string.IsNullOrEmpty(request.PlinQRUrl);
                // Cuenta bancaria
                brandSettings.BankName = request.BankName;
                brandSettings.BankAccountType = request.BankAccountType;
                brandSettings.BankAccountNumber = request.BankAccountNumber;
                brandSettings.BankCCI = request.BankCCI;
                brandSettings.BankAccountVisible = !string.IsNullOrEmpty(request.BankAccountNumber);
                // Opciones de envío
                brandSettings.DeliveryType = request.DeliveryType ?? "Ambos";
                brandSettings.DeliveryCost = request.DeliveryCost;
                brandSettings.DeliveryZones = request.DeliveryZones;
                // Personalización de página
                brandSettings.HomeTitle = request.HomeTitle;
                brandSettings.HomeSubtitle = request.HomeSubtitle;
                brandSettings.HomeDescription = request.HomeDescription;
                brandSettings.HomeBannerImageUrl = request.HomeBannerImageUrl;
                brandSettings.UpdatedBy = request.UserId;
                await _unitOfWork.BrandSettings.UpdateAsync(brandSettings, cancellationToken);
            }

            // 2. Crear Sede si no es virtual
            if (!request.IsVirtual && !string.IsNullOrEmpty(request.SedeAddress) && !string.IsNullOrEmpty(request.SedeCity))
            {
                var sede = new Sede
                {
                    Nombre = request.StoreName + " - Sede Principal",
                    Direccion = request.SedeAddress,
                    Ciudad = request.SedeCity,
                    Pais = "Perú",
                    Latitud = 0, // Se puede mejorar con geocoding
                    Longitud = 0,
                    Telefono = request.SedePhone,
                    HorariosJson = "{}", // Horarios por defecto
                    Estado = true
                };
                await _unitOfWork.Sedes.AddAsync(sede, cancellationToken);
            }

            // 3. Crear categorías si no existen
            foreach (var categoryName in request.Categories)
            {
                var existingCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(
                    c => c.Name == categoryName, cancellationToken);
                
                if (existingCategory == null)
                {
                    var category = new Category
                    {
                        Name = categoryName,
                        Description = $"Categoría {categoryName}",
                        IsActive = true
                    };
                    await _unitOfWork.Categories.AddAsync(category, cancellationToken);
                }
            }

            // 4. Crear usuario cajero si se solicita
            if (request.CreateCashier && 
                !string.IsNullOrEmpty(request.CashierEmail) && 
                !string.IsNullOrEmpty(request.CashierPassword) &&
                !string.IsNullOrEmpty(request.CashierDni))
            {
                // Verificar si el DNI ya existe
                var existingProfile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
                    up => up.Dni == request.CashierDni, cancellationToken);
                
                if (existingProfile == null)
                {
                    var cashier = new IdentityUser<Guid>
                    {
                        UserName = request.CashierDni,
                        Email = request.CashierEmail,
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(cashier, request.CashierPassword);
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(cashier, "Cajero");

                        // Crear perfil del cajero
                        var cashierProfile = new UserProfile
                        {
                            UserId = cashier.Id,
                            FirstName = request.CashierFirstName,
                            LastName = request.CashierLastName,
                            Dni = request.CashierDni,
                            Phone = request.Phone,
                            ProfileCompleted = true
                        };
                        await _unitOfWork.UserProfiles.AddAsync(cashierProfile, cancellationToken);

                        // Asignar permisos por defecto al rol Cajero
                        await AssignDefaultCashierPermissionsAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("Error al crear usuario cajero: {Errors}", 
                            string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            // 5. Marcar perfil del admin como completo
            var adminProfile = await _unitOfWork.UserProfiles.FirstOrDefaultAsync(
                up => up.UserId == request.UserId, cancellationToken);
            
            if (adminProfile != null)
            {
                adminProfile.ProfileCompleted = true;
                await _unitOfWork.UserProfiles.UpdateAsync(adminProfile, cancellationToken);
            }
            else
            {
                // Crear perfil si no existe
                adminProfile = new UserProfile
                {
                    UserId = request.UserId,
                    ProfileCompleted = true
                };
                await _unitOfWork.UserProfiles.AddAsync(adminProfile, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success("Configuración inicial completada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar la configuración inicial del admin");
            return Result<string>.Failure("Error al guardar la configuración: " + ex.Message);
        }
    }

    /// <summary>
    /// Asigna permisos por defecto al rol Cajero si no existen
    /// </summary>
    private async Task AssignDefaultCashierPermissionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cajeroRole = await _roleManager.FindByNameAsync("Cajero");
            if (cajeroRole == null)
            {
                _logger.LogWarning("Rol 'Cajero' no encontrado, no se pueden asignar permisos");
                return;
            }

            // Módulos y permisos por defecto para Cajero
            var defaultPermissions = new Dictionary<string, (bool View, bool Create, bool Edit, bool Delete)>
            {
                { "ventas", (View: true, Create: true, Edit: false, Delete: false) },      // Ver y crear ventas
                { "productos", (View: true, Create: false, Edit: false, Delete: false) },  // Solo ver productos
                { "clientes", (View: true, Create: true, Edit: true, Delete: false) },     // Ver, crear y editar clientes
            };

            foreach (var (moduleSlug, permissions) in defaultPermissions)
            {
                // Buscar el módulo
                var module = await _unitOfWork.Modules.FirstOrDefaultAsync(
                    m => m.Slug == moduleSlug && m.IsActive, cancellationToken);

                if (module == null)
                {
                    _logger.LogWarning("Módulo '{ModuleSlug}' no encontrado", moduleSlug);
                    continue;
                }

                // Verificar si ya existe el permiso
                var existingPermission = await _unitOfWork.RolePermissions.FirstOrDefaultAsync(
                    rp => rp.RoleId == cajeroRole.Id && rp.ModuleId == module.Id, cancellationToken);

                if (existingPermission == null)
                {
                    // Crear permiso por defecto
                    var rolePermission = new RolePermission
                    {
                        RoleId = cajeroRole.Id,
                        ModuleId = module.Id,
                        CanView = permissions.View,
                        CanCreate = permissions.Create,
                        CanEdit = permissions.Edit,
                        CanDelete = permissions.Delete
                    };
                    await _unitOfWork.RolePermissions.AddAsync(rolePermission, cancellationToken);
                    _logger.LogInformation("Permiso asignado: Rol=Cajero, Módulo={ModuleSlug}, View={View}, Create={Create}, Edit={Edit}, Delete={Delete}",
                        moduleSlug, permissions.View, permissions.Create, permissions.Edit, permissions.Delete);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar permisos por defecto al rol Cajero");
            // No fallar el proceso completo si falla la asignación de permisos
        }
    }
}


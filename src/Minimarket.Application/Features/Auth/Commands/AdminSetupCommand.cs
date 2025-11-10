using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Auth.Commands;

public class AdminSetupCommand : IRequest<Result<string>>
{
    public Guid UserId { get; set; }
    
    // Información básica
    public string StoreName { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WhatSells { get; set; }
    public bool IsVirtual { get; set; }
    
    // Sede (si no es virtual)
    public string? SedeAddress { get; set; }
    public string? SedeCity { get; set; }
    public string? SedeRegion { get; set; }
    public string? SedePhone { get; set; }
    
    // Branding
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string PrimaryColor { get; set; } = "#4CAF50";
    public string SecondaryColor { get; set; } = "#0d7ff2";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Ruc { get; set; }
    public string? Slogan { get; set; }
    
    // Configuración
    public List<string> Categories { get; set; } = new();
    public string SystemUsers { get; set; } = "1-5";
    
    // Personalización de página
    public string? HomeTitle { get; set; }
    public string? HomeSubtitle { get; set; }
    public string? HomeDescription { get; set; }
    public string? HomeBannerImageUrl { get; set; }
    
    // Información de Pago y Envío
    public string? YapePhone { get; set; }
    public string? PlinPhone { get; set; }
    public string? YapeQRUrl { get; set; }
    public string? PlinQRUrl { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountType { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankCCI { get; set; }
    public string DeliveryType { get; set; } = "Ambos"; // "SoloRecogida" | "SoloEnvio" | "Ambos"
    public decimal? DeliveryCost { get; set; }
    public string? DeliveryZones { get; set; }
    
    // Usuario cajero
    public bool CreateCashier { get; set; }
    public string? CashierEmail { get; set; }
    public string? CashierPassword { get; set; }
    public string? CashierFirstName { get; set; }
    public string? CashierLastName { get; set; }
    public string? CashierDni { get; set; }
}


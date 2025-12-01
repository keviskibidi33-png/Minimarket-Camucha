using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Minimarket.Infrastructure.Services;

public class PdfService : IPdfService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PdfService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public PdfService(IUnitOfWork unitOfWork, ILogger<PdfService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Descarga una imagen desde una URL HTTP/HTTPS y la guarda temporalmente en el sistema de archivos
    /// </summary>
    private async Task<string?> DownloadImageToTempFileAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            // Si es una ruta local, retornarla directamente
            if (!imageUrl.StartsWith("http://") && !imageUrl.StartsWith("https://") && !imageUrl.StartsWith("data:"))
            {
                // Verificar si el archivo existe localmente (ruta absoluta)
                if (Path.IsPathRooted(imageUrl) && System.IO.File.Exists(imageUrl))
                {
                    _logger.LogInformation("Usando ruta local absoluta: {ImageUrl}", imageUrl);
                    return imageUrl;
                }
                
                // Si no existe localmente, construir la ruta completa desde wwwroot
                var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var normalizedPath = imageUrl.Replace("\\", "/").TrimStart('/');
                var localPath = Path.Combine(wwwrootPath, normalizedPath);
                
                if (System.IO.File.Exists(localPath))
                {
                    _logger.LogInformation("Imagen encontrada en wwwroot: {LocalPath}", localPath);
                    return localPath;
                }
                
                _logger.LogWarning("No se encontró la imagen en ruta local: {ImageUrl}", imageUrl);
            }

            // Si es una URL HTTP/HTTPS, descargarla
            if (imageUrl.StartsWith("http://") || imageUrl.StartsWith("https://"))
            {
                _logger.LogInformation("Descargando imagen desde URL: {ImageUrl}", imageUrl);
                
                var response = await _httpClient.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();
                
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                
                // Guardar en un archivo temporal
                var tempFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageUrl).Split('?')[0]}";
                var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);
                
                await System.IO.File.WriteAllBytesAsync(tempFilePath, imageBytes);
                
                _logger.LogInformation("Imagen descargada y guardada en: {TempFilePath}", tempFilePath);
                return tempFilePath;
            }

            // Si es data URI, convertirla a archivo temporal
            if (imageUrl.StartsWith("data:"))
            {
                _logger.LogInformation("Procesando imagen data URI");
                
                var base64Data = imageUrl.Split(',')[1];
                var imageBytes = Convert.FromBase64String(base64Data);
                
                // Determinar extensión desde el MIME type
                var mimeType = imageUrl.Split(';')[0].Split(':')[1];
                var extension = mimeType switch
                {
                    "image/png" => ".png",
                    "image/jpeg" => ".jpg",
                    "image/jpg" => ".jpg",
                    "image/gif" => ".gif",
                    "image/webp" => ".webp",
                    _ => ".png"
                };
                
                var tempFileName = $"{Guid.NewGuid()}{extension}";
                var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);
                
                await System.IO.File.WriteAllBytesAsync(tempFilePath, imageBytes);
                
                _logger.LogInformation("Imagen data URI guardada en: {TempFilePath}", tempFilePath);
                return tempFilePath;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar imagen desde: {ImageUrl}", imageUrl);
            return null;
        }
    }

    public async Task<string> GenerateSaleReceiptAsync(Guid saleId, string documentType)
    {
        try
        {
            // Validar que el tipo de documento sea válido
            if (documentType != "Boleta" && documentType != "Factura")
            {
                _logger.LogError("Tipo de documento inválido: {DocumentType}", documentType);
                throw new ArgumentException($"Tipo de documento inválido: {documentType}. Debe ser 'Boleta' o 'Factura'");
            }

            // Validar que la plantilla esté activa
            var templateKey = documentType == "Factura" 
                ? "document_factura_template_active" 
                : "document_boleta_template_active";
            
            var templateSetting = (await _unitOfWork.SystemSettings.FindAsync(
                s => s.Key == templateKey))
                .FirstOrDefault();
            
            var isTemplateActive = templateSetting == null || 
                templateSetting.Value?.ToLower() != "false";
            
            if (!isTemplateActive)
            {
                _logger.LogError("Template {TemplateType} is not active. Cannot generate PDF for sale {SaleId}", 
                    documentType, saleId);
                throw new InvalidOperationException($"La plantilla de {documentType} no está activa. Por favor, active la plantilla en Configuraciones.");
            }

            _logger.LogInformation("Generando {DocumentType} para venta {SaleId}", documentType, saleId);

            var sale = await _unitOfWork.Sales.GetByIdAsync(saleId);
            if (sale == null)
            {
                _logger.LogWarning("Venta no encontrada: {SaleId}", saleId);
                throw new Exception("Venta no encontrada");
            }

            // Validar que el tipo de documento de la venta coincida con el solicitado
            var expectedDocumentType = documentType == "Factura" ? DocumentType.Factura : DocumentType.Boleta;
            if (sale.DocumentType != expectedDocumentType)
            {
                _logger.LogWarning("Tipo de documento de la venta ({SaleType}) no coincide con el solicitado ({RequestType})", 
                    sale.DocumentType, documentType);
                throw new Exception($"El tipo de documento de la venta ({sale.DocumentType}) no coincide con el solicitado ({documentType})");
            }

            // Optimizar consultas: cargar detalles y productos en una sola consulta
            var saleDetails = (await _unitOfWork.SaleDetails.FindAsync(sd => sd.SaleId == sale.Id)).ToList();
            
            // Validar que hay detalles (protección contra ventas vacías)
            if (saleDetails == null || !saleDetails.Any())
            {
                _logger.LogWarning("Sale {SaleId} has no details. Cannot generate PDF.", saleId);
                throw new InvalidOperationException("La venta no tiene detalles. No se puede generar el documento.");
            }

            // Validar límite de items para prevenir problemas de memoria (500 items máximo)
            if (saleDetails.Count > 500)
            {
                _logger.LogWarning("Sale {SaleId} has {ItemCount} items (max 500). PDF generation may fail due to memory constraints.", 
                    saleId, saleDetails.Count);
                // Continuar pero con advertencia
            }

            var customer = sale.CustomerId.HasValue 
                ? await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId.Value) 
                : null;

            // Obtener productos para mapear nombres y códigos (optimizado: una sola consulta)
            var productIds = saleDetails.Select(sd => sd.ProductId).Distinct().ToList();
            var products = (await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id)))
                .ToDictionary(p => p.Id, p => new { p.Name, p.Code });

            static IContainer CellStyle(IContainer container)
            {
                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
            }

            // Obtener BrandSettings desde la base de datos
            var brandSettingsList = await _unitOfWork.BrandSettings.GetAllAsync();
            var brandSettings = brandSettingsList.FirstOrDefault();
            
            // Log detallado para debugging
            _logger.LogInformation("=== INICIO GENERACIÓN PDF ===");
            _logger.LogInformation("SaleId: {SaleId}, DocumentType: {DocumentType}", saleId, documentType);
            _logger.LogInformation("Total BrandSettings encontrados: {Count}", brandSettingsList?.Count() ?? 0);
            
            if (brandSettings == null)
            {
                _logger.LogWarning("⚠️ No se encontraron BrandSettings en la base de datos, usando valores por defecto");
            }
            else
            {
                _logger.LogInformation("✅ BrandSettings encontrado:");
                _logger.LogInformation("  - StoreName: '{StoreName}' (IsNullOrWhiteSpace: {IsEmpty})", 
                    brandSettings.StoreName ?? "NULL", 
                    string.IsNullOrWhiteSpace(brandSettings.StoreName));
                _logger.LogInformation("  - LogoUrl: '{LogoUrl}' (IsNullOrWhiteSpace: {IsEmpty})", 
                    brandSettings.LogoUrl ?? "NULL", 
                    string.IsNullOrWhiteSpace(brandSettings.LogoUrl));
                _logger.LogInformation("  - PrimaryColor: '{PrimaryColor}' (IsNullOrWhiteSpace: {IsEmpty})", 
                    brandSettings.PrimaryColor ?? "NULL", 
                    string.IsNullOrWhiteSpace(brandSettings.PrimaryColor));
                _logger.LogInformation("  - Ruc: '{Ruc}' (IsNullOrWhiteSpace: {IsEmpty})", 
                    brandSettings.Ruc ?? "NULL", 
                    string.IsNullOrWhiteSpace(brandSettings.Ruc));
                _logger.LogInformation("  - Address: '{Address}' (IsNullOrWhiteSpace: {IsEmpty})", 
                    brandSettings.Address ?? "NULL", 
                    string.IsNullOrWhiteSpace(brandSettings.Address));
                _logger.LogInformation("  - Phone: '{Phone}' (IsNullOrWhiteSpace: {IsEmpty})", 
                    brandSettings.Phone ?? "NULL", 
                    string.IsNullOrWhiteSpace(brandSettings.Phone));
                _logger.LogInformation("  - Email: '{Email}' (IsNullOrWhiteSpace: {IsEmpty})", 
                    brandSettings.Email ?? "NULL", 
                    string.IsNullOrWhiteSpace(brandSettings.Email));
            }
            
            // Usar valores de BrandSettings o fallback a configuración/appsettings
            // IMPORTANTE: Verificar tanto null como string vacío para usar valores por defecto
            var companyName = !string.IsNullOrWhiteSpace(brandSettings?.StoreName) 
                ? brandSettings.StoreName 
                : (!string.IsNullOrWhiteSpace(_configuration["Company:Name"]) 
                    ? _configuration["Company:Name"] 
                    : "Minimarket Camucha");
            var companyRuc = !string.IsNullOrWhiteSpace(brandSettings?.Ruc) 
                ? brandSettings.Ruc 
                : (!string.IsNullOrWhiteSpace(_configuration["Company:Ruc"]) 
                    ? _configuration["Company:Ruc"] 
                    : "10095190559");
            var companyAddress = !string.IsNullOrWhiteSpace(brandSettings?.Address) 
                ? brandSettings.Address 
                : (!string.IsNullOrWhiteSpace(_configuration["Company:Address"]) 
                    ? _configuration["Company:Address"] 
                    : "Jr. Pedro Labarthe 449 – Ingeniería, San Martín de Porres, Lima, Lima, Perú");
            var companyPhone = !string.IsNullOrWhiteSpace(brandSettings?.Phone) 
                ? brandSettings.Phone 
                : (!string.IsNullOrWhiteSpace(_configuration["Company:Phone"]) 
                    ? _configuration["Company:Phone"] 
                    : "+51 999 999 999");
            var companyEmail = !string.IsNullOrWhiteSpace(brandSettings?.Email) 
                ? brandSettings.Email 
                : (!string.IsNullOrWhiteSpace(_configuration["Company:Email"]) 
                    ? _configuration["Company:Email"] 
                    : "");
            // Siempre usar el logo de assets por defecto
            var logoUrl = "/assets/logo.png";
            // Usar el color primario de BrandSettings, o un azul por defecto si no existe
            var primaryColor = !string.IsNullOrWhiteSpace(brandSettings?.PrimaryColor) 
                ? brandSettings.PrimaryColor 
                : "#4A90E2"; // Azul por defecto (mismo que en el formulario)
            // TextColor siempre será negro en diseño minimalista
            var textColor = "#000000";
            
            // Log detallado de valores que se usarán
            _logger.LogInformation("=== VALORES FINALES PARA PDF ===");
            _logger.LogInformation("LogoUrl: '{LogoUrl}' (viene de BrandSettings: {FromDb})", 
                logoUrl, 
                !string.IsNullOrWhiteSpace(brandSettings?.LogoUrl) ? "SÍ" : "NO (usando vacío)");
            _logger.LogInformation("PrimaryColor desde BrandSettings: '{PrimaryColorFromDb}'", brandSettings?.PrimaryColor ?? "NULL");
            _logger.LogInformation("PrimaryColor que se usará: '{PrimaryColorFinal}'", primaryColor);
            _logger.LogInformation("TextColor: '{TextColor}'", textColor);
            _logger.LogInformation("CompanyName: '{CompanyName}' (viene de BrandSettings: {FromDb}, valor en DB: '{DbValue}')", 
                companyName, 
                !string.IsNullOrWhiteSpace(brandSettings?.StoreName) ? "SÍ" : "NO (usando default)",
                brandSettings?.StoreName ?? "NULL");
            _logger.LogInformation("CompanyRuc: '{CompanyRuc}' (viene de BrandSettings: {FromDb}, valor en DB: '{DbValue}')", 
                companyRuc, 
                !string.IsNullOrWhiteSpace(brandSettings?.Ruc) ? "SÍ" : "NO (usando default)",
                brandSettings?.Ruc ?? "NULL");
            _logger.LogInformation("CompanyAddress: '{CompanyAddress}' (viene de BrandSettings: {FromDb}, valor en DB: '{DbValue}')", 
                companyAddress, 
                !string.IsNullOrWhiteSpace(brandSettings?.Address) ? "SÍ" : "NO (usando default)",
                brandSettings?.Address ?? "NULL");
            _logger.LogInformation("CompanyPhone: '{CompanyPhone}' (viene de BrandSettings: {FromDb}, valor en DB: '{DbValue}')", 
                companyPhone, 
                !string.IsNullOrWhiteSpace(brandSettings?.Phone) ? "SÍ" : "NO (usando default)",
                brandSettings?.Phone ?? "NULL");
            _logger.LogInformation("BrandSettings completo - PrimaryColor: {PrimaryColor}, TextColor: {TextColor}, StoreName: {StoreName}", 
                brandSettings?.PrimaryColor ?? "NULL", brandSettings?.TextColor ?? "NULL", brandSettings?.StoreName ?? "NULL");

            // Convertir color hexadecimal a QuestPDF Color
            // Mapeo de colores comunes a colores predefinidos de QuestPDF para mejor rendimiento
            QuestPDF.Infrastructure.Color ParseColor(string hexColor, bool isTextColor = false)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(hexColor))
                    {
                        // Para textColor, usar negro por defecto; para primaryColor, usar verde
                        hexColor = isTextColor ? "#000000" : primaryColor;
                    }
                    
                    // Normalizar el formato
                    hexColor = hexColor.Trim().ToUpper();
                    if (!hexColor.StartsWith("#"))
                    {
                        hexColor = "#" + hexColor;
                    }
                    
                    // Mapeo rápido de colores comunes
                    var colorMap = new Dictionary<string, QuestPDF.Infrastructure.Color>
                    {
                        { "#4CAF50", Colors.Green.Medium }, // Verde Material Design
                        { "#4A90E2", Colors.Blue.Medium }, // Azul corporativo (color por defecto del formulario)
                        { "#2196F3", Colors.Blue.Medium }, // Azul Material Design
                        { "#FF9800", Colors.Orange.Medium }, // Naranja Material Design
                        { "#F44336", Colors.Red.Medium }, // Rojo Material Design
                        { "#9C27B0", Colors.Purple.Medium }, // Morado Material Design
                        { "#333333", Colors.Grey.Darken3 }, // Gris oscuro
                        { "#111827", Colors.Black }, // Gris muy oscuro (casi negro) - usado comúnmente en Tailwind
                        { "#000000", Colors.Black }, // Negro
                        { "#FFFFFF", Colors.White }, // Blanco
                    };
                    
                    if (colorMap.TryGetValue(hexColor, out var mappedColor))
                    {
                        return mappedColor;
                    }
                    
                    // Para colores personalizados, parsear el hex y crear el color usando Reflection
                    hexColor = hexColor.TrimStart('#');
                    if (hexColor.Length == 6)
                    {
                        var r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                        var g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                        var b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                        
                        // Normalizar a valores entre 0 y 1
                        var rNorm = r / 255f;
                        var gNorm = g / 255f;
                        var bNorm = b / 255f;
                        
                        // Intentar crear el color usando el constructor con Reflection
                        var colorType = typeof(QuestPDF.Infrastructure.Color);
                        var constructor = colorType.GetConstructor(
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                            null,
                            new[] { typeof(float), typeof(float), typeof(float) },
                            null);
                        
                        if (constructor != null)
                        {
                            return (QuestPDF.Infrastructure.Color)constructor.Invoke(new object[] { rNorm, gNorm, bNorm });
                        }
                        
                        // Si el constructor no funciona, intentar con propiedades usando System.Drawing
                        var systemColor = ColorTranslator.FromHtml("#" + hexColor);
                        // Usar el color más cercano de QuestPDF basado en el RGB
                        // Calcular distancia euclidiana a colores predefinidos
                        // Para textColor, preferir colores oscuros (negro/gris)
                        // Para primaryColor, usar azul por defecto (mismo que en formulario)
                        var closestColor = isTextColor ? Colors.Black : Colors.Blue.Medium; // Por defecto
                        var minDistance = double.MaxValue;
                        
                        var predefinedColors = new[]
                        {
                            (Colors.Green.Medium, 76, 175, 80),
                            (Colors.Blue.Medium, 33, 150, 243),
                            (Colors.Orange.Medium, 255, 152, 0),
                            (Colors.Red.Medium, 244, 67, 54),
                            (Colors.Purple.Medium, 156, 39, 176),
                            (Colors.Black, 0, 0, 0), // Agregar negro para textColor
                            (Colors.Grey.Darken3, 51, 51, 51), // Agregar gris oscuro para textColor
                        };
                        
                        foreach (var (color, pr, pg, pb) in predefinedColors)
                        {
                            var distance = Math.Sqrt(
                                Math.Pow(systemColor.R - pr, 2) +
                                Math.Pow(systemColor.G - pg, 2) +
                                Math.Pow(systemColor.B - pb, 2));
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestColor = color;
                            }
                        }
                        
                        _logger.LogWarning("No se pudo crear color personalizado, usando color más cercano: {ClosestColor}", closestColor);
                        return closestColor;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing color {HexColor}, using default. Error: {Error}", hexColor, ex.Message);
                }
                
                // Fallback: usar negro para textColor, azul para primaryColor (mismo que formulario)
                var fallbackColor = isTextColor ? Colors.Black : Colors.Blue.Medium;
                _logger.LogWarning("Usando color por defecto como fallback: {FallbackColor}", fallbackColor);
                return fallbackColor;
            }

            var primaryColorParsed = ParseColor(primaryColor, false);
            var textColorParsed = ParseColor(textColor, true);
            
            // Log de colores parseados para debugging
            _logger.LogInformation("=== COLORES PARSEADOS ===");
            _logger.LogInformation("PrimaryColor hex recibido: '{PrimaryColorHex}'", primaryColor);
            _logger.LogInformation("PrimaryColor parseado: {PrimaryColorParsed}", primaryColorParsed);
            _logger.LogInformation("TextColor hex: '{TextColorHex}'", textColor);
            _logger.LogInformation("TextColor parseado: {TextColorParsed}", textColorParsed);
            _logger.LogInformation("TextColor desde BrandSettings: {TextColorFromDb}", brandSettings?.TextColor ?? "NULL");
            _logger.LogInformation("TextColor final usado: {TextColorFinal}", textColor);

            // Descargar el logo antes de crear el documento si es necesario
            string? localLogoPath = null;
            if (!string.IsNullOrWhiteSpace(logoUrl))
            {
                try
                {
                    // Si es /assets/logo.png, buscar primero en wwwroot local
                    if (logoUrl.StartsWith("/assets/"))
                    {
                        var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var normalizedPath = logoUrl.Replace("\\", "/").TrimStart('/');
                        var localPath = Path.Combine(wwwrootPath, normalizedPath);
                        
                        if (System.IO.File.Exists(localPath))
                        {
                            _logger.LogInformation("✅ Logo encontrado localmente en: {LocalPath}", localPath);
                            localLogoPath = localPath;
                        }
                        else
                        {
                            // Si no existe localmente, intentar descargar desde el frontend
                            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
                            var normalizedLogoPath = logoUrl.Replace("\\", "/").TrimStart('/');
                            var fullLogoUrl = $"{frontendUrl.TrimEnd('/')}/{normalizedLogoPath}";
                            _logger.LogInformation("Logo no encontrado localmente, descargando desde: {LogoUrl}", fullLogoUrl);
                            localLogoPath = await DownloadImageToTempFileAsync(fullLogoUrl);
                        }
                    }
                    else
                    {
                        // Construir la URL completa si es necesario
                        var fullLogoUrl = logoUrl;
                        if (!logoUrl.StartsWith("http://") && !logoUrl.StartsWith("https://") && !logoUrl.StartsWith("data:") && !Path.IsPathRooted(logoUrl))
                        {
                            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
                            var normalizedLogoPath = logoUrl.Replace("\\", "/").TrimStart('/');
                            fullLogoUrl = $"{baseUrl.TrimEnd('/')}/{normalizedLogoPath}";
                        }
                        
                        _logger.LogInformation("Descargando logo desde: {LogoUrl}", fullLogoUrl);
                        localLogoPath = await DownloadImageToTempFileAsync(fullLogoUrl);
                    }
                    
                    if (localLogoPath != null && System.IO.File.Exists(localLogoPath))
                    {
                        _logger.LogInformation("✅ Logo descargado exitosamente a: {LocalLogoPath}", localLogoPath);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ No se pudo descargar el logo desde: {LogoUrl}", logoUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error al descargar el logo desde: {LogoUrl}. Error: {ErrorMessage}", logoUrl, ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ LogoUrl está vacío, no se mostrará logo");
            }

            var pdfPath = Path.Combine(
                Path.GetTempPath(),
                $"{(documentType == "Factura" ? "F" : "B")}_{sale.DocumentNumber.Replace("-", "_")}.pdf"
            );

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                    page.Header()
                        .PaddingBottom(15)
                        .Column(headerCol =>
                        {
                            // Línea decorativa superior (3px) - único acento de color
                            headerCol.Item().BorderTop(3).BorderColor(primaryColorParsed).PaddingTop(15);
                            
                            // Encabezado: Logo y datos empresa (izquierda) | RUC y Número (derecha)
                            headerCol.Item().Row(row =>
                            {
                                // Logo y datos empresa (izquierda)
                                row.RelativeItem().Column(leftCol =>
                                {
                                    // Logo (máximo 60px de altura) - ARRIBA del nombre de la empresa
                                    if (localLogoPath != null && System.IO.File.Exists(localLogoPath))
                                    {
                                        try
                                        {
                                            // Logo arriba del nombre, con altura máxima de 60px
                                            leftCol.Item().Height(60).Image(localLogoPath).FitArea();
                                            _logger.LogInformation("✅ Logo cargado exitosamente en PDF desde: {LocalLogoPath}", localLogoPath);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError(ex, "❌ Error al cargar el logo en PDF desde: {LocalLogoPath}. Error: {ErrorMessage}", localLogoPath, ex.Message);
                                            // Si falla el logo, continuar sin logo pero loguear el error
                                        }
                                    }
                                    
                                    // NOMBRE DE LA EMPRESA - SIEMPRE DEBE MOSTRARSE (después del logo)
                                    // Asegurar que siempre se muestre, incluso si está vacío (usar valor por defecto)
                                    var displayName = !string.IsNullOrWhiteSpace(companyName) 
                                        ? companyName 
                                        : "Minimarket Camucha";
                                    
                                    _logger.LogInformation("Mostrando nombre de empresa: '{DisplayName}'", displayName);
                                    
                                    // Si hay logo, agregar padding antes del nombre; si no, mostrar directamente
                                    if (localLogoPath != null && System.IO.File.Exists(localLogoPath))
                                    {
                                        // Si hay logo, agregar padding antes del nombre (el logo está arriba)
                                        leftCol.Item().PaddingTop(8).Text(displayName)
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor(Colors.Black);
                                    }
                                    else
                                    {
                                        // Si no hay logo, mostrar el nombre directamente
                                        leftCol.Item().Text(displayName)
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor(Colors.Black);
                                    }
                                    
                                    // Datos de contacto (gris #666, 9px) - DESPUÉS del nombre
                                    leftCol.Item().PaddingTop(3).Column(contactCol =>
                                    {
                                        if (!string.IsNullOrWhiteSpace(companyAddress))
                                        {
                                            contactCol.Item().Text(companyAddress)
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Medium);
                                        }
                                        if (!string.IsNullOrWhiteSpace(companyPhone))
                                        {
                                            contactCol.Item().PaddingTop(1).Text($"Tel: {companyPhone}")
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Medium);
                                        }
                                        if (!string.IsNullOrWhiteSpace(companyEmail))
                                        {
                                            contactCol.Item().PaddingTop(1).Text(companyEmail)
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Medium);
                                        }
                                    });
                                });
                                
                                // RUC y Número de Boleta (derecha, alineado)
                                row.RelativeItem().Column(rightCol =>
                                {
                                    rightCol.Item().AlignRight();
                                    
                                    // RUC (gris #666, 9px)
                                    if (!string.IsNullOrWhiteSpace(companyRuc))
                                    {
                                        rightCol.Item().Text($"RUC: {companyRuc}")
                                            .FontSize(9)
                                            .FontColor(Colors.Grey.Medium)
                                            .AlignRight();
                                    }
                                    
                                    // Título del documento (negro, bold, 12px)
                                    rightCol.Item().PaddingTop(5).Text(documentType == "Factura" ? "FACTURA" : "BOLETA DE VENTA")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor(Colors.Black)
                                        .AlignRight();
                                    
                                    // Número de documento (gris #666, 10px)
                                    rightCol.Item().PaddingTop(3).Text($"N° {sale.DocumentNumber}")
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Medium)
                                        .AlignRight();
                                });
                            });
                        });

                    page.Content()
                        .PaddingVertical(15)
                        .Column(column =>
                        {
                            // Información de la venta: Fecha (izquierda) y Cliente (derecha)
                            column.Item().PaddingBottom(10).Row(row =>
                            {
                                // Fecha (izquierda, negro, 9px)
                                row.RelativeItem().Column(leftCol =>
                                {
                                    leftCol.Item().Text($"Fecha: {sale.SaleDate:dd/MM/yyyy HH:mm}")
                                        .FontSize(9)
                                        .FontColor(Colors.Black);
                                });
                                
                                // Cliente (derecha, negro, 9px)
                                row.RelativeItem().Column(rightCol =>
                                {
                                    rightCol.Item().AlignRight();
                                    if (customer != null)
                                    {
                                        rightCol.Item().Text($"Cliente: {customer.Name}")
                                            .FontSize(9)
                                            .FontColor(Colors.Black)
                                            .AlignRight();
                                        if (!string.IsNullOrWhiteSpace(customer.DocumentType) && !string.IsNullOrWhiteSpace(customer.DocumentNumber))
                                        {
                                            rightCol.Item().PaddingTop(2).Text($"{customer.DocumentType}: {customer.DocumentNumber}")
                                                .FontSize(9)
                                                .FontColor(Colors.Black)
                                                .AlignRight();
                                        }
                                    }
                                    else if (documentType == "Factura")
                                    {
                                        rightCol.Item().Text("Cliente: No especificado")
                                            .FontSize(9)
                                            .FontColor(Colors.Black)
                                            .AlignRight();
                                    }
                                    else
                                    {
                                        rightCol.Item().Text("Cliente: Público General")
                                            .FontSize(9)
                                            .FontColor(Colors.Black)
                                            .AlignRight();
                                    }
                                });
                            });

                            // Tabla de productos - Minimalista: solo líneas horizontales grises tenues
                            column.Item().PaddingBottom(10).Table(table =>
                            {
                                // Definir columnas: 50%, 15%, 17.5%, 17.5%
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3.33f); // Producto (50%)
                                    columns.RelativeColumn(1);     // Cantidad (15%)
                                    columns.RelativeColumn(1.17f); // P. Unit (17.5%)
                                    columns.RelativeColumn(1.17f); // Subtotal (17.5%)
                                });

                                // Header minimalista: texto negro, bold, sin fondo
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .Text("Producto")
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                    
                                    header.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignCenter()
                                        .Text("Cant.")
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                    
                                    header.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignRight()
                                        .Text("P. Unit")
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                    
                                    header.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignRight()
                                        .Text("Subtotal")
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                });

                                // Items
                                foreach (var detail in saleDetails)
                                {
                                    var productName = products.TryGetValue(detail.ProductId, out var product) 
                                        ? product.Name 
                                        : "Producto eliminado";
                                    var productCode = products.TryGetValue(detail.ProductId, out var productInfo) 
                                        ? productInfo.Code 
                                        : "";

                                    // Producto: nombre (negro, 9px) y código (gris, 8px)
                                    table.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .Column(productCol =>
                                        {
                                            productCol.Item().Text(productName)
                                                .FontSize(9)
                                                .FontColor(Colors.Black)
                                                .SemiBold();
                                            if (!string.IsNullOrWhiteSpace(productCode))
                                            {
                                                productCol.Item().PaddingTop(1).Text($"({productCode})")
                                                    .FontSize(8)
                                                    .FontColor(Colors.Grey.Medium);
                                            }
                                        });
                                    
                                    // Cantidad (centrada, negro, 9px)
                                    table.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignCenter()
                                        .Text(detail.Quantity.ToString())
                                        .FontSize(9)
                                        .FontColor(Colors.Black);
                                    
                                    // Precio unitario (derecha, negro, 9px)
                                    table.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignRight()
                                        .Text($"S/ {detail.UnitPrice:F2}")
                                        .FontSize(9)
                                        .FontColor(Colors.Black);
                                    
                                    // Subtotal (derecha, negro, 9px)
                                    table.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignRight()
                                        .Text($"S/ {detail.Subtotal:F2}")
                                        .FontSize(9)
                                        .FontColor(Colors.Black);
                                }
                            });

                            // Totales (alineados a la derecha, minimalista)
                            column.Item().PaddingBottom(10).AlignRight().Column(totals =>
                            {
                                totals.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("Subtotal:").FontSize(9).FontColor(Colors.Black);
                                    row.ConstantItem(80).Text($"S/ {sale.Subtotal:F2}").FontSize(9).AlignRight().FontColor(Colors.Black);
                                });

                                if (sale.Discount > 0)
                                {
                                    totals.Item().PaddingTop(3).Row(row =>
                                    {
                                        row.ConstantItem(100).Text("Descuento:").FontSize(9).FontColor(Colors.Black);
                                        row.ConstantItem(80).Text($"S/ {sale.Discount:F2}").FontSize(9).AlignRight().FontColor(Colors.Black);
                                    });
                                }

                                totals.Item().PaddingTop(3).Row(row =>
                                {
                                    row.ConstantItem(100).Text("IGV (18%):").FontSize(9).Bold().FontColor(Colors.Black);
                                    row.ConstantItem(80).Text($"S/ {sale.Tax:F2}").FontSize(9).Bold().AlignRight().FontColor(Colors.Black);
                                });

                                // TOTAL con línea separadora superior
                                totals.Item().PaddingTop(8).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(8).Row(row =>
                                {
                                    row.ConstantItem(100).Text("TOTAL:").FontSize(11).Bold().FontColor(Colors.Black);
                                    row.ConstantItem(80).Text($"S/ {sale.Total:F2}").FontSize(11).Bold().AlignRight().FontColor(Colors.Black);
                                });
                            });

                            // Información de pago (minimalista, con línea separadora superior)
                            column.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).Column(payment =>
                            {
                                payment.Item().PaddingBottom(5).Text("INFORMACIÓN DE PAGO")
                                    .FontSize(9)
                                    .Bold()
                                    .FontColor(Colors.Grey.Medium);
                                
                                payment.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(paymentCol =>
                                    {
                                        paymentCol.Item().Text("Método de pago:").FontSize(9).FontColor(Colors.Grey.Medium);
                                        paymentCol.Item().PaddingTop(2).Text(GetPaymentMethodText(sale.PaymentMethod))
                                            .FontSize(9)
                                            .FontColor(Colors.Black)
                                            .SemiBold();
                                    });
                                    row.RelativeItem().Column(paymentCol =>
                                    {
                                        paymentCol.Item().Text("Monto pagado:").FontSize(9).FontColor(Colors.Grey.Medium);
                                        paymentCol.Item().PaddingTop(2).Text($"S/ {sale.AmountPaid:F2}")
                                            .FontSize(9)
                                            .FontColor(Colors.Black)
                                            .SemiBold();
                                    });
                                });
                                
                                if (sale.Change > 0)
                                {
                                    payment.Item().PaddingTop(5).Row(row =>
                                    {
                                        row.RelativeItem().Column(paymentCol =>
                                        {
                                            paymentCol.Item().Text("Vuelto:").FontSize(9).FontColor(Colors.Grey.Medium);
                                            paymentCol.Item().PaddingTop(2).Text($"S/ {sale.Change:F2}")
                                                .FontSize(9)
                                                .FontColor(Colors.Black)
                                                .SemiBold();
                                        });
                                    });
                                }
                            });
                        });

                    page.Footer()
                        .BorderTop(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .PaddingTop(20)
                        .AlignCenter()
                        .Column(footerCol =>
                        {
                            // Nombre de la empresa
                            var footerCompanyName = !string.IsNullOrWhiteSpace(companyName) 
                                ? companyName 
                                : "Minimarket Camucha";
                            footerCol.Item().Text(footerCompanyName).FontSize(10).Bold().FontColor(Colors.Black);
                            
                            // Información de contacto
                            var footerInfo = new List<string>();
                            if (!string.IsNullOrWhiteSpace(companyAddress))
                            {
                                footerInfo.Add(companyAddress);
                            }
                            if (!string.IsNullOrWhiteSpace(companyPhone))
                            {
                                footerInfo.Add($"Tel: {companyPhone}");
                            }
                            if (!string.IsNullOrWhiteSpace(companyEmail))
                            {
                                footerInfo.Add(companyEmail);
                            }
                            
                            if (footerInfo.Any())
                            {
                                footerCol.Item().PaddingTop(3).Text(string.Join(" | ", footerInfo))
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Medium);
                            }
                            
                            footerCol.Item().PaddingTop(5).Text("Gracias por su compra")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Medium);
                        });
                });
            });

            document.GeneratePdf(pdfPath);

            _logger.LogInformation("✅ PDF generated successfully: {Path}", pdfPath);
            _logger.LogInformation("=== FIN GENERACIÓN PDF ===");
            return pdfPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for sale {SaleId}", saleId);
            throw;
        }
    }

    public async Task<string> GenerateWebOrderReceiptAsync(Guid orderId, string documentType = "Boleta")
    {
        try
        {
            _logger.LogInformation("Generando PDF de pedido web. OrderId: {OrderId}, DocumentType: {DocumentType}", orderId, documentType);

            var order = await _unitOfWork.WebOrders.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception($"Pedido con ID {orderId} no encontrado");
            }

            // Cargar items del pedido
            var orderItems = (await _unitOfWork.WebOrderItems.FindAsync(oi => oi.WebOrderId == orderId)).ToList();
            if (orderItems == null || !orderItems.Any())
            {
                throw new InvalidOperationException("El pedido no tiene items. No se puede generar el documento.");
            }

            // Obtener BrandSettings
            var brandSettingsList = await _unitOfWork.BrandSettings.GetAllAsync();
            var brandSettings = brandSettingsList.FirstOrDefault();

            var companyName = !string.IsNullOrWhiteSpace(brandSettings?.StoreName)
                ? brandSettings.StoreName
                : (!string.IsNullOrWhiteSpace(_configuration["Company:Name"])
                    ? _configuration["Company:Name"]
                    : "Minimarket Camucha");
            var companyRuc = !string.IsNullOrWhiteSpace(brandSettings?.Ruc)
                ? brandSettings.Ruc
                : (!string.IsNullOrWhiteSpace(_configuration["Company:Ruc"])
                    ? _configuration["Company:Ruc"]
                    : "10095190559");
            var companyAddress = !string.IsNullOrWhiteSpace(brandSettings?.Address)
                ? brandSettings.Address
                : (!string.IsNullOrWhiteSpace(_configuration["Company:Address"])
                    ? _configuration["Company:Address"]
                    : "Jr. Pedro Labarthe 449 – Ingeniería, San Martín de Porres, Lima, Lima, Perú");
            var companyPhone = !string.IsNullOrWhiteSpace(brandSettings?.Phone)
                ? brandSettings.Phone
                : (!string.IsNullOrWhiteSpace(_configuration["Company:Phone"])
                    ? _configuration["Company:Phone"]
                    : "+51 999 999 999");
            var companyEmail = !string.IsNullOrWhiteSpace(brandSettings?.Email)
                ? brandSettings.Email
                : (!string.IsNullOrWhiteSpace(_configuration["Company:Email"])
                    ? _configuration["Company:Email"]
                    : "");

            var logoUrl = "/assets/logo.png";
            var primaryColor = !string.IsNullOrWhiteSpace(brandSettings?.PrimaryColor)
                ? brandSettings.PrimaryColor
                : "#4A90E2";

            // Parsear colores (reutilizar lógica similar a GenerateSaleReceiptAsync)
            QuestPDF.Infrastructure.Color ParseColor(string hexColor)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(hexColor))
                        return Colors.Blue.Medium;

                    hexColor = hexColor.Trim().ToUpper();
                    if (!hexColor.StartsWith("#"))
                        hexColor = "#" + hexColor;

                    var colorMap = new Dictionary<string, QuestPDF.Infrastructure.Color>
                    {
                        { "#4CAF50", Colors.Green.Medium },
                        { "#4A90E2", Colors.Blue.Medium },
                        { "#2196F3", Colors.Blue.Medium },
                        { "#FF9800", Colors.Orange.Medium },
                        { "#F44336", Colors.Red.Medium },
                        { "#9C27B0", Colors.Purple.Medium },
                        { "#333333", Colors.Grey.Darken3 },
                        { "#111827", Colors.Black },
                        { "#000000", Colors.Black },
                        { "#FFFFFF", Colors.White },
                    };

                    if (colorMap.TryGetValue(hexColor, out var mappedColor))
                        return mappedColor;

                    hexColor = hexColor.TrimStart('#');
                    if (hexColor.Length == 6)
                    {
                        var r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                        var g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                        var b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                        
                        // Normalizar a valores entre 0 y 1
                        var rNorm = r / 255f;
                        var gNorm = g / 255f;
                        var bNorm = b / 255f;
                        
                        // Crear el color usando el constructor con Reflection
                        var colorType = typeof(QuestPDF.Infrastructure.Color);
                        var constructor = colorType.GetConstructor(
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                            null,
                            new[] { typeof(float), typeof(float), typeof(float) },
                            null);
                        
                        if (constructor != null)
                        {
                            return (QuestPDF.Infrastructure.Color)constructor.Invoke(new object[] { rNorm, gNorm, bNorm });
                        }
                    }
                }
                catch { }
                return Colors.Blue.Medium;
            }

            var primaryColorParsed = ParseColor(primaryColor);

            // Descargar logo
            string? localLogoPath = null;
            if (!string.IsNullOrWhiteSpace(logoUrl))
            {
                try
                {
                    if (logoUrl.StartsWith("/assets/"))
                    {
                        var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var normalizedPath = logoUrl.Replace("\\", "/").TrimStart('/');
                        var localPath = Path.Combine(wwwrootPath, normalizedPath);

                        if (System.IO.File.Exists(localPath))
                        {
                            localLogoPath = localPath;
                        }
                        else
                        {
                            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
                            var normalizedLogoPath = logoUrl.Replace("\\", "/").TrimStart('/');
                            var fullLogoUrl = $"{frontendUrl.TrimEnd('/')}/{normalizedLogoPath}";
                            localLogoPath = await DownloadImageToTempFileAsync(fullLogoUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al descargar el logo");
                }
            }

            var pdfPath = Path.Combine(
                Path.GetTempPath(),
                $"{(documentType == "Factura" ? "F" : "B")}_{order.OrderNumber.Replace("-", "_")}.pdf"
            );

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                    page.Header()
                        .PaddingBottom(15)
                        .Column(headerCol =>
                        {
                            headerCol.Item().BorderTop(3).BorderColor(primaryColorParsed).PaddingTop(15);

                            headerCol.Item().Row(row =>
                            {
                                // Logo y datos empresa (izquierda)
                                row.RelativeItem().Column(leftCol =>
                                {
                                    if (localLogoPath != null && System.IO.File.Exists(localLogoPath))
                                    {
                                        try
                                        {
                                            leftCol.Item().Height(60).Image(localLogoPath).FitArea();
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError(ex, "Error al cargar el logo en PDF");
                                        }
                                    }

                                    var displayName = !string.IsNullOrWhiteSpace(companyName) ? companyName : "Minimarket Camucha";
                                    if (localLogoPath != null && System.IO.File.Exists(localLogoPath))
                                    {
                                        leftCol.Item().PaddingTop(8).Text(displayName)
                                            .FontSize(14).Bold().FontColor(Colors.Black);
                                    }
                                    else
                                    {
                                        leftCol.Item().Text(displayName)
                                            .FontSize(14).Bold().FontColor(Colors.Black);
                                    }

                                    leftCol.Item().PaddingTop(3).Column(contactCol =>
                                    {
                                        if (!string.IsNullOrWhiteSpace(companyAddress))
                                        {
                                            contactCol.Item().Text(companyAddress)
                                                .FontSize(9).FontColor(Colors.Grey.Medium);
                                        }
                                        if (!string.IsNullOrWhiteSpace(companyPhone))
                                        {
                                            contactCol.Item().PaddingTop(1).Text($"Tel: {companyPhone}")
                                                .FontSize(9).FontColor(Colors.Grey.Medium);
                                        }
                                        if (!string.IsNullOrWhiteSpace(companyEmail))
                                        {
                                            contactCol.Item().PaddingTop(1).Text(companyEmail)
                                                .FontSize(9).FontColor(Colors.Grey.Medium);
                                        }
                                    });
                                });

                                // RUC y Número (derecha)
                                row.RelativeItem().Column(rightCol =>
                                {
                                    rightCol.Item().AlignRight();

                                    if (!string.IsNullOrWhiteSpace(companyRuc))
                                    {
                                        rightCol.Item().Text($"RUC: {companyRuc}")
                                            .FontSize(9).FontColor(Colors.Grey.Medium).AlignRight();
                                    }

                                    rightCol.Item().PaddingTop(5).Text(documentType.ToUpper())
                                        .FontSize(16).Bold().FontColor(Colors.Black).AlignRight();

                                    rightCol.Item().Text($"N° {order.OrderNumber}")
                                        .FontSize(12).Bold().FontColor(Colors.Black).AlignRight();
                                });
                            });
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            // Datos del cliente
                            column.Item().Column(clientCol =>
                            {
                                clientCol.Item().PaddingBottom(10).Text("DATOS DEL CLIENTE")
                                    .FontSize(9).Bold().FontColor(Colors.Grey.Medium);

                                clientCol.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(infoCol =>
                                    {
                                        infoCol.Item().Text($"Nombre: {order.CustomerName}")
                                            .FontSize(9).FontColor(Colors.Black);
                                        if (!string.IsNullOrWhiteSpace(order.CustomerEmail))
                                        {
                                            infoCol.Item().PaddingTop(2).Text($"Email: {order.CustomerEmail}")
                                                .FontSize(9).FontColor(Colors.Black);
                                        }
                                        if (!string.IsNullOrWhiteSpace(order.CustomerPhone))
                                        {
                                            infoCol.Item().PaddingTop(2).Text($"Teléfono: {order.CustomerPhone}")
                                                .FontSize(9).FontColor(Colors.Black);
                                        }
                                    });

                                    row.RelativeItem().Column(infoCol =>
                                    {
                                        infoCol.Item().Text($"Fecha: {order.CreatedAt:dd/MM/yyyy HH:mm}")
                                            .FontSize(9).FontColor(Colors.Black);
                                        infoCol.Item().PaddingTop(2).Text($"Método de envío: {(order.ShippingMethod == "delivery" ? "Delivery" : "Recojo en Tienda")}")
                                            .FontSize(9).FontColor(Colors.Black);
                                        if (!string.IsNullOrWhiteSpace(order.ShippingAddress))
                                        {
                                            infoCol.Item().PaddingTop(2).Text($"Dirección: {order.ShippingAddress}")
                                                .FontSize(9).FontColor(Colors.Black);
                                        }
                                    });
                                });
                            });

                            // Tabla de productos
                            column.Item().PaddingTop(15).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(60);
                                    columns.ConstantColumn(70);
                                    columns.ConstantColumn(80);
                                });

                                // Encabezado
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Producto").FontSize(9).Bold().FontColor(Colors.Black);
                                    header.Cell().Element(CellStyle).Text("Cant.").FontSize(9).Bold().FontColor(Colors.Black).AlignRight();
                                    header.Cell().Element(CellStyle).Text("P. Unit.").FontSize(9).Bold().FontColor(Colors.Black).AlignRight();
                                    header.Cell().Element(CellStyle).Text("Subtotal").FontSize(9).Bold().FontColor(Colors.Black).AlignRight();
                                });

                                // Items
                                foreach (var item in orderItems)
                                {
                                    table.Cell().Element(CellStyle).Text(item.ProductName).FontSize(9).FontColor(Colors.Black);
                                    table.Cell().Element(CellStyle).Text(item.Quantity.ToString()).FontSize(9).FontColor(Colors.Black).AlignRight();
                                    table.Cell().Element(CellStyle).Text($"S/ {item.UnitPrice:F2}").FontSize(9).FontColor(Colors.Black).AlignRight();
                                    table.Cell().Element(CellStyle).Text($"S/ {item.Subtotal:F2}").FontSize(9).FontColor(Colors.Black).AlignRight();
                                }
                            });

                            // Totales
                            column.Item().PaddingTop(15).Column(totals =>
                            {
                                totals.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("Subtotal:").FontSize(9).FontColor(Colors.Black);
                                    row.ConstantItem(80).Text($"S/ {order.Subtotal:F2}").FontSize(9).AlignRight().FontColor(Colors.Black);
                                });

                                totals.Item().PaddingTop(3).Row(row =>
                                {
                                    row.ConstantItem(100).Text("Envío:").FontSize(9).FontColor(Colors.Black);
                                    row.ConstantItem(80).Text($"S/ {order.ShippingCost:F2}").FontSize(9).AlignRight().FontColor(Colors.Black);
                                });

                                totals.Item().PaddingTop(8).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(8).Row(row =>
                                {
                                    row.ConstantItem(100).Text("TOTAL:").FontSize(11).Bold().FontColor(Colors.Black);
                                    row.ConstantItem(80).Text($"S/ {order.Total:F2}").FontSize(11).Bold().AlignRight().FontColor(Colors.Black);
                                });
                            });

                            // Información de pago
                            column.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).Column(payment =>
                            {
                                payment.Item().PaddingBottom(5).Text("INFORMACIÓN DE PAGO")
                                    .FontSize(9).Bold().FontColor(Colors.Grey.Medium);

                                payment.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(paymentCol =>
                                    {
                                        paymentCol.Item().Text("Método de pago:").FontSize(9).FontColor(Colors.Grey.Medium);
                                        paymentCol.Item().PaddingTop(2).Text(GetPaymentMethodTextFromString(order.PaymentMethod))
                                            .FontSize(9).FontColor(Colors.Black).SemiBold();
                                    });
                                });
                            });
                        });

                    page.Footer()
                        .BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(20).AlignCenter()
                        .Column(footerCol =>
                        {
                            var footerCompanyName = !string.IsNullOrWhiteSpace(companyName) ? companyName : "Minimarket Camucha";
                            footerCol.Item().Text(footerCompanyName).FontSize(10).Bold().FontColor(Colors.Black);

                            var footerInfo = new List<string>();
                            if (!string.IsNullOrWhiteSpace(companyAddress))
                                footerInfo.Add(companyAddress);
                            if (!string.IsNullOrWhiteSpace(companyPhone))
                                footerInfo.Add($"Tel: {companyPhone}");
                            if (!string.IsNullOrWhiteSpace(companyEmail))
                                footerInfo.Add(companyEmail);

                            if (footerInfo.Any())
                            {
                                footerCol.Item().PaddingTop(3).Text(string.Join(" | ", footerInfo))
                                    .FontSize(8).FontColor(Colors.Grey.Medium);
                            }

                            footerCol.Item().PaddingTop(5).Text("Gracias por su compra")
                                .FontSize(9).FontColor(Colors.Grey.Medium);
                        });

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                });
            });

            document.GeneratePdf(pdfPath);

            _logger.LogInformation("✅ PDF de pedido generado exitosamente: {Path}", pdfPath);
            return pdfPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<string> GeneratePreviewPdfAsync(string documentType, Dictionary<string, string>? customSettings = null)
    {
        try
        {
            // Validar que el tipo de documento sea válido
            if (documentType != "Boleta" && documentType != "Factura")
            {
                _logger.LogError("Tipo de documento inválido: {DocumentType}", documentType);
                throw new ArgumentException($"Tipo de documento inválido: {documentType}. Debe ser 'Boleta' o 'Factura'");
            }

            _logger.LogInformation("Generando PDF de prueba para {DocumentType}", documentType);

            // Log de customSettings recibidos
            if (customSettings != null && customSettings.Any())
            {
                _logger.LogInformation("=== CUSTOM SETTINGS RECIBIDOS ===");
                foreach (var setting in customSettings)
                {
                    _logger.LogInformation("  - {Key}: {Value}", setting.Key, setting.Value);
                }
            }
            else
            {
                _logger.LogInformation("No se recibieron customSettings, usando BrandSettings");
            }

            // Obtener BrandSettings desde la base de datos
            var brandSettingsList = await _unitOfWork.BrandSettings.GetAllAsync();
            var brandSettings = brandSettingsList.FirstOrDefault();
            
            // Si hay customSettings (del formulario), usarlos; si no, usar BrandSettings
            // Para colores, si están en customSettings (incluso si están vacíos), usarlos
            var companyName = customSettings?.ContainsKey("companyName") == true && !string.IsNullOrWhiteSpace(customSettings["companyName"])
                ? customSettings["companyName"]
                : brandSettings?.StoreName 
                ?? _configuration["Company:Name"] 
                ?? "Minimarket Camucha";
            var companyRuc = customSettings?.ContainsKey("companyRuc") == true && !string.IsNullOrWhiteSpace(customSettings["companyRuc"])
                ? customSettings["companyRuc"]
                : brandSettings?.Ruc 
                ?? _configuration["Company:Ruc"] 
                ?? "10095190559";
            var companyAddress = customSettings?.ContainsKey("companyAddress") == true && !string.IsNullOrWhiteSpace(customSettings["companyAddress"])
                ? customSettings["companyAddress"]
                : brandSettings?.Address 
                ?? _configuration["Company:Address"] 
                ?? "Jr. Pedro Labarthe 449 – Ingeniería, San Martín de Porres, Lima, Lima, Perú";
            var companyPhone = customSettings?.ContainsKey("companyPhone") == true && !string.IsNullOrWhiteSpace(customSettings["companyPhone"])
                ? customSettings["companyPhone"]
                : brandSettings?.Phone 
                ?? _configuration["Company:Phone"] 
                ?? "+51 999 999 999";
            var companyEmail = customSettings?.ContainsKey("companyEmail") == true && !string.IsNullOrWhiteSpace(customSettings["companyEmail"])
                ? customSettings["companyEmail"]
                : brandSettings?.Email 
                ?? _configuration["Company:Email"] 
                ?? "";
            // Siempre usar el logo de assets por defecto
            var logoUrl = customSettings?.ContainsKey("logoUrl") == true && !string.IsNullOrWhiteSpace(customSettings["logoUrl"])
                ? customSettings["logoUrl"]
                : "/assets/logo.png";
            
            // Para colores, si están en customSettings, usarlos (incluso si están vacíos, usar el valor del formulario)
            // IMPORTANTE: Si customSettings contiene la clave, siempre usar ese valor (incluso si está vacío)
            var primaryColor = customSettings?.ContainsKey("primaryColor") == true
                ? (string.IsNullOrWhiteSpace(customSettings["primaryColor"]) ? "#4A90E2" : customSettings["primaryColor"])
                : (!string.IsNullOrWhiteSpace(brandSettings?.PrimaryColor) ? brandSettings.PrimaryColor : "#4A90E2"); // Azul por defecto (mismo que formulario)
            // TextColor siempre será negro en diseño minimalista
            var textColor = "#000000";

            // Log de valores finales que se usarán
            _logger.LogInformation("=== VALORES FINALES PARA PDF ===");
            _logger.LogInformation("  - PrimaryColor: {PrimaryColor}", primaryColor);
            _logger.LogInformation("  - TextColor: {TextColor}", textColor);
            _logger.LogInformation("  - CompanyName: {CompanyName}", companyName);
            _logger.LogInformation("  - TextColor en customSettings: {TextColorInCustom}", 
                customSettings?.ContainsKey("textColor") == true ? customSettings["textColor"] : "NO PRESENTE");
            _logger.LogInformation("  - TextColor en BrandSettings: {TextColorInDb}", brandSettings?.TextColor ?? "NULL");

            // Convertir color hexadecimal a QuestPDF Color
            // Mapeo de colores comunes a colores predefinidos de QuestPDF para mejor rendimiento
            QuestPDF.Infrastructure.Color ParseColor(string hexColor, bool isTextColor = false)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(hexColor))
                    {
                        // Para textColor, usar negro por defecto; para primaryColor, usar verde
                        hexColor = isTextColor ? "#000000" : primaryColor;
                    }
                    
                    // Normalizar el formato
                    hexColor = hexColor.Trim().ToUpper();
                    if (!hexColor.StartsWith("#"))
                    {
                        hexColor = "#" + hexColor;
                    }
                    
                    _logger.LogInformation("Parseando color: {HexColor} (isTextColor: {IsTextColor})", hexColor, isTextColor);
                    
                    // Mapeo rápido de colores comunes
                    var colorMap = new Dictionary<string, QuestPDF.Infrastructure.Color>
                    {
                        { "#4CAF50", Colors.Green.Medium }, // Verde Material Design
                        { "#4A90E2", Colors.Blue.Medium }, // Azul corporativo (color por defecto del formulario)
                        { "#2196F3", Colors.Blue.Medium }, // Azul Material Design
                        { "#FF9800", Colors.Orange.Medium }, // Naranja Material Design
                        { "#F44336", Colors.Red.Medium }, // Rojo Material Design
                        { "#9C27B0", Colors.Purple.Medium }, // Morado Material Design
                        { "#333333", Colors.Grey.Darken3 }, // Gris oscuro
                        { "#111827", Colors.Black }, // Gris muy oscuro (casi negro) - usado comúnmente en Tailwind
                        { "#000000", Colors.Black }, // Negro
                        { "#FFFFFF", Colors.White }, // Blanco
                    };
                    
                    if (colorMap.TryGetValue(hexColor, out var mappedColor))
                    {
                        _logger.LogInformation("Color encontrado en mapa: {HexColor} -> {MappedColor}", hexColor, mappedColor);
                        return mappedColor;
                    }
                    
                    // Para colores personalizados, parsear el hex y crear el color usando Reflection
                    hexColor = hexColor.TrimStart('#');
                    if (hexColor.Length == 6)
                    {
                        var r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                        var g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                        var b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                        
                        _logger.LogInformation("Color personalizado RGB: R={R}, G={G}, B={B}", r, g, b);
                        
                        // Normalizar a valores entre 0 y 1
                        var rNorm = r / 255f;
                        var gNorm = g / 255f;
                        var bNorm = b / 255f;
                        
                        // Intentar crear el color usando el constructor con Reflection
                        var colorType = typeof(QuestPDF.Infrastructure.Color);
                        var constructor = colorType.GetConstructor(
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                            null,
                            new[] { typeof(float), typeof(float), typeof(float) },
                            null);
                        
                        if (constructor != null)
                        {
                            var customColor = (QuestPDF.Infrastructure.Color)constructor.Invoke(new object[] { rNorm, gNorm, bNorm });
                            _logger.LogInformation("Color personalizado creado exitosamente usando Reflection");
                            return customColor;
                        }
                        
                        // Si el constructor no funciona, intentar con propiedades usando System.Drawing
                        var systemColor = ColorTranslator.FromHtml("#" + hexColor);
                        // Usar el color más cercano de QuestPDF basado en el RGB
                        // Calcular distancia euclidiana a colores predefinidos
                        // Para textColor, preferir colores oscuros (negro/gris)
                        // Para primaryColor, usar azul por defecto (mismo que en formulario)
                        var closestColor = isTextColor ? Colors.Black : Colors.Blue.Medium; // Por defecto
                        var minDistance = double.MaxValue;
                        
                        var predefinedColors = new[]
                        {
                            (Colors.Green.Medium, 76, 175, 80),
                            (Colors.Blue.Medium, 33, 150, 243),
                            (Colors.Orange.Medium, 255, 152, 0),
                            (Colors.Red.Medium, 244, 67, 54),
                            (Colors.Purple.Medium, 156, 39, 176),
                            (Colors.Black, 0, 0, 0), // Agregar negro para textColor
                            (Colors.Grey.Darken3, 51, 51, 51), // Agregar gris oscuro para textColor
                        };
                        
                        foreach (var (color, pr, pg, pb) in predefinedColors)
                        {
                            var distance = Math.Sqrt(
                                Math.Pow(systemColor.R - pr, 2) +
                                Math.Pow(systemColor.G - pg, 2) +
                                Math.Pow(systemColor.B - pb, 2));
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestColor = color;
                            }
                        }
                        
                        _logger.LogWarning("No se pudo crear color personalizado, usando color más cercano: {ClosestColor}", closestColor);
                        return closestColor;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing color {HexColor}, using default. Error: {Error}", hexColor, ex.Message);
                }
                
                // Fallback: usar negro para textColor, azul para primaryColor (mismo que formulario)
                var fallbackColor = isTextColor ? Colors.Black : Colors.Blue.Medium;
                _logger.LogWarning("Usando color por defecto como fallback: {FallbackColor}", fallbackColor);
                return fallbackColor;
            }

            var primaryColorParsed = ParseColor(primaryColor, false);
            var textColorParsed = ParseColor(textColor, true);

            // Log de colores parseados para debugging
            _logger.LogInformation("TextColor desde BrandSettings: {TextColorFromDb}", brandSettings?.TextColor ?? "NULL");
            _logger.LogInformation("TextColor final usado: {TextColorFinal}", textColor);
            _logger.LogInformation("TextColorParsed creado exitosamente");

            // Crear datos de ejemplo para el preview
            var previewDocumentNumber = documentType == "Factura" ? "001-00000001" : "001-00000001";
            var previewDate = DateTime.Now;
            var previewCustomerName = "Cliente de Ejemplo";
            var previewCustomerDocumentType = "DNI";
            var previewCustomerDocumentNumber = "12345678";
            
            // Productos de ejemplo
            var previewProducts = new List<(string Name, string Code, int Quantity, decimal UnitPrice, decimal Subtotal)>
            {
                ("Producto de Ejemplo", "PROD001", 2, 10.00m, 20.00m),
                ("Otro Producto", "PROD002", 1, 15.00m, 15.00m)
            };
            
            var previewSubtotal = previewProducts.Sum(p => p.Subtotal);
            var previewTax = previewSubtotal * 0.18m;
            var previewTotal = previewSubtotal + previewTax;
            var previewAmountPaid = previewTotal;
            var previewPaymentMethod = PaymentMethod.Efectivo;

            var pdfPath = Path.Combine(
                Path.GetTempPath(),
                $"preview_{(documentType == "Factura" ? "F" : "B")}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            );

            static IContainer CellStyle(IContainer container)
            {
                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
            }

            // Descargar el logo antes de crear el documento si es necesario
            string? localLogoPath = null;
            if (!string.IsNullOrWhiteSpace(logoUrl))
            {
                try
                {
                    // Si es /assets/logo.png, buscar primero en wwwroot local
                    if (logoUrl.StartsWith("/assets/"))
                    {
                        var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var normalizedPath = logoUrl.Replace("\\", "/").TrimStart('/');
                        var localPath = Path.Combine(wwwrootPath, normalizedPath);
                        
                        if (System.IO.File.Exists(localPath))
                        {
                            _logger.LogInformation("✅ Logo encontrado localmente en: {LocalPath}", localPath);
                            localLogoPath = localPath;
                        }
                        else
                        {
                            // Si no existe localmente, intentar descargar desde el frontend
                            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
                            var normalizedLogoPath = logoUrl.Replace("\\", "/").TrimStart('/');
                            var fullLogoUrl = $"{frontendUrl.TrimEnd('/')}/{normalizedLogoPath}";
                            _logger.LogInformation("Logo no encontrado localmente, descargando desde: {LogoUrl}", fullLogoUrl);
                            localLogoPath = await DownloadImageToTempFileAsync(fullLogoUrl);
                        }
                    }
                    else
                    {
                        // Construir la URL completa si es necesario
                        var fullLogoUrl = logoUrl;
                        if (!logoUrl.StartsWith("http://") && !logoUrl.StartsWith("https://") && !logoUrl.StartsWith("data:") && !Path.IsPathRooted(logoUrl))
                        {
                            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
                            var normalizedLogoPath = logoUrl.Replace("\\", "/").TrimStart('/');
                            fullLogoUrl = $"{baseUrl.TrimEnd('/')}/{normalizedLogoPath}";
                        }
                        
                        _logger.LogInformation("Descargando logo desde: {LogoUrl}", fullLogoUrl);
                        localLogoPath = await DownloadImageToTempFileAsync(fullLogoUrl);
                    }
                    
                    if (localLogoPath != null && System.IO.File.Exists(localLogoPath))
                    {
                        _logger.LogInformation("✅ Logo descargado exitosamente a: {LocalLogoPath}", localLogoPath);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ No se pudo descargar el logo desde: {LogoUrl}", logoUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error al descargar el logo desde: {LogoUrl}. Error: {ErrorMessage}", logoUrl, ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ LogoUrl está vacío, no se mostrará logo");
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                    page.Header()
                        .PaddingBottom(15)
                        .Column(headerCol =>
                        {
                            // Línea decorativa superior (3px) - único acento de color
                            headerCol.Item().BorderTop(3).BorderColor(primaryColorParsed).PaddingTop(15);
                            
                            // Encabezado: Logo y datos empresa (izquierda) | RUC y Número (derecha)
                            headerCol.Item().Row(row =>
                            {
                                // Logo y datos empresa (izquierda)
                                row.RelativeItem().Column(leftCol =>
                                {
                                    // Logo (máximo 60px de altura) - ARRIBA del nombre de la empresa
                                    if (localLogoPath != null && System.IO.File.Exists(localLogoPath))
                                    {
                                        try
                                        {
                                            // Logo arriba del nombre, con altura máxima de 60px
                                            leftCol.Item().Height(60).Image(localLogoPath).FitArea();
                                            _logger.LogInformation("✅ Logo cargado exitosamente en PDF desde: {LocalLogoPath}", localLogoPath);
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError(ex, "❌ Error al cargar el logo en PDF desde: {LocalLogoPath}. Error: {ErrorMessage}", localLogoPath, ex.Message);
                                            // Si falla el logo, continuar sin logo pero loguear el error
                                        }
                                    }
                                    
                                    // NOMBRE DE LA EMPRESA - SIEMPRE DEBE MOSTRARSE (después del logo)
                                    // Asegurar que siempre se muestre, incluso si está vacío (usar valor por defecto)
                                    var displayName = !string.IsNullOrWhiteSpace(companyName) 
                                        ? companyName 
                                        : "Minimarket Camucha";
                                    
                                    // Si hay logo, agregar padding antes del nombre; si no, mostrar directamente
                                    if (!string.IsNullOrWhiteSpace(logoUrl))
                                    {
                                        // Si hay logo, agregar padding antes del nombre (el logo está arriba)
                                        leftCol.Item().PaddingTop(8).Text(displayName)
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor(Colors.Black);
                                    }
                                    else
                                    {
                                        // Si no hay logo, mostrar el nombre directamente
                                        leftCol.Item().Text(displayName)
                                            .FontSize(14)
                                            .Bold()
                                            .FontColor(Colors.Black);
                                    }
                                    
                                    // Datos de contacto (gris #666, 9px) - DESPUÉS del nombre
                                    leftCol.Item().PaddingTop(3).Column(contactCol =>
                                    {
                                        if (!string.IsNullOrWhiteSpace(companyAddress))
                                        {
                                            contactCol.Item().Text(companyAddress)
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Medium);
                                        }
                                        if (!string.IsNullOrWhiteSpace(companyPhone))
                                        {
                                            contactCol.Item().PaddingTop(1).Text($"Tel: {companyPhone}")
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Medium);
                                        }
                                        if (!string.IsNullOrWhiteSpace(companyEmail))
                                        {
                                            contactCol.Item().PaddingTop(1).Text(companyEmail)
                                                .FontSize(9)
                                                .FontColor(Colors.Grey.Medium);
                                        }
                                    });
                                });
                                
                                // RUC y Número de Boleta (derecha, alineado)
                                row.RelativeItem().Column(rightCol =>
                                {
                                    rightCol.Item().AlignRight();
                                    
                                    // RUC (gris #666, 9px)
                                    if (!string.IsNullOrWhiteSpace(companyRuc))
                                    {
                                        rightCol.Item().Text($"RUC: {companyRuc}")
                                            .FontSize(9)
                                            .FontColor(Colors.Grey.Medium)
                                            .AlignRight();
                                    }
                                    
                                    // Título del documento (negro, bold, 12px)
                                    rightCol.Item().PaddingTop(5).Text(documentType == "Factura" ? "FACTURA" : "BOLETA DE VENTA")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor(Colors.Black)
                                        .AlignRight();
                                    
                                    // Número de documento (gris #666, 10px)
                                    rightCol.Item().PaddingTop(3).Text($"N° {previewDocumentNumber}")
                                        .FontSize(10)
                                        .FontColor(Colors.Grey.Medium)
                                        .AlignRight();
                                });
                            });
                        });

                    page.Content()
                        .PaddingVertical(15)
                        .Column(column =>
                        {
                            // Información de la venta: Fecha (izquierda) y Cliente (derecha)
                            column.Item().PaddingBottom(10).Row(row =>
                            {
                                // Fecha (izquierda, negro, 9px)
                                row.RelativeItem().Column(leftCol =>
                                {
                                    leftCol.Item().Text($"Fecha: {previewDate:dd/MM/yyyy HH:mm}")
                                        .FontSize(9)
                                        .FontColor(Colors.Black);
                                });
                                
                                // Cliente (derecha, negro, 9px)
                                row.RelativeItem().Column(rightCol =>
                                {
                                    rightCol.Item().AlignRight();
                                    rightCol.Item().Text($"Cliente: {previewCustomerName}")
                                        .FontSize(9)
                                        .FontColor(Colors.Black)
                                        .AlignRight();
                                    rightCol.Item().PaddingTop(2).Text($"{previewCustomerDocumentType}: {previewCustomerDocumentNumber}")
                                        .FontSize(9)
                                        .FontColor(Colors.Black)
                                        .AlignRight();
                                });
                            });

                            // Tabla de productos - Minimalista: solo líneas horizontales grises tenues
                            column.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3.33f);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1.17f);
                                    columns.RelativeColumn(1.17f);
                                });

                                // Header minimalista: texto negro, bold, sin fondo
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .Text("Producto")
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                    
                                    header.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignCenter()
                                        .Text("Cant.")
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                    
                                    header.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignRight()
                                        .Text("P. Unit")
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                    
                                    header.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignRight()
                                        .Text("Subtotal")
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor(Colors.Black);
                                });

                                foreach (var product in previewProducts)
                                {
                                    // Producto: nombre (negro, 9px) y código (gris, 8px)
                                    table.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .Column(productCol =>
                                        {
                                            productCol.Item().Text(product.Name)
                                                .FontSize(9)
                                                .FontColor(Colors.Black)
                                                .SemiBold();
                                            if (!string.IsNullOrWhiteSpace(product.Code))
                                            {
                                                productCol.Item().PaddingTop(1).Text($"({product.Code})")
                                                    .FontSize(8)
                                                    .FontColor(Colors.Grey.Medium);
                                            }
                                        });
                                    
                                    // Cantidad (centrada, negro, 9px)
                                    table.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignCenter()
                                        .Text(product.Quantity.ToString())
                                        .FontSize(9)
                                        .FontColor(Colors.Black);
                                    
                                    // Precio unitario (derecha, negro, 9px)
                                    table.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignRight()
                                        .Text($"S/ {product.UnitPrice:F2}")
                                        .FontSize(9)
                                        .FontColor(Colors.Black);
                                    
                                    // Subtotal (derecha, negro, 9px)
                                    table.Cell().Element(CellStyle)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .PaddingVertical(8)
                                        .PaddingHorizontal(0)
                                        .AlignRight()
                                        .Text($"S/ {product.Subtotal:F2}")
                                        .FontSize(9)
                                        .FontColor(Colors.Black);
                                }
                            });

                            // Totales (alineados a la derecha, minimalista)
                            column.Item().PaddingBottom(10).AlignRight().Column(totals =>
                            {
                                totals.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("Subtotal:").FontSize(9).FontColor(Colors.Black);
                                    row.ConstantItem(80).Text($"S/ {previewSubtotal:F2}").FontSize(9).AlignRight().FontColor(Colors.Black);
                                });

                                totals.Item().PaddingTop(3).Row(row =>
                                {
                                    row.ConstantItem(100).Text("IGV (18%):").FontSize(9).Bold().FontColor(Colors.Black);
                                    row.ConstantItem(80).Text($"S/ {previewTax:F2}").FontSize(9).Bold().AlignRight().FontColor(Colors.Black);
                                });

                                // TOTAL con línea separadora superior
                                totals.Item().PaddingTop(8).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(8).Row(row =>
                                {
                                    row.ConstantItem(100).Text("TOTAL:").FontSize(11).Bold().FontColor(Colors.Black);
                                    row.ConstantItem(80).Text($"S/ {previewTotal:F2}").FontSize(11).Bold().AlignRight().FontColor(Colors.Black);
                                });
                            });

                            // Información de pago (minimalista, con línea separadora superior)
                            column.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).Column(payment =>
                            {
                                payment.Item().PaddingBottom(5).Text("INFORMACIÓN DE PAGO")
                                    .FontSize(9)
                                    .Bold()
                                    .FontColor(Colors.Grey.Medium);
                                
                                payment.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(paymentCol =>
                                    {
                                        paymentCol.Item().Text("Método de pago:").FontSize(9).FontColor(Colors.Grey.Medium);
                                        paymentCol.Item().PaddingTop(2).Text(GetPaymentMethodText(previewPaymentMethod))
                                            .FontSize(9)
                                            .FontColor(Colors.Black)
                                            .SemiBold();
                                    });
                                    row.RelativeItem().Column(paymentCol =>
                                    {
                                        paymentCol.Item().Text("Monto pagado:").FontSize(9).FontColor(Colors.Grey.Medium);
                                        paymentCol.Item().PaddingTop(2).Text($"S/ {previewAmountPaid:F2}")
                                            .FontSize(9)
                                            .FontColor(Colors.Black)
                                            .SemiBold();
                                    });
                                });
                            });
                        });

                    page.Footer()
                        .PaddingTop(20)
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Gracias por su compra").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                });
            });

            document.GeneratePdf(pdfPath);

            _logger.LogInformation("✅ PDF de prueba generado exitosamente: {Path}", pdfPath);
            return pdfPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating preview PDF for {DocumentType}", documentType);
            throw;
        }
    }

    private string GetPaymentMethodText(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Efectivo => "Efectivo",
            PaymentMethod.Tarjeta => "Tarjeta",
            PaymentMethod.YapePlin => "Yape/Plin",
            PaymentMethod.Transferencia => "Transferencia",
            _ => method.ToString()
        };
    }

    private string GetPaymentMethodTextFromString(string paymentMethod)
    {
        return paymentMethod?.ToLower() switch
        {
            "cash" => "Efectivo",
            "bank" => "Transferencia Bancaria",
            "wallet" => "Yape/Plin",
            "card" => "Tarjeta",
            _ => paymentMethod ?? "No especificado"
        };
    }

    public async Task<string> GenerateCashClosurePdfAsync(DateTime startDate, DateTime endDate, List<Domain.Entities.Sale> sales)
    {
        try
        {
            _logger.LogInformation("Generando PDF de cierre de caja desde {StartDate} hasta {EndDate} con {SaleCount} ventas", 
                startDate, endDate, sales.Count);

            // Obtener BrandSettings
            var brandSettingsList = await _unitOfWork.BrandSettings.GetAllAsync();
            var brandSettings = brandSettingsList.FirstOrDefault();
            
            var companyName = brandSettings?.StoreName 
                ?? _configuration["Company:Name"] 
                ?? "Minimarket Camucha";
            var companyRuc = brandSettings?.Ruc 
                ?? _configuration["Company:Ruc"] 
                ?? "";
            var companyAddress = brandSettings?.Address 
                ?? _configuration["Company:Address"] 
                ?? "";
            var companyPhone = brandSettings?.Phone 
                ?? _configuration["Company:Phone"] 
                ?? "";

            // Calcular totales
            var totalPaid = sales.Where(s => s.Status == SaleStatus.Pagado).Sum(s => s.Total);
            var totalCount = sales.Count(s => s.Status == SaleStatus.Pagado);
            
            // Agrupar por método de pago
            var paymentGroups = sales
                .Where(s => s.Status == SaleStatus.Pagado)
                .GroupBy(s => s.PaymentMethod)
                .Select(g => new
                {
                    Method = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(s => s.Total)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            // Descargar logo si existe
            string? localLogoPath = null;
            // Siempre usar el logo de assets
            var logoUrl = "/assets/logo.png";
            if (!string.IsNullOrWhiteSpace(logoUrl))
            {
                try
                {
                    // Si es /assets/logo.png, buscar primero en wwwroot local
                    if (logoUrl.StartsWith("/assets/"))
                    {
                        var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var normalizedPath = logoUrl.Replace("\\", "/").TrimStart('/');
                        var localPath = Path.Combine(wwwrootPath, normalizedPath);
                        
                        if (System.IO.File.Exists(localPath))
                        {
                            _logger.LogInformation("✅ Logo encontrado localmente en: {LocalPath}", localPath);
                            localLogoPath = localPath;
                        }
                        else
                        {
                            // Si no existe localmente, intentar descargar desde el frontend
                            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";
                            var normalizedLogoPath = logoUrl.Replace("\\", "/").TrimStart('/');
                            var fullLogoUrl = $"{frontendUrl.TrimEnd('/')}/{normalizedLogoPath}";
                            _logger.LogInformation("Logo no encontrado localmente, descargando desde: {LogoUrl}", fullLogoUrl);
                            localLogoPath = await DownloadImageToTempFileAsync(fullLogoUrl);
                        }
                    }
                    else
                    {
                        // Construir la URL completa si es necesario
                        var fullLogoUrl = logoUrl;
                        if (!logoUrl.StartsWith("http://") && !logoUrl.StartsWith("https://") && !logoUrl.StartsWith("data:") && !Path.IsPathRooted(logoUrl))
                        {
                            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
                            var normalizedLogoPath = logoUrl.Replace("\\", "/").TrimStart('/');
                            fullLogoUrl = $"{baseUrl.TrimEnd('/')}/{normalizedLogoPath}";
                        }
                        
                        _logger.LogInformation("Descargando logo desde: {LogoUrl}", fullLogoUrl);
                        localLogoPath = await DownloadImageToTempFileAsync(fullLogoUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al descargar el logo para cierre de caja");
                }
            }

            var pdfPath = Path.Combine(
                Path.GetTempPath(),
                $"Cierre_Caja_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}_{DateTime.Now:HHmmss}.pdf"
            );

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    // Header
                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            // Logo
                            if (localLogoPath != null && File.Exists(localLogoPath))
                            {
                                row.ConstantItem(60).Height(60).Image(localLogoPath);
                            }
                            else
                            {
                                row.ConstantItem(60).Height(60).Placeholder();
                            }

                            // Información de la empresa
                            row.RelativeItem().PaddingLeft(10).Column(companyCol =>
                            {
                                companyCol.Item().Text(companyName).FontSize(16).Bold();
                                if (!string.IsNullOrWhiteSpace(companyRuc))
                                {
                                    companyCol.Item().Text($"RUC: {companyRuc}").FontSize(10).FontColor(Colors.Grey.Medium);
                                }
                                if (!string.IsNullOrWhiteSpace(companyAddress))
                                {
                                    companyCol.Item().Text(companyAddress).FontSize(9).FontColor(Colors.Grey.Medium);
                                }
                                if (!string.IsNullOrWhiteSpace(companyPhone))
                                {
                                    companyCol.Item().Text($"Tel: {companyPhone}").FontSize(9).FontColor(Colors.Grey.Medium);
                                }
                            });
                        });
                    });

                    // Content
                    page.Content().Column(column =>
                    {
                        // Título
                        column.Item().PaddingBottom(10).AlignCenter().Text("CIERRE DE CAJA")
                            .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);

                        // Período
                        column.Item().PaddingBottom(15).Column(periodCol =>
                        {
                            periodCol.Item().AlignCenter().Text($"Período: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}")
                                .FontSize(12).FontColor(Colors.Grey.Darken1);
                            periodCol.Item().AlignCenter().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}")
                                .FontSize(10).FontColor(Colors.Grey.Medium);
                        });

                        // Resumen
                        column.Item().PaddingBottom(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Element(CellStyle).Text("Total de Ventas:").FontSize(11);
                            table.Cell().Element(CellStyle).AlignRight().Text(totalCount.ToString()).FontSize(11).Bold();

                            table.Cell().Element(CellStyle).Text("Total Recaudado:").FontSize(11);
                            table.Cell().Element(CellStyle).AlignRight().Text($"S/ {totalPaid:F2}").FontSize(11).Bold().FontColor(Colors.Green.Darken2);
                        });

                        // Desglose por método de pago
                        column.Item().PaddingBottom(10).Text("Desglose por Método de Pago").FontSize(14).Bold();
                        column.Item().PaddingBottom(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            // Header
                            table.Cell().Element(CellStyle).Text("Método").FontSize(10).Bold();
                            table.Cell().Element(CellStyle).AlignRight().Text("Cantidad").FontSize(10).Bold();
                            table.Cell().Element(CellStyle).AlignRight().Text("Total").FontSize(10).Bold();
                            table.Cell().Element(CellStyle).AlignRight().Text("%").FontSize(10).Bold();

                            // Data
                            foreach (var payment in paymentGroups)
                            {
                                var percentage = totalPaid > 0 ? (payment.Total / totalPaid * 100) : 0;
                                table.Cell().Element(CellStyle).Text(GetPaymentMethodText(payment.Method)).FontSize(10);
                                table.Cell().Element(CellStyle).AlignRight().Text(payment.Count.ToString()).FontSize(10);
                                table.Cell().Element(CellStyle).AlignRight().Text($"S/ {payment.Total:F2}").FontSize(10);
                                table.Cell().Element(CellStyle).AlignRight().Text($"{percentage:F1}%").FontSize(10);
                            }
                        });

                        // Lista detallada de ventas
                        column.Item().PaddingTop(10).PaddingBottom(10).Text("Detalle de Ventas").FontSize(14).Bold();
                        
                        var paidSales = sales.Where(s => s.Status == SaleStatus.Pagado).OrderBy(s => s.SaleDate).ToList();
                        
                        foreach (var sale in paidSales)
                        {
                            column.Item().PaddingBottom(8).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Column(saleCol =>
                            {
                                saleCol.Item().Row(row =>
                                {
                                    row.RelativeItem().Text($"{sale.DocumentNumber} - {sale.DocumentType}").FontSize(10).Bold();
                                    row.ConstantItem(80).AlignRight().Text(sale.SaleDate.ToString("dd/MM/yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Medium);
                                });
                                
                                if (sale.Customer != null)
                                {
                                    saleCol.Item().Text($"Cliente: {sale.Customer.Name}").FontSize(9).FontColor(Colors.Grey.Darken1);
                                }
                                
                                // Mostrar items de la venta si están disponibles
                                if (sale.SaleDetails != null && sale.SaleDetails.Any())
                                {
                                    saleCol.Item().PaddingTop(3).PaddingLeft(10).Column(itemsCol =>
                                    {
                                        foreach (var item in sale.SaleDetails)
                                        {
                                            var productName = item.Product?.Name ?? "Producto";
                                            itemsCol.Item().Row(itemRow =>
                                            {
                                                itemRow.RelativeItem().Text($"{item.Quantity}x {productName}").FontSize(8).FontColor(Colors.Grey.Darken2);
                                                itemRow.ConstantItem(80).AlignRight().Text($"S/ {item.Subtotal:F2}").FontSize(8).FontColor(Colors.Grey.Darken2);
                                            });
                                        }
                                    });
                                }
                                
                                saleCol.Item().Row(row =>
                                {
                                    row.RelativeItem().Text($"Método: {GetPaymentMethodText(sale.PaymentMethod)}").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    row.ConstantItem(100).AlignRight().Text($"S/ {sale.Total:F2}").FontSize(10).Bold();
                                });
                            });
                        }
                    });

                    // Footer
                    page.Footer()
                        .BorderTop(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .PaddingTop(10)
                        .AlignCenter()
                        .Text("Este documento es un reporte de cierre de caja generado automáticamente")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            document.GeneratePdf(pdfPath);
            _logger.LogInformation("PDF de cierre de caja generado exitosamente: {Path}", pdfPath);
            return pdfPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando PDF de cierre de caja");
            throw;
        }
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(5)
            .PaddingHorizontal(5);
    }
}


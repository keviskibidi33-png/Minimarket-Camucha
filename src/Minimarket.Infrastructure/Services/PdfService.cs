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

    public PdfService(IUnitOfWork unitOfWork, ILogger<PdfService> logger, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _configuration = configuration;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenerateSaleReceiptAsync(Guid saleId, string documentType)
    {
        try
        {
            var sale = await _unitOfWork.Sales.GetByIdAsync(saleId);
            if (sale == null)
            {
                throw new Exception("Venta no encontrada");
            }

            var saleDetails = (await _unitOfWork.SaleDetails.FindAsync(sd => sd.SaleId == sale.Id)).ToList();
            var customer = sale.CustomerId.HasValue 
                ? await _unitOfWork.Customers.GetByIdAsync(sale.CustomerId.Value) 
                : null;

            var companyName = _configuration["Company:Name"] ?? "Minimarket Camucha";
            var companyRuc = _configuration["Company:Ruc"] ?? "20123456789";
            var companyAddress = _configuration["Company:Address"] ?? "Av. Principal 123, Lima, Perú";
            var companyPhone = _configuration["Company:Phone"] ?? "+51 999 999 999";

            var pdfPath = Path.Combine(
                Path.GetTempPath(),
                $"{(documentType == "Factura" ? "F" : "B")}_{sale.DocumentNumber.Replace("-", "_")}.pdf"
            );

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Column(column =>
                        {
                            column.Item().Text(companyName).FontSize(16).Bold();
                            column.Item().Text($"RUC: {companyRuc}").FontSize(9);
                            column.Item().Text(companyAddress).FontSize(9);
                            column.Item().Text($"Tel: {companyPhone}").FontSize(9);
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            // Tipo de documento
                            column.Item().Text(documentType == "Factura" ? "FACTURA" : "BOLETA DE VENTA")
                                .FontSize(18).Bold().AlignCenter();

                            // Número de documento y fecha
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Número: {sale.DocumentNumber}").FontSize(11);
                                    c.Item().Text($"Fecha: {sale.SaleDate:dd/MM/yyyy HH:mm}").FontSize(11);
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    if (customer != null)
                                    {
                                        c.Item().Text($"Cliente: {customer.Name}").FontSize(11);
                                        c.Item().Text($"{customer.DocumentType}: {customer.DocumentNumber}").FontSize(11);
                                        c.Item().Text($"Dirección: {customer.Address ?? "-"}").FontSize(11);
                                    }
                                });
                            });

                            column.Item().PaddingTop(10).LineHorizontal(1);

                            // Detalle de productos
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Producto").FontSize(9).Bold();
                                    header.Cell().Element(CellStyle).AlignCenter().Text("Cantidad").FontSize(9).Bold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("P. Unit").FontSize(9).Bold();
                                    header.Cell().Element(CellStyle).AlignRight().Text("Subtotal").FontSize(9).Bold();

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                    }
                                });

                                // Items
                                foreach (var detail in saleDetails)
                                {
                                    table.Cell().Element(CellStyle).Text($"{detail.ProductName} ({detail.ProductCode})").FontSize(9);
                                    table.Cell().Element(CellStyle).AlignCenter().Text(detail.Quantity.ToString()).FontSize(9);
                                    table.Cell().Element(CellStyle).AlignRight().Text($"S/ {detail.UnitPrice:F2}").FontSize(9);
                                    table.Cell().Element(CellStyle).AlignRight().Text($"S/ {detail.Subtotal:F2}").FontSize(9);
                                }
                            });

                            column.Item().PaddingTop(10);

                            // Totales
                            column.Item().AlignRight().Column(totals =>
                            {
                                totals.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("Subtotal:").FontSize(10);
                                    row.ConstantItem(80).Text($"S/ {sale.Subtotal:F2}").FontSize(10).AlignRight();
                                });

                                if (sale.Discount > 0)
                                {
                                    totals.Item().Row(row =>
                                    {
                                        row.ConstantItem(100).Text("Descuento:").FontSize(10);
                                        row.ConstantItem(80).Text($"S/ {sale.Discount:F2}").FontSize(10).AlignRight();
                                    });
                                }

                                totals.Item().Row(row =>
                                {
                                    row.ConstantItem(100).Text("IGV (18%):").FontSize(10).Bold();
                                    row.ConstantItem(80).Text($"S/ {sale.Tax:F2}").FontSize(10).Bold().AlignRight();
                                });

                                totals.Item().PaddingTop(5).Row(row =>
                                {
                                    row.ConstantItem(100).Text("TOTAL:").FontSize(12).Bold();
                                    row.ConstantItem(80).Text($"S/ {sale.Total:F2}").FontSize(12).Bold().AlignRight();
                                });
                            });

                            column.Item().PaddingTop(10).LineHorizontal(1);

                            // Información de pago
                            column.Item().PaddingTop(5).Column(payment =>
                            {
                                payment.Item().Text($"Método de pago: {GetPaymentMethodText(sale.PaymentMethod)}").FontSize(10);
                                payment.Item().Text($"Monto pagado: S/ {sale.AmountPaid:F2}").FontSize(10);
                                if (sale.Change > 0)
                                {
                                    payment.Item().Text($"Vuelto: S/ {sale.Change:F2}").FontSize(10);
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Gracias por su compra");
                        }).FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });

            document.GeneratePdf(pdfPath);

            _logger.LogInformation("PDF generated successfully: {Path}", pdfPath);
            return pdfPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for sale {SaleId}", saleId);
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
}


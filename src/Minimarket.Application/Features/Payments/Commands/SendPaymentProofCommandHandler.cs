using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;
using System.Text;
using System.IO;

namespace Minimarket.Application.Features.Payments.Commands;

public class SendPaymentProofCommandHandler : IRequestHandler<SendPaymentProofCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<SendPaymentProofCommandHandler> _logger;

    public SendPaymentProofCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<SendPaymentProofCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(SendPaymentProofCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Buscar el pedido por número de pedido
            var order = await _unitOfWork.WebOrders.FirstOrDefaultAsync(
                o => o.OrderNumber == request.OrderNumber, 
                cancellationToken);

            if (order == null)
            {
                return Result<bool>.Failure($"Pedido con número {request.OrderNumber} no encontrado");
            }

            // Convertir base64 a bytes
            string base64Data = request.FileData;
            if (base64Data.Contains(","))
            {
                base64Data = base64Data.Split(',')[1]; // Remover el prefijo data:image/...;base64,
            }

            byte[] fileBytes = Convert.FromBase64String(base64Data);

            // Determinar extensión del archivo
            string extension = ".jpg";
            if (request.FileType.Contains("png"))
                extension = ".png";
            else if (request.FileType.Contains("pdf"))
                extension = ".pdf";
            else if (request.FileType.Contains("jpeg") || request.FileType.Contains("jpg"))
                extension = ".jpg";

            // Generar nombre único para el archivo
            string uniqueFileName = $"payment-proof-{order.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
            
            // Guardar archivo directamente (permite PDFs)
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "payment-proofs");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var filePath = Path.Combine(uploadsPath, uniqueFileName);
            await File.WriteAllBytesAsync(filePath, fileBytes, cancellationToken);

            // Generar URL del archivo
            var relativePath = Path.Combine("uploads", "payment-proofs", uniqueFileName).Replace("\\", "/");
            var fileUrl = _fileStorageService.GetFileUrl(relativePath);

            // Actualizar el pedido con la URL del comprobante
            order.PaymentProofUrl = fileUrl;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Comprobante de pago guardado para el pedido {OrderNumber}: {FileUrl}", request.OrderNumber, fileUrl);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar el comprobante de pago para el pedido {OrderNumber}", request.OrderNumber);
            return Result<bool>.Failure($"Error al procesar el comprobante: {ex.Message}");
        }
    }
}


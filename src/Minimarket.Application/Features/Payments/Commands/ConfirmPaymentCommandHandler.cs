using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Minimarket.Application.Features.Payments.Commands;

public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public ConfirmPaymentCommandHandler(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<Result<bool>> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        var stripeSecretKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(stripeSecretKey))
        {
            return Result<bool>.Failure("Stripe no está configurado correctamente");
        }

        StripeConfiguration.ApiKey = stripeSecretKey;

        // Verificar que la venta existe
        var sale = await _unitOfWork.Sales.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale == null)
        {
            throw new NotFoundException("Sale", request.SaleId);
        }

        // Verificar el estado del PaymentIntent en Stripe
        var service = new PaymentIntentService();
        var paymentIntent = await service.GetAsync(request.PaymentIntentId, cancellationToken: cancellationToken);

        if (paymentIntent.Status != "succeeded")
        {
            return Result<bool>.Failure($"El pago no fue exitoso. Estado: {paymentIntent.Status}");
        }

        // Actualizar la venta con el ID del pago
        // Nota: Podrías agregar un campo PaymentIntentId a la entidad Sale si lo necesitas
        sale.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Sales.UpdateAsync(sale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}


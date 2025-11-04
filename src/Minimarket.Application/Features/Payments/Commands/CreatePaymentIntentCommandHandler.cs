using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Payments.DTOs;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Minimarket.Application.Features.Payments.Commands;

public class CreatePaymentIntentCommandHandler : IRequestHandler<CreatePaymentIntentCommand, Result<PaymentIntentResponseDto>>
{
    private readonly IConfiguration _configuration;

    public CreatePaymentIntentCommandHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Result<PaymentIntentResponseDto>> Handle(CreatePaymentIntentCommand request, CancellationToken cancellationToken)
    {
        var stripeSecretKey = _configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(stripeSecretKey))
        {
            return Result<PaymentIntentResponseDto>.Failure("Stripe no está configurado correctamente");
        }

        StripeConfiguration.ApiKey = stripeSecretKey;

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(request.PaymentIntent.Amount * 100), // Convertir a centavos
            Currency = request.PaymentIntent.Currency,
            Description = request.PaymentIntent.Description,
            Metadata = request.PaymentIntent.Metadata
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

        var response = new PaymentIntentResponseDto
        {
            ClientSecret = paymentIntent.ClientSecret,
            PaymentIntentId = paymentIntent.Id
        };

        return Result<PaymentIntentResponseDto>.Success(response);
    }
}


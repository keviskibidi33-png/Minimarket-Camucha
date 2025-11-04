using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Payments.DTOs;

namespace Minimarket.Application.Features.Payments.Commands;

public class CreatePaymentIntentCommand : IRequest<Result<PaymentIntentResponseDto>>
{
    public CreatePaymentIntentDto PaymentIntent { get; set; } = new();
}


using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Payments.Commands;

public class SendPaymentProofCommand : IRequest<Result<bool>>
{
    public string OrderNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? WalletMethod { get; set; }
    public string? BankAccount { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileData { get; set; } = string.Empty; // Base64
    public string FileType { get; set; } = string.Empty;
    public string? OperationCode { get; set; }
}


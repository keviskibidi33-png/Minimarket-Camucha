using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Auth.Commands;

public class ForgotPasswordCommand : IRequest<Result<string>>
{
    public string Email { get; set; } = string.Empty;
}


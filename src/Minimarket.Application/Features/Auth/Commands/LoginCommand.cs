using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Auth.DTOs;

namespace Minimarket.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}


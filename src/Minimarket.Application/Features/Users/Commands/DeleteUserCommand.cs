using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.Users.Commands;

public class DeleteUserCommand : IRequest<Result<string>>
{
    public Guid UserId { get; set; }
}


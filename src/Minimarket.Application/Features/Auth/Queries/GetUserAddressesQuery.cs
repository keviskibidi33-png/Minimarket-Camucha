using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Auth.Commands;

namespace Minimarket.Application.Features.Auth.Queries;

public class GetUserAddressesQuery : IRequest<Result<List<UserAddressResponse>>>
{
    public Guid UserId { get; set; }
}


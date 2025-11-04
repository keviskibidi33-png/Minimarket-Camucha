using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Permissions.DTOs;

namespace Minimarket.Application.Features.Permissions.Queries;

public class GetAllModulesQuery : IRequest<Result<IEnumerable<ModuleDto>>>
{
}


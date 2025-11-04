using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Features.Permissions.Commands;
using Minimarket.Application.Features.Permissions.Queries;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public PermissionsController(
        IMediator mediator,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _mediator = mediator;
        _roleManager = roleManager;
    }

    [HttpGet("modules")]
    public async Task<IActionResult> GetAllModules()
    {
        var query = new GetAllModulesQuery();
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpGet("role-permissions")]
    public async Task<IActionResult> GetRolePermissions([FromQuery] Guid? roleId)
    {
        var query = new GetRolePermissionsQuery { RoleId = roleId };
        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpPut("role-permissions")]
    public async Task<IActionResult> UpdateRolePermissions([FromBody] UpdateRolePermissionsCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result.Data);
    }

    [HttpGet("roles")]
    public IActionResult GetAllRoles()
    {
        var roles = _roleManager.Roles
            .Select(r => new { Id = r.Id, Name = r.Name })
            .ToList();

        return Ok(roles);
    }
}


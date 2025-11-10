using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimarket.Application.Common.Attributes;
using Minimarket.Application.Features.Users.Commands;
using Minimarket.Application.Features.Users.Queries;
using System.Security.Claims;

namespace Minimarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere autenticación
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [RequirePermission("usuarios", "View")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? roleFilter = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllUsersQuery
        {
            SearchTerm = searchTerm,
            RoleFilter = roleFilter,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [RequirePermission("usuarios", "Create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    [RequirePermission("usuarios", "Edit")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        command.UserId = id;
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                errors = result.Errors,
                message = result.Error
            });
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    [RequirePermission("usuarios", "Delete")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand { UserId = id };
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                message = result.Error
            });
        }

        return Ok(new { message = result.Data });
    }

    [HttpPost("{id}/reset-password")]
    [RequirePermission("usuarios", "Edit")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetUserPasswordCommand command)
    {
        command.UserId = id;
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { 
                succeeded = false,
                message = result.Error
            });
        }

        return Ok(new { message = result.Data });
    }
}


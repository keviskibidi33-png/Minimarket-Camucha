using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Settings.DTOs;

namespace Minimarket.Application.Features.Settings.Commands;

public class UpdateSystemSettingsCommand : IRequest<Result<SystemSettingsDto>>
{
    public UpdateSystemSettingsDto Setting { get; set; } = new();
}


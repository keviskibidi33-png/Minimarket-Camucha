using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Settings.DTOs;

namespace Minimarket.Application.Features.Settings.Queries;

public class GetSettingByKeyQuery : IRequest<Result<SystemSettingsDto?>>
{
    public string Key { get; set; } = string.Empty;
}


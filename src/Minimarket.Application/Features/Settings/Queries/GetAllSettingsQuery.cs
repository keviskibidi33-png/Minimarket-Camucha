using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Settings.DTOs;

namespace Minimarket.Application.Features.Settings.Queries;

public class GetAllSettingsQuery : IRequest<Result<IEnumerable<SystemSettingsDto>>>
{
    public string? Category { get; set; } // Filtrar por categoría opcional
}


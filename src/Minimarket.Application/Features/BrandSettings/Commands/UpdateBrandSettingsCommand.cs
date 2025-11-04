using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.BrandSettings.DTOs;

namespace Minimarket.Application.Features.BrandSettings.Commands;

public class UpdateBrandSettingsCommand : IRequest<Result<BrandSettingsDto>>
{
    public UpdateBrandSettingsDto BrandSettings { get; set; } = new();
    public Guid UpdatedBy { get; set; }
}


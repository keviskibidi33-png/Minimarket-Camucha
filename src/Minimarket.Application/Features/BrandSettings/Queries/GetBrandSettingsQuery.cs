using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.BrandSettings.DTOs;

namespace Minimarket.Application.Features.BrandSettings.Queries;

public class GetBrandSettingsQuery : IRequest<Result<BrandSettingsDto?>>
{
}


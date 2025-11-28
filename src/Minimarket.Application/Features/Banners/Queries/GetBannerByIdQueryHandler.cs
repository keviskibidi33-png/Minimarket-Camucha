using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Banners.DTOs;
using Minimarket.Application.Features.Banners.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Banners.Queries;

public class GetBannerByIdQueryHandler : IRequestHandler<GetBannerByIdQuery, Result<BannerDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBannerByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BannerDto>> Handle(GetBannerByIdQuery request, CancellationToken cancellationToken)
    {
        var banner = await _unitOfWork.Banners.GetByIdAsync(request.Id, cancellationToken);

        if (banner == null || banner.IsDeleted)
        {
            throw new NotFoundException("Banner", request.Id);
        }

        var dto = CreateBannerCommandHandler.MapToDto(banner);
        return Result<BannerDto>.Success(dto);
    }
}

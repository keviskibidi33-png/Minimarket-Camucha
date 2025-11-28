using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Banners.Commands;

public class IncrementBannerViewCommandHandler : IRequestHandler<IncrementBannerViewCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public IncrementBannerViewCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(IncrementBannerViewCommand request, CancellationToken cancellationToken)
    {
        var banner = await _unitOfWork.Banners.GetByIdAsync(request.Id, cancellationToken);

        if (banner == null)
        {
            throw new NotFoundException("Banner", request.Id);
        }

        banner.IncrementarVisualizacion();
        banner.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Banners.UpdateAsync(banner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}


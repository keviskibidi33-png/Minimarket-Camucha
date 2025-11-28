using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Banners.Commands;

public class DeleteBannerCommandHandler : IRequestHandler<DeleteBannerCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBannerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await _unitOfWork.Banners.GetByIdAsync(request.Id, cancellationToken);

        if (banner == null || banner.IsDeleted)
        {
            throw new NotFoundException("Banner", request.Id);
        }

        // Soft Delete: Marcar como eliminado en lugar de borrar físicamente
        banner.IsDeleted = true;
        banner.DeletedAt = DateTime.UtcNow;
        banner.UpdatedAt = DateTime.UtcNow;
        
        await _unitOfWork.Banners.UpdateAsync(banner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}


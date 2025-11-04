using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Analytics.Commands;

public class TrackProductViewCommandHandler : IRequestHandler<TrackProductViewCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public TrackProductViewCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(TrackProductViewCommand request, CancellationToken cancellationToken)
    {
        var productView = new ProductView
        {
            ProductId = request.ProductId,
            UserId = request.UserId,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            ViewedAt = DateTime.UtcNow
        };

        await _unitOfWork.ProductViews.AddAsync(productView, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

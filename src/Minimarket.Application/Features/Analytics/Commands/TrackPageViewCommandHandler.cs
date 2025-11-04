using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Analytics.Commands;

public class TrackPageViewCommandHandler : IRequestHandler<TrackPageViewCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public TrackPageViewCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(TrackPageViewCommand request, CancellationToken cancellationToken)
    {
        var pageView = new PageView
        {
            PageSlug = request.PageSlug,
            UserId = request.UserId,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            ViewedAt = DateTime.UtcNow
        };

        await _unitOfWork.PageViews.AddAsync(pageView, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}

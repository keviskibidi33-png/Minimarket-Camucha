using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Auth.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Auth.Queries;

public class GetUserAddressesQueryHandler : IRequestHandler<GetUserAddressesQuery, Result<List<UserAddressResponse>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserAddressesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<UserAddressResponse>>> Handle(GetUserAddressesQuery request, CancellationToken cancellationToken)
    {
        var addresses = await _unitOfWork.UserAddresses
            .FindAsync(ua => ua.UserId == request.UserId, cancellationToken);
        
        var response = addresses.Select(a => new UserAddressResponse
        {
            Id = a.Id,
            Label = a.Label,
            FullName = a.FullName,
            Phone = a.Phone,
            Address = a.Address,
            Reference = a.Reference,
            District = a.District,
            City = a.City,
            Region = a.Region,
            PostalCode = a.PostalCode,
            Latitude = a.Latitude,
            Longitude = a.Longitude,
            IsDefault = a.IsDefault
        }).OrderByDescending(a => a.IsDefault).ThenBy(a => a.Label).ToList();
        
        return Result<List<UserAddressResponse>>.Success(response);
    }
}


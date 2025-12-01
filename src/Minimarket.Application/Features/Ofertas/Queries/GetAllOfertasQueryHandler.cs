using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Ofertas.DTOs;
using Minimarket.Application.Features.Ofertas.Commands;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Ofertas.Queries;

public class GetAllOfertasQueryHandler : IRequestHandler<GetAllOfertasQuery, Result<IEnumerable<OfertaDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllOfertasQueryHandler> _logger;

    public GetAllOfertasQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllOfertasQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<OfertaDto>>> Handle(GetAllOfertasQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Loading ofertas. SoloActivas: {SoloActivas}", request.SoloActivas);
            
            var ofertas = await _unitOfWork.Ofertas.GetAllAsync(cancellationToken);
            _logger.LogInformation("Loaded {Count} ofertas from database", ofertas?.Count() ?? 0);

            IEnumerable<Domain.Entities.Oferta> filtered = ofertas ?? Enumerable.Empty<Domain.Entities.Oferta>();
            
            if (request.SoloActivas.HasValue && request.SoloActivas.Value && ofertas != null)
            {
                var now = DateTime.UtcNow;
                filtered = ofertas.Where(o => 
                {
                    try
                    {
                        return o != null && o.Activa && o.IsActive(now);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error checking if oferta {OfertaId} is active", o?.Id);
                        return false;
                    }
                });
            }

            var result = new List<OfertaDto>();
            var orderedOfertas = filtered.OrderBy(o => o?.Orden ?? int.MaxValue).ThenBy(o => o?.FechaInicio ?? DateTime.MaxValue);
            
            foreach (var oferta in orderedOfertas)
            {
                if (oferta == null)
                {
                    _logger.LogWarning("Found null oferta in collection");
                    continue;
                }

                try
                {
                    var dto = CreateOfertaCommandHandler.MapToDto(oferta);
                    if (dto != null)
                    {
                        result.Add(dto);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error mapping oferta {OfertaId} to DTO", oferta.Id);
                    // Continuar con la siguiente oferta
                }
            }

            _logger.LogInformation("Successfully mapped {Count} ofertas to DTOs", result.Count);
            return Result<IEnumerable<OfertaDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading ofertas: {Message}", ex.Message);
            return Result<IEnumerable<OfertaDto>>.Failure($"Error al cargar ofertas: {ex.Message}");
        }
    }
}


using System;
using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Banners.DTOs;
using Minimarket.Application.Features.Banners.Commands;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Minimarket.Application.Features.Banners.Queries;

public class GetAllBannersQueryHandler : IRequestHandler<GetAllBannersQuery, Result<List<BannerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllBannersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<BannerDto>>> Handle(GetAllBannersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var banners = await _unitOfWork.Banners.GetAllAsync(cancellationToken);

            // Aplicar filtros
            var filteredBanners = banners.AsQueryable();

            // Filtrar banners eliminados (soft delete) - SOLO en consultas públicas
            // El backoffice debe ver TODOS los banners (activos, inactivos, eliminados)
            if (request.SoloActivos.HasValue && request.SoloActivos.Value)
            {
                // En consultas públicas (soloActivos=true), excluir eliminados
                filteredBanners = filteredBanners.Where(b => !b.IsDeleted);
                
                // Filtrar solo banners activos y válidos
                var fechaActual = DateTime.UtcNow;
                filteredBanners = filteredBanners.Where(b => 
                    b.Activo && 
                    (!b.FechaInicio.HasValue || b.FechaInicio.Value <= fechaActual) &&
                    (!b.FechaFin.HasValue || b.FechaFin.Value >= fechaActual) &&
                    (!b.MaxVisualizaciones.HasValue || b.VisualizacionesActuales < b.MaxVisualizaciones.Value)
                );
            }
            // Si NO es consulta pública (backoffice), NO filtrar por IsDeleted
            // El backoffice debe ver TODOS los banners para gestionarlos

            if (request.Tipo.HasValue)
            {
                filteredBanners = filteredBanners.Where(b => (int)b.Tipo == request.Tipo.Value);
            }

            if (request.Posicion.HasValue)
            {
                filteredBanners = filteredBanners.Where(b => (int)b.Posicion == request.Posicion.Value);
            }

            // Ordenar por orden y luego por fecha de creación
            var sortedBanners = filteredBanners
                .OrderBy(b => b.Orden)
                .ThenBy(b => b.CreatedAt)
                .ToList();

            var dtos = sortedBanners.Select(b => CreateBannerCommandHandler.MapToDto(b)).ToList();
            return Result<List<BannerDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            // Log del error para debugging
            // En producción, usar un logger apropiado
            return Result<List<BannerDto>>.Failure($"Error al cargar banners: {ex.Message}");
        }
    }
}

using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Banners.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Banners.Commands;

public class CreateBannerCommandHandler : IRequestHandler<CreateBannerCommand, Result<BannerDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBannerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BannerDto>> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        // Validar fechas
        if (request.Banner.FechaInicio.HasValue && request.Banner.FechaFin.HasValue)
        {
            if (request.Banner.FechaInicio.Value >= request.Banner.FechaFin.Value)
            {
                return Result<BannerDto>.Failure("La fecha de inicio debe ser anterior a la fecha de fin");
            }
        }

        var banner = new Banner
        {
            Titulo = request.Banner.Titulo,
            Descripcion = request.Banner.Descripcion,
            ImagenUrl = request.Banner.ImagenUrl,
            UrlDestino = request.Banner.UrlDestino,
            AbrirEnNuevaVentana = request.Banner.AbrirEnNuevaVentana,
            Tipo = (BannerTipo)request.Banner.Tipo,
            Posicion = (BannerPosicion)request.Banner.Posicion,
            FechaInicio = request.Banner.FechaInicio,
            FechaFin = request.Banner.FechaFin,
            Activo = request.Banner.Activo,
            Orden = request.Banner.Orden,
            AnchoMaximo = request.Banner.AnchoMaximo,
            AltoMaximo = request.Banner.AltoMaximo,
            ClasesCss = request.Banner.ClasesCss,
            SoloMovil = request.Banner.SoloMovil,
            SoloDesktop = request.Banner.SoloDesktop,
            MaxVisualizaciones = request.Banner.MaxVisualizaciones,
            VisualizacionesActuales = 0
        };

        await _unitOfWork.Banners.AddAsync(banner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(banner);
        return Result<BannerDto>.Success(dto);
    }

    public static BannerDto MapToDto(Banner banner)
    {
        return new BannerDto
        {
            Id = banner.Id,
            Titulo = banner.Titulo,
            Descripcion = banner.Descripcion,
            ImagenUrl = banner.ImagenUrl,
            UrlDestino = banner.UrlDestino,
            AbrirEnNuevaVentana = banner.AbrirEnNuevaVentana,
            Tipo = (int)banner.Tipo,
            Posicion = (int)banner.Posicion,
            FechaInicio = banner.FechaInicio,
            FechaFin = banner.FechaFin,
            Activo = banner.Activo,
            Orden = banner.Orden,
            AnchoMaximo = banner.AnchoMaximo,
            AltoMaximo = banner.AltoMaximo,
            ClasesCss = banner.ClasesCss,
            SoloMovil = banner.SoloMovil,
            SoloDesktop = banner.SoloDesktop,
            MaxVisualizaciones = banner.MaxVisualizaciones,
            VisualizacionesActuales = banner.VisualizacionesActuales,
            CreatedAt = banner.CreatedAt,
            UpdatedAt = banner.UpdatedAt
        };
    }
}

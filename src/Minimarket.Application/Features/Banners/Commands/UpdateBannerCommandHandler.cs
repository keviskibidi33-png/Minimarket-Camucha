using MediatR;
using Minimarket.Application.Common.Exceptions;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.Banners.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.Banners.Commands;

public class UpdateBannerCommandHandler : IRequestHandler<UpdateBannerCommand, Result<BannerDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBannerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BannerDto>> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await _unitOfWork.Banners.GetByIdAsync(request.Id, cancellationToken);

        if (banner == null)
        {
            throw new NotFoundException("Banner", request.Id);
        }

        // Validar fechas
        if (request.Banner.FechaInicio.HasValue && request.Banner.FechaFin.HasValue)
        {
            if (request.Banner.FechaInicio.Value >= request.Banner.FechaFin.Value)
            {
                return Result<BannerDto>.Failure("La fecha de inicio debe ser anterior a la fecha de fin");
            }
        }

        banner.Titulo = request.Banner.Titulo;
        banner.Descripcion = request.Banner.Descripcion;
        banner.ImagenUrl = request.Banner.ImagenUrl;
        banner.UrlDestino = request.Banner.UrlDestino;
        banner.AbrirEnNuevaVentana = request.Banner.AbrirEnNuevaVentana;
        banner.Tipo = (BannerTipo)request.Banner.Tipo;
        banner.Posicion = (BannerPosicion)request.Banner.Posicion;
        banner.FechaInicio = request.Banner.FechaInicio;
        banner.FechaFin = request.Banner.FechaFin;
        banner.Activo = request.Banner.Activo;
        banner.Orden = request.Banner.Orden;
        banner.AnchoMaximo = request.Banner.AnchoMaximo;
        banner.AltoMaximo = request.Banner.AltoMaximo;
        banner.ClasesCss = request.Banner.ClasesCss;
        banner.SoloMovil = request.Banner.SoloMovil;
        banner.SoloDesktop = request.Banner.SoloDesktop;
        banner.MaxVisualizaciones = request.Banner.MaxVisualizaciones;
        banner.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Banners.UpdateAsync(banner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = CreateBannerCommandHandler.MapToDto(banner);
        return Result<BannerDto>.Success(dto);
    }
}

import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { BannersService, Banner, CreateBanner, UpdateBanner, BANNER_TIPOS, BANNER_POSICIONES } from '../../../core/services/banners.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-banners',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './banners.component.html',
  styleUrl: './banners.component.css'
})
export class BannersComponent implements OnInit {
  banners = signal<Banner[]>([]);
  isLoading = signal(false);
  showForm = signal(false);
  editingBanner = signal<Banner | null>(null);
  
  // Filtros
  filtroTipo = signal<number | null>(null);
  filtroPosicion = signal<number | null>(null);
  filtroSoloActivos = signal(false);

  // Form data
  titulo = signal('');
  descripcion = signal('');
  imagenUrl = signal('');
  urlDestino = signal('');
  abrirEnNuevaVentana = signal(false);
  tipo = signal(0);
  posicion = signal(0);
  fechaInicio = signal('');
  fechaFin = signal('');
  activo = signal(true);
  orden = signal(0);
  anchoMaximo = signal<number | null>(null);
  altoMaximo = signal<number | null>(null);
  clasesCss = signal('');
  soloMovil = signal(false);
  soloDesktop = signal(false);
  maxVisualizaciones = signal<number | null>(null);

  // Exponer constantes para el template
  BANNER_TIPOS = BANNER_TIPOS;
  BANNER_POSICIONES = BANNER_POSICIONES;

  constructor(
    private bannersService: BannersService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadBanners();
  }

  loadBanners(): void {
    this.isLoading.set(true);
    this.bannersService.getAll(
      this.filtroSoloActivos() ? true : undefined,
      this.filtroTipo() ?? undefined,
      this.filtroPosicion() ?? undefined
    ).subscribe({
      next: (banners) => {
        this.banners.set(banners);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading banners:', error);
        this.toastService.error('Error al cargar banners');
        this.isLoading.set(false);
      }
    });
  }

  openForm(banner?: Banner): void {
    if (banner) {
      this.editingBanner.set(banner);
      this.titulo.set(banner.titulo);
      this.descripcion.set(banner.descripcion || '');
      this.imagenUrl.set(banner.imagenUrl);
      this.urlDestino.set(banner.urlDestino || '');
      this.abrirEnNuevaVentana.set(banner.abrirEnNuevaVentana);
      this.tipo.set(banner.tipo);
      this.posicion.set(banner.posicion);
      this.fechaInicio.set(banner.fechaInicio ? new Date(banner.fechaInicio).toISOString().split('T')[0] : '');
      this.fechaFin.set(banner.fechaFin ? new Date(banner.fechaFin).toISOString().split('T')[0] : '');
      this.activo.set(banner.activo);
      this.orden.set(banner.orden);
      this.anchoMaximo.set(banner.anchoMaximo ?? null);
      this.altoMaximo.set(banner.altoMaximo ?? null);
      this.clasesCss.set(banner.clasesCss || '');
      this.soloMovil.set(banner.soloMovil);
      this.soloDesktop.set(banner.soloDesktop);
      this.maxVisualizaciones.set(banner.maxVisualizaciones ?? null);
    } else {
      this.resetForm();
    }
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingBanner.set(null);
    this.resetForm();
  }

  resetForm(): void {
    this.titulo.set('');
    this.descripcion.set('');
    this.imagenUrl.set('');
    this.urlDestino.set('');
    this.abrirEnNuevaVentana.set(false);
    this.tipo.set(0);
    this.posicion.set(0);
    this.fechaInicio.set('');
    this.fechaFin.set('');
    this.activo.set(true);
    this.orden.set(0);
    this.anchoMaximo.set(null);
    this.altoMaximo.set(null);
    this.clasesCss.set('');
    this.soloMovil.set(false);
    this.soloDesktop.set(false);
    this.maxVisualizaciones.set(null);
  }

  saveBanner(): void {
    if (!this.titulo().trim() || !this.imagenUrl().trim()) {
      this.toastService.error('Complete todos los campos requeridos');
      return;
    }

    if (this.fechaInicio() && this.fechaFin()) {
      if (new Date(this.fechaInicio()) >= new Date(this.fechaFin())) {
        this.toastService.error('La fecha de inicio debe ser anterior a la fecha de fin');
        return;
      }
    }

    const bannerData: CreateBanner | UpdateBanner = {
      titulo: this.titulo(),
      descripcion: this.descripcion().trim() || undefined,
      imagenUrl: this.imagenUrl(),
      urlDestino: this.urlDestino().trim() || undefined,
      abrirEnNuevaVentana: this.abrirEnNuevaVentana(),
      tipo: this.tipo(),
      posicion: this.posicion(),
      fechaInicio: this.fechaInicio() ? new Date(this.fechaInicio()).toISOString() : undefined,
      fechaFin: this.fechaFin() ? new Date(this.fechaFin()).toISOString() : undefined,
      activo: this.activo(),
      orden: this.orden(),
      anchoMaximo: this.anchoMaximo() ?? undefined,
      altoMaximo: this.altoMaximo() ?? undefined,
      clasesCss: this.clasesCss().trim() || undefined,
      soloMovil: this.soloMovil(),
      soloDesktop: this.soloDesktop(),
      maxVisualizaciones: this.maxVisualizaciones() ?? undefined
    };

    const banner = this.editingBanner();
    if (banner) {
      this.bannersService.update(banner.id, bannerData as UpdateBanner).subscribe({
        next: () => {
          this.toastService.success('Banner actualizado correctamente');
          this.loadBanners();
          this.closeForm();
        },
        error: (error) => {
          console.error('Error updating banner:', error);
          this.toastService.error('Error al actualizar banner');
        }
      });
    } else {
      this.bannersService.create(bannerData as CreateBanner).subscribe({
        next: () => {
          this.toastService.success('Banner creado correctamente');
          this.loadBanners();
          this.closeForm();
        },
        error: (error) => {
          console.error('Error creating banner:', error);
          this.toastService.error('Error al crear banner');
        }
      });
    }
  }

  deleteBanner(banner: Banner): void {
    if (confirm(`¿Está seguro de eliminar el banner "${banner.titulo}"?`)) {
      this.bannersService.delete(banner.id).subscribe({
        next: () => {
          this.toastService.success('Banner eliminado correctamente');
          this.loadBanners();
        },
        error: (error) => {
          console.error('Error deleting banner:', error);
          this.toastService.error('Error al eliminar banner');
        }
      });
    }
  }

  toggleBannerActivo(banner: Banner): void {
    const updateData: UpdateBanner = {
      titulo: banner.titulo,
      descripcion: banner.descripcion,
      imagenUrl: banner.imagenUrl,
      urlDestino: banner.urlDestino,
      abrirEnNuevaVentana: banner.abrirEnNuevaVentana,
      tipo: banner.tipo,
      posicion: banner.posicion,
      fechaInicio: banner.fechaInicio,
      fechaFin: banner.fechaFin,
      activo: !banner.activo,
      orden: banner.orden,
      anchoMaximo: banner.anchoMaximo,
      altoMaximo: banner.altoMaximo,
      clasesCss: banner.clasesCss,
      soloMovil: banner.soloMovil,
      soloDesktop: banner.soloDesktop,
      maxVisualizaciones: banner.maxVisualizaciones
    };

    this.bannersService.update(banner.id, updateData).subscribe({
      next: () => {
        this.toastService.success(banner.activo ? 'Banner desactivado' : 'Banner activado');
        this.loadBanners();
      },
      error: (error) => {
        console.error('Error toggling banner:', error);
        this.toastService.error('Error al cambiar estado del banner');
      }
    });
  }

  isBannerActive(banner: Banner): boolean {
    const now = new Date();
    const inicio = banner.fechaInicio ? new Date(banner.fechaInicio) : null;
    const fin = banner.fechaFin ? new Date(banner.fechaFin) : null;
    
    if (!banner.activo) return false;
    if (inicio && now < inicio) return false;
    if (fin && now > fin) return false;
    if (banner.maxVisualizaciones && banner.visualizacionesActuales >= banner.maxVisualizaciones) return false;
    
    return true;
  }

  getBannerTipoLabel(tipo: number): string {
    const tipoObj = BANNER_TIPOS.find(t => t.value === tipo);
    return tipoObj?.label || 'Desconocido';
  }

  getBannerPosicionLabel(posicion: number): string {
    const posObj = BANNER_POSICIONES.find(p => p.value === posicion);
    return posObj?.label || 'Desconocido';
  }

  applyFilters(): void {
    this.loadBanners();
  }

  clearFilters(): void {
    this.filtroTipo.set(null);
    this.filtroPosicion.set(null);
    this.filtroSoloActivos.set(false);
    this.loadBanners();
  }

  formatDate(dateString: string): string {
    if (!dateString) return '-';
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('es-PE', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      });
    } catch {
      return dateString;
    }
  }
}


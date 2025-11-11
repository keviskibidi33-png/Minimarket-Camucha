import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { OfertasService, Oferta } from '../../../core/services/ofertas.service';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { StoreHeaderComponent } from '../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../shared/components/store-footer/store-footer.component';

@Component({
  selector: 'app-store-ofertas',
  standalone: true,
  imports: [CommonModule, RouterModule, StoreHeaderComponent, StoreFooterComponent],
  templateUrl: './ofertas.component.html',
  styleUrl: './ofertas.component.css'
})
export class StoreOfertasComponent implements OnInit {
  ofertas = signal<Oferta[]>([]);
  isLoading = signal(true);

  constructor(
    private ofertasService: OfertasService,
    private analyticsService: AnalyticsService
  ) {}

  ngOnInit(): void {
    // Trackear vista de página
    this.analyticsService.trackPageView('tienda/ofertas').subscribe({
      error: (error) => console.error('Error tracking page view:', error)
    });
    
    this.loadOfertas();
  }

  loadOfertas(): void {
    this.isLoading.set(true);
    this.ofertasService.getAll(true).subscribe({
      next: (ofertas) => {
        // Filtrar solo las ofertas activas y que estén dentro del rango de fechas
        const now = new Date();
        const activeOfertas = ofertas.filter(oferta => {
          const inicio = new Date(oferta.fechaInicio);
          const fin = new Date(oferta.fechaFin);
          return oferta.activa && now >= inicio && now <= fin;
        });
        // Ordenar por orden y luego por fecha de inicio
        activeOfertas.sort((a, b) => {
          if (a.orden !== b.orden) {
            return a.orden - b.orden;
          }
          return new Date(a.fechaInicio).getTime() - new Date(b.fechaInicio).getTime();
        });
        this.ofertas.set(activeOfertas);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading ofertas:', error);
        this.isLoading.set(false);
      }
    });
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-PE', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  getDescuentoText(oferta: Oferta): string {
    if (oferta.descuentoTipo === 0) {
      return `${oferta.descuentoValor}% OFF`;
    } else {
      return `S/ ${oferta.descuentoValor} OFF`;
    }
  }

  isActive(oferta: Oferta): boolean {
    const now = new Date();
    const inicio = new Date(oferta.fechaInicio);
    const fin = new Date(oferta.fechaFin);
    return oferta.activa && now >= inicio && now <= fin;
  }
}


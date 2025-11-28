import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Banner {
  id: string;
  titulo: string;
  descripcion?: string;
  imagenUrl: string;
  urlDestino?: string;
  abrirEnNuevaVentana: boolean;
  tipo: number; // 0 = Header, 1 = Sidebar, 2 = Footer, 3 = Popup, 4 = Carousel, 5 = Inline
  posicion: number; // 0 = Top, 1 = Middle, 2 = Bottom, 3 = Left, 4 = Right, 5 = Center
  fechaInicio?: string;
  fechaFin?: string;
  activo: boolean;
  orden: number;
  anchoMaximo?: number;
  altoMaximo?: number;
  clasesCss?: string;
  soloMovil: boolean;
  soloDesktop: boolean;
  maxVisualizaciones?: number;
  visualizacionesActuales: number;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateBanner {
  titulo: string;
  descripcion?: string;
  imagenUrl: string;
  urlDestino?: string;
  abrirEnNuevaVentana?: boolean;
  tipo: number;
  posicion: number;
  fechaInicio?: string;
  fechaFin?: string;
  activo?: boolean;
  orden?: number;
  anchoMaximo?: number;
  altoMaximo?: number;
  clasesCss?: string;
  soloMovil?: boolean;
  soloDesktop?: boolean;
  maxVisualizaciones?: number;
}

export interface UpdateBanner {
  titulo: string;
  descripcion?: string;
  imagenUrl: string;
  urlDestino?: string;
  abrirEnNuevaVentana: boolean;
  tipo: number;
  posicion: number;
  fechaInicio?: string;
  fechaFin?: string;
  activo: boolean;
  orden: number;
  anchoMaximo?: number;
  altoMaximo?: number;
  clasesCss?: string;
  soloMovil: boolean;
  soloDesktop: boolean;
  maxVisualizaciones?: number;
}

export const BANNER_TIPOS = [
  { value: 0, label: 'Header', icon: 'view_headline' },
  { value: 1, label: 'Sidebar', icon: 'view_sidebar' },
  { value: 2, label: 'Footer', icon: 'vertical_align_bottom' },
  { value: 3, label: 'Popup', icon: 'open_in_new' },
  { value: 4, label: 'Carousel', icon: 'view_carousel' },
  { value: 5, label: 'Inline', icon: 'view_agenda' }
];

export const BANNER_POSICIONES = [
  { value: 0, label: 'Arriba', icon: 'vertical_align_top' },
  { value: 1, label: 'Medio', icon: 'horizontal_rule' },
  { value: 2, label: 'Abajo', icon: 'vertical_align_bottom' },
  { value: 3, label: 'Izquierda', icon: 'align_horizontal_left' },
  { value: 4, label: 'Derecha', icon: 'align_horizontal_right' },
  { value: 5, label: 'Centro', icon: 'center_focus_strong' }
];

@Injectable({
  providedIn: 'root'
})
export class BannersService {
  private readonly apiUrl = `${environment.apiUrl}/banners`;

  constructor(private http: HttpClient) {}

  getAll(soloActivos?: boolean, tipo?: number, posicion?: number): Observable<Banner[]> {
    let params = new HttpParams();
    if (soloActivos !== undefined) {
      params = params.set('soloActivos', soloActivos.toString());
    }
    if (tipo !== undefined) {
      params = params.set('tipo', tipo.toString());
    }
    if (posicion !== undefined) {
      params = params.set('posicion', posicion.toString());
    }
    return this.http.get<Banner[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Banner> {
    return this.http.get<Banner>(`${this.apiUrl}/${id}`);
  }

  create(banner: CreateBanner): Observable<Banner> {
    // El backend espera { Banner: ... } con mayúscula
    return this.http.post<Banner>(this.apiUrl, { Banner: banner });
  }

  update(id: string, banner: UpdateBanner): Observable<Banner> {
    return this.http.put<Banner>(`${this.apiUrl}/${id}`, banner);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  incrementView(id: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/${id}/increment-view`, {});
  }
}


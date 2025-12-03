import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Oferta {
  id: string;
  nombre: string;
  descripcion?: string;
  descuentoTipo: number; // 0 = Porcentaje, 1 = MontoFijo
  descuentoValor: number;
  categoriasIds: string[];
  productosIds: string[];
  fechaInicio: string;
  fechaFin: string;
  activa: boolean;
  orden: number;
  imagenUrl?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateOferta {
  nombre: string;
  descripcion?: string;
  descuentoTipo: number;
  descuentoValor: number;
  categoriasIds: string[];
  productosIds: string[];
  fechaInicio: string;
  fechaFin: string;
  activa: boolean;
  orden: number;
  imagenUrl?: string;
}

export interface UpdateOferta {
  nombre: string;
  descripcion?: string;
  descuentoTipo: number;
  descuentoValor: number;
  categoriasIds: string[];
  productosIds: string[];
  fechaInicio: string;
  fechaFin: string;
  activa: boolean;
  orden: number;
  imagenUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class OfertasService {
  private readonly apiUrl = `${environment.apiUrl}/ofertas`;

  constructor(private http: HttpClient) {}

  getAll(soloActivas?: boolean): Observable<Oferta[]> {
    let params = new HttpParams();
    if (soloActivas !== undefined) {
      params = params.set('soloActivas', soloActivas.toString());
    }
    return this.http.get<Oferta[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Oferta> {
    return this.http.get<Oferta>(`${this.apiUrl}/${id}`);
  }

  create(oferta: CreateOferta): Observable<Oferta> {
    // El backend ahora acepta directamente CreateOfertaDto
    return this.http.post<Oferta>(this.apiUrl, oferta);
  }

  update(id: string, oferta: UpdateOferta): Observable<Oferta> {
    // El backend ahora acepta directamente UpdateOfertaDto
    return this.http.put<Oferta>(`${this.apiUrl}/${id}`, oferta);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}


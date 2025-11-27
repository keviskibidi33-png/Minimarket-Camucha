import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Sede {
  id: string;
  nombre: string;
  direccion: string;
  ciudad: string;
  pais: string;
  latitud: number;
  longitud: number;
  telefono?: string;
  horarios: { [key: string]: { abre: string; cierra: string } };
  logoUrl?: string;
  estado: boolean;
  isOpen: boolean;
  nextOpenTime?: string;
  googleMapsUrl?: string;
}

export interface CreateSede {
  nombre: string;
  direccion: string;
  ciudad: string;
  pais: string;
  latitud: number;
  longitud: number;
  telefono?: string;
  horarios: { [key: string]: { abre: string; cierra: string } };
  logoUrl?: string;
  estado: boolean;
  googleMapsUrl?: string;
}

export interface UpdateSede {
  nombre: string;
  direccion: string;
  ciudad: string;
  pais: string;
  latitud: number;
  longitud: number;
  telefono?: string;
  horarios: { [key: string]: { abre: string; cierra: string } };
  logoUrl?: string;
  estado: boolean;
  googleMapsUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class SedesService {
  private readonly apiUrl = `${environment.apiUrl}/sedes`;

  constructor(private http: HttpClient) {}

  getAll(soloActivas?: boolean): Observable<Sede[]> {
    let params = new HttpParams();
    if (soloActivas !== undefined) {
      params = params.set('soloActivas', soloActivas.toString());
    }
    return this.http.get<Sede[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Sede> {
    return this.http.get<Sede>(`${this.apiUrl}/${id}`);
  }

  create(sede: CreateSede): Observable<Sede> {
    return this.http.post<Sede>(this.apiUrl, { sede });
  }

  update(id: string, sede: UpdateSede): Observable<Sede> {
    return this.http.put<Sede>(`${this.apiUrl}/${id}`, { sede });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}


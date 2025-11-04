import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PageSection {
  id?: string;
  pageId?: string;
  seccionTipo: number; // 0-7
  orden: number;
  datos: { [key: string]: any };
}

export interface Page {
  id: string;
  titulo: string;
  slug: string;
  tipoPlantilla: number; // 0 = Home, 1 = ProductoDetalle, 2 = Generica
  metaDescription?: string;
  keywords?: string;
  orden: number;
  activa: boolean;
  sections: PageSection[];
  createdAt: string;
  updatedAt?: string;
}

export interface CreatePage {
  titulo: string;
  slug: string;
  tipoPlantilla: number;
  metaDescription?: string;
  keywords?: string;
  orden: number;
  activa: boolean;
  sections: CreatePageSection[];
}

export interface CreatePageSection {
  seccionTipo: number;
  orden: number;
  datos: { [key: string]: any };
}

export interface UpdatePage {
  titulo: string;
  slug: string;
  tipoPlantilla: number;
  metaDescription?: string;
  keywords?: string;
  orden: number;
  activa: boolean;
  sections: UpdatePageSection[];
}

export interface UpdatePageSection {
  id?: string;
  seccionTipo: number;
  orden: number;
  datos: { [key: string]: any };
}

@Injectable({
  providedIn: 'root'
})
export class PagesService {
  private readonly apiUrl = `${environment.apiUrl}/pages`;

  constructor(private http: HttpClient) {}

  getAll(soloActivas?: boolean): Observable<Page[]> {
    let params = new HttpParams();
    if (soloActivas !== undefined) {
      params = params.set('soloActivas', soloActivas.toString());
    }
    return this.http.get<Page[]>(this.apiUrl, { params });
  }

  getBySlug(slug: string): Observable<Page> {
    return this.http.get<Page>(`${this.apiUrl}/${slug}`);
  }

  create(page: CreatePage): Observable<Page> {
    return this.http.post<Page>(this.apiUrl, { page });
  }

  update(id: string, page: UpdatePage): Observable<Page> {
    return this.http.put<Page>(`${this.apiUrl}/${id}`, { page });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}


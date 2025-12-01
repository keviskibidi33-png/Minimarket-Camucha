import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Category {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
}

export interface CategoryDto {
  id: string;
  name: string;
  description: string;
  imageUrl?: string;
  isActive: boolean;
  orden?: number; // Orden para categorías populares
  productCount?: number; // Conteo de productos en esta categoría
}

export interface CreateCategoryDto {
  name: string;
  description?: string;
}

export interface UpdateCategoryDto {
  name: string;
  description?: string;
  imageUrl?: string;
  isActive: boolean;
  orden?: number; // Orden para categorías populares
}

@Injectable({
  providedIn: 'root'
})
export class CategoriesService {
  private readonly apiUrl = `${environment.apiUrl}/categories`;

  constructor(private http: HttpClient) {}

  getAll(params?: { page?: number; pageSize?: number; searchTerm?: string }): Observable<import('./products.service').PagedResult<CategoryDto>> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.page) {
        httpParams = httpParams.set('page', params.page.toString());
      }
      if (params.pageSize) {
        httpParams = httpParams.set('pageSize', params.pageSize.toString());
      }
      if (params.searchTerm) {
        httpParams = httpParams.set('searchTerm', params.searchTerm);
      }
    }
    return this.http.get<import('./products.service').PagedResult<CategoryDto>>(this.apiUrl, { params: httpParams });
  }

  // Método para obtener todas las categorías sin paginación (para compatibilidad)
  getAllWithoutPagination(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>(this.apiUrl);
  }

  getById(id: string): Observable<CategoryDto> {
    return this.http.get<CategoryDto>(`${this.apiUrl}/${id}`);
  }

  create(category: CreateCategoryDto): Observable<CategoryDto> {
    // El backend CreateCategoryCommand espera { category: {...} }
    return this.http.post<CategoryDto>(this.apiUrl, { category });
  }

  update(id: string, category: UpdateCategoryDto): Observable<CategoryDto> {
    // El backend UpdateCategoryCommand espera { category: {...} }
    return this.http.put<CategoryDto>(`${this.apiUrl}/${id}`, { category });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}


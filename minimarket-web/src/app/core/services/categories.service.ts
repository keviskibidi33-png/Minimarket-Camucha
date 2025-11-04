import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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
}

@Injectable({
  providedIn: 'root'
})
export class CategoriesService {
  private readonly apiUrl = `${environment.apiUrl}/categories`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>(this.apiUrl);
  }

  getById(id: string): Observable<CategoryDto> {
    return this.http.get<CategoryDto>(`${this.apiUrl}/${id}`);
  }

  create(category: CreateCategoryDto): Observable<CategoryDto> {
    // NOTA: El backend CreateCategoryCommand espera { category: {...} }
    // pero ASP.NET Core model binding puede mapear directamente si el JSON coincide
    // Manteniendo consistencia con ProductsService y CustomersService que envían directamente
    return this.http.post<CategoryDto>(this.apiUrl, category);
  }

  update(id: string, category: UpdateCategoryDto): Observable<CategoryDto> {
    // NOTA: El backend UpdateCategoryCommand espera { category: {...} }
    // pero ASP.NET Core model binding puede mapear directamente si el JSON coincide
    // Manteniendo consistencia con ProductsService y CustomersService que envían directamente
    return this.http.put<CategoryDto>(`${this.apiUrl}/${id}`, category);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}


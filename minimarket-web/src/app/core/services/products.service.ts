import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface Product {
  id: string;
  code: string;
  name: string;
  description: string;
  purchasePrice: number;
  salePrice: number;
  stock: number;
  minimumStock: number;
  categoryId: string;
  categoryName: string;
  imageUrl?: string;
  paginas?: { [key: string]: any }; // Para productos destacados: {"home": true, "home_orden": 1}
  isActive: boolean;
  createdAt: string;
  expirationDate?: string; // Fecha de vencimiento
}

export interface CreateProductDto {
  code: string;
  name: string;
  description: string;
  purchasePrice: number;
  salePrice: number;
  stock: number;
  minimumStock: number;
  categoryId: string;
  imageUrl?: string;
  expirationDate?: string; // Fecha de vencimiento
}

export interface UpdateProductDto extends CreateProductDto {
  id: string;
  isActive: boolean;
  paginas?: { [key: string]: any }; // Para productos destacados
}

export interface ProductsQueryParams {
  searchTerm?: string;
  categoryId?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ProductsService {
  private readonly apiUrl = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient) {}

  getAll(params?: ProductsQueryParams): Observable<PagedResult<Product>> {
    let httpParams = new HttpParams();
    
    if (params?.searchTerm && params.searchTerm.trim()) {
      httpParams = httpParams.set('searchTerm', params.searchTerm.trim());
    }
    if (params?.categoryId) {
      // Solo enviar categoryId si es un GUID válido
      const categoryIdStr = String(params.categoryId).trim();
      if (categoryIdStr && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(categoryIdStr)) {
        httpParams = httpParams.set('categoryId', categoryIdStr);
      }
    }
    if (params?.isActive !== undefined) {
      httpParams = httpParams.set('isActive', params.isActive.toString());
    }
    if (params?.page && params.page > 0) {
      httpParams = httpParams.set('page', params.page.toString());
    }
    if (params?.pageSize && params.pageSize > 0) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<PagedResult<Product>>(this.apiUrl, { params: httpParams });
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  /**
   * Crea un nuevo producto
   * 
   * Ejemplo de payload JSON válido:
   * {
   *   "code": "PROD-001",
   *   "name": "Producto Ejemplo",
   *   "description": "Descripción del producto",
   *   "purchasePrice": 10.50,
   *   "salePrice": 15.75,
   *   "stock": 100,
   *   "minimumStock": 10,
   *   "categoryId": "123e4567-e89b-12d3-a456-426614174000",
   *   "imageUrl": "https://example.com/image.jpg",  // Opcional
   *   "expirationDate": "2024-12-31T00:00:00Z"     // Opcional
   * }
   * 
   * Campos requeridos: code, name, purchasePrice, salePrice, stock, minimumStock, categoryId
   * Campos opcionales: description, imageUrl, expirationDate
   */
  create(product: CreateProductDto): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product);
  }

  update(product: UpdateProductDto): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${product.id}`, product).pipe(
      catchError((error: HttpErrorResponse) => {
        // No loguear errores 401 en consola (son esperados cuando no hay sesión)
        if (error.status !== 401) {
          console.error('Error updating product:', error);
        }
        return throwError(() => error);
      })
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}


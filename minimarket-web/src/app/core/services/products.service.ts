import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
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
  isActive: boolean;
  createdAt: string;
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
}

export interface UpdateProductDto extends CreateProductDto {
  id: string;
  isActive: boolean;
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
    
    if (params?.searchTerm) {
      httpParams = httpParams.set('searchTerm', params.searchTerm);
    }
    if (params?.categoryId) {
      httpParams = httpParams.set('categoryId', params.categoryId);
    }
    if (params?.isActive !== undefined) {
      httpParams = httpParams.set('isActive', params.isActive.toString());
    }
    if (params?.page) {
      httpParams = httpParams.set('page', params.page.toString());
    }
    if (params?.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<PagedResult<Product>>(this.apiUrl, { params: httpParams });
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  create(product: CreateProductDto): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product);
  }

  update(product: UpdateProductDto): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${product.id}`, product);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}


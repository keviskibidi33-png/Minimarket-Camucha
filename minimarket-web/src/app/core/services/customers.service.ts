import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Customer {
  id: string;
  documentType: string;
  documentNumber: string;
  name: string;
  email?: string;
  phone?: string;
  address?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateCustomerDto {
  documentType: string;
  documentNumber: string;
  name: string;
  email?: string;
  phone?: string;
  address?: string;
}

export interface UpdateCustomerDto extends CreateCustomerDto {
  id: string;
  isActive: boolean;
}

export interface CustomersQueryParams {
  searchTerm?: string;
  documentType?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class CustomersService {
  private readonly apiUrl = `${environment.apiUrl}/customers`;

  constructor(private http: HttpClient) {}

  getAll(params?: CustomersQueryParams): Observable<import('./products.service').PagedResult<Customer>> {
    let httpParams = new HttpParams();

    if (params?.searchTerm) {
      httpParams = httpParams.set('searchTerm', params.searchTerm);
    }
    if (params?.documentType) {
      httpParams = httpParams.set('documentType', params.documentType);
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

    return this.http.get<import('./products.service').PagedResult<Customer>>(this.apiUrl, { params: httpParams });
  }

  getById(id: string): Observable<Customer> {
    return this.http.get<Customer>(`${this.apiUrl}/${id}`);
  }

  create(customer: CreateCustomerDto): Observable<Customer> {
    // El backend espera { Customer: ... } con mayúscula
    return this.http.post<Customer>(this.apiUrl, { Customer: customer });
  }

  update(customer: UpdateCustomerDto): Observable<Customer> {
    return this.http.put<Customer>(`${this.apiUrl}/${customer.id}`, customer);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}


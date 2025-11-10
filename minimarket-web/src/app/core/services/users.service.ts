import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface User {
  id: string;
  firstName?: string;
  lastName?: string;
  email: string;
  dni?: string;
  phone?: string;
  roles: string[];
  createdAt: string;
  profileCompleted: boolean;
  emailConfirmed: boolean;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  dni: string;
  phone?: string;
  password: string;
  roles: string[];
  emailConfirmed?: boolean;
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  roles: string[];
  emailConfirmed: boolean;
}

export interface UsersResponse {
  items: User[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private readonly apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getAll(params?: {
    searchTerm?: string;
    roleFilter?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<User[]> {
    let httpParams = new HttpParams();
    
    if (params?.searchTerm) {
      httpParams = httpParams.set('searchTerm', params.searchTerm);
    }
    if (params?.roleFilter) {
      httpParams = httpParams.set('roleFilter', params.roleFilter);
    }
    if (params?.pageNumber) {
      httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    }
    if (params?.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<User[]>(this.apiUrl, { params: httpParams });
  }

  create(user: CreateUserRequest): Observable<User> {
    return this.http.post<User>(this.apiUrl, user);
  }

  update(id: string, user: UpdateUserRequest): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/${id}`, user);
  }

  delete(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
  }

  resetPassword(id: string, newPassword: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${id}/reset-password`, {
      newPassword
    });
  }
}


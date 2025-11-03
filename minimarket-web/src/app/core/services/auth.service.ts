import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiration: string;
  userId: string;
  username: string;
  roles: string[];
}

export interface User {
  id: string;
  username: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly tokenKey = 'auth_token';
  private readonly userKey = 'auth_user';

  // Signals para estado reactivo
  public currentUser = signal<User | null>(this.getStoredUser());
  public isAuthenticated = signal<boolean>(this.hasToken());

  constructor(private http: HttpClient) {
    this.loadStoredAuth();
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        this.storeAuth(response);
        this.currentUser.set({
          id: response.userId,
          username: response.username,
          roles: response.roles
        });
        this.isAuthenticated.set(true);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  private hasToken(): boolean {
    return !!this.getToken();
  }

  private storeAuth(response: LoginResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.userKey, JSON.stringify({
      id: response.userId,
      username: response.username,
      roles: response.roles
    }));
  }

  private getStoredUser(): User | null {
    const userStr = localStorage.getItem(this.userKey);
    if (!userStr) return null;
    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  }

  private loadStoredAuth(): void {
    if (this.hasToken() && this.currentUser()) {
      this.isAuthenticated.set(true);
    }
  }
}


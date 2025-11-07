import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Interceptor para agregar el token de autenticación a las peticiones HTTP
 * Usa inyección de dependencias por clase para mayor estabilidad
 */
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.authService.getToken();

    // Headers base para todas las peticiones
    const headers: { [key: string]: string } = {
      'Accept': 'application/json'
    };

    // Agregar token si existe
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    // Solo agregar Content-Type para peticiones que envían datos
    if (req.method !== 'GET' && req.method !== 'HEAD' && req.method !== 'DELETE') {
      headers['Content-Type'] = 'application/json';
    }

    const cloned = req.clone({
      setHeaders: headers
    });

    return next.handle(cloned);
  }
}

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
    // Verificar que el servicio esté disponible y tenga el método
    let token: string | null = null;
    try {
      if (this.authService && typeof this.authService.getToken === 'function') {
        token = this.authService.getToken();
      } else {
        // Fallback: obtener token directamente del localStorage
        token = localStorage.getItem('auth_token');
      }
    } catch (error) {
      // Si hay error, intentar obtener el token directamente
      token = localStorage.getItem('auth_token');
    }

    // Headers base para todas las peticiones
    const headers: { [key: string]: string } = {
      'Accept': 'application/json'
    };

    // Agregar token si existe
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    // Solo agregar Content-Type para peticiones que envían datos
    // NO establecer Content-Type si:
    // 1. Ya existe un Content-Type (para FormData, el navegador lo establece automáticamente)
    // 2. El body es FormData (para uploads de archivos)
    const isFormData = req.body instanceof FormData;
    const hasContentType = req.headers.has('Content-Type');
    
    if (req.method !== 'GET' && req.method !== 'HEAD' && req.method !== 'DELETE') {
      // Solo establecer Content-Type si no es FormData y no tiene Content-Type ya establecido
      if (!isFormData && !hasContentType) {
        headers['Content-Type'] = 'application/json';
      }
    }

    const cloned = req.clone({
      setHeaders: headers
    });

    return next.handle(cloned);
  }
}

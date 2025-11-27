import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

/**
 * Interceptor para manejo centralizado de errores HTTP
 * Usa inyección de dependencias por clase para mayor estabilidad
 */
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private router: Router,
    private toastr: ToastrService
  ) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        this.handleError(error, req);
        return throwError(() => error);
      })
    );
  }

  private handleError(error: HttpErrorResponse, req: HttpRequest<unknown>): void {
    // Manejar errores de conexión (backend no disponible)
    if (error.status === 0) {
      this.handleConnectionError(req);
      return;
    }

    if (error.status === 400) {
      this.handleValidationError(error);
    } else if (error.status === 401) {
      this.handleUnauthorizedError(req);
    } else if (error.status === 403) {
      this.toastr.error(error.error?.message || 'No tienes permiso para acceder a este recurso', 'Acceso Denegado');
    } else if (error.status === 404) {
      // Ignorar errores 404 para endpoints opcionales que pueden no existir aún
      const url = req.url.toLowerCase();
      const isOptionalEndpoint = url.includes('/email-templates/') || 
                                 url.includes('/payment-method-settings') ||
                                 url.includes('/brandsettings');
      
      if (!isOptionalEndpoint) {
        this.toastr.error(error.error?.message || 'Recurso no encontrado', 'Error');
      }
      // Silenciar el error para endpoints opcionales
    } else if (error.status === 422) {
      this.toastr.warning(error.error?.message || 'Regla de negocio violada', 'Regla de Negocio');
    } else if (error.status === 500) {
      this.toastr.error('Error interno del servidor. Contacte al administrador.', 'Error');
    } else {
      this.toastr.error(
        error.error?.message || `Error ${error.status}: ${error.statusText}`,
        'Error'
      );
    }
  }

  private handleConnectionError(req: HttpRequest<unknown>): void {
    const requestUrl = req.url.toLowerCase();
    const currentUrl = this.router.url;
    
    // Endpoints opcionales que pueden fallar sin backend sin ser críticos
    const isOptionalEndpoint = requestUrl.includes('/api/auth/profile') ||
                              requestUrl.includes('/api/auth/addresses') ||
                              requestUrl.includes('/api/brandsettings');
    
    // Rutas públicas donde el backend puede no ser necesario inmediatamente
    const isPublicRoute = currentUrl === '' ||
                         currentUrl === '/' ||
                         currentUrl.startsWith('/tienda') ||
                         currentUrl.startsWith('/carrito') ||
                         currentUrl.startsWith('/checkout') ||
                         currentUrl === '/login';
    
    // Silenciar errores de conexión para endpoints opcionales en rutas públicas
    // Solo mostrar un mensaje si es crítico o en rutas protegidas
    if (isOptionalEndpoint && isPublicRoute) {
      return; // Silenciar completamente
    }
    
    // Mostrar mensaje solo una vez (usando una variable estática o servicio)
    // Por ahora, mostramos solo si no es un endpoint opcional
    if (!isOptionalEndpoint) {
      // Solo mostrar el error si no es un endpoint opcional
      // El mensaje se mostrará una vez por tipo de error
      console.warn('Backend no disponible:', req.url);
    }
  }

  private handleValidationError(error: HttpErrorResponse): void {
    if (error.error?.errors) {
      if (Array.isArray(error.error.errors)) {
        // Errores de validación como lista de strings
        error.error.errors.forEach((err: string) => {
          this.toastr.error(err, 'Error de Validación');
        });
      } else if (typeof error.error.errors === 'object') {
        // Errores de validación como objeto (por campo)
        const errors = error.error.errors;
        Object.keys(errors).forEach(key => {
          const fieldErrors = Array.isArray(errors[key]) ? errors[key] : [errors[key]];
          fieldErrors.forEach((err: string) => {
            this.toastr.error(`${key}: ${err}`, 'Error de Validación');
          });
        });
      }
    } else {
      this.toastr.error(error.error?.message || 'Error de validación', 'Error');
    }
  }

  private handleUnauthorizedError(req: HttpRequest<unknown>): void {
    const currentUrl = this.router.url;
    const requestUrl = req.url.toLowerCase();
    
    // Endpoints opcionales que pueden fallar con 401 sin ser un problema
    // (cuando el usuario no está autenticado pero la funcionalidad sigue funcionando)
    const isOptionalAuthEndpoint = requestUrl.includes('/api/auth/profile') ||
                                   requestUrl.includes('/api/auth/addresses');
    
    // Verificar si es una ruta pública
    const isPublicRoute = currentUrl === '' || 
                         currentUrl === '/' ||
                         currentUrl.startsWith('/tienda') || 
                         currentUrl.startsWith('/carrito') || 
                         currentUrl.startsWith('/checkout') ||
                         currentUrl === '/login' ||
                         requestUrl.includes('/api/products') ||
                         requestUrl.includes('/api/categories');
    
    // Si es un endpoint opcional de auth en una ruta pública, silenciar completamente
    // (no mostrar toast ni loguear en consola)
    if (isOptionalAuthEndpoint && isPublicRoute) {
      return; // Silenciar el error completamente
    }
    
    // Solo mostrar error y redirigir si NO estamos en una ruta pública
    if (!isPublicRoute) {
      this.toastr.error('Sesión expirada o no autorizado', 'No autorizado');
      this.router.navigate(['/login']);
    }
  }
}

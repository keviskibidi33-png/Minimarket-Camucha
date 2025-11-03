import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastr = inject(ToastrService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 400) {
        // Validation errors
        if (error.error && error.error.errors && Array.isArray(error.error.errors)) {
          // Errores de validación como lista de strings
          error.error.errors.forEach((err: string) => {
            toastr.error(err, 'Error de Validación');
          });
        } else if (error.error && error.error.errors && typeof error.error.errors === 'object') {
          // Errores de validación como objeto (por campo)
          const errors = error.error.errors;
          Object.keys(errors).forEach(key => {
            const fieldErrors = Array.isArray(errors[key]) ? errors[key] : [errors[key]];
            fieldErrors.forEach((err: string) => {
              toastr.error(`${key}: ${err}`, 'Error de Validación');
            });
          });
        } else {
          toastr.error(error.error?.message || 'Error de validación', 'Error');
        }
      } else if (error.status === 401) {
        // Unauthorized
        toastr.error('Sesión expirada o no autorizado', 'No autorizado');
        router.navigate(['/login']);
      } else if (error.status === 404) {
        // Not found
        toastr.error(error.error?.message || 'Recurso no encontrado', 'Error');
      } else if (error.status === 422) {
        // Business rule violation
        toastr.warning(error.error?.message || 'Regla de negocio violada', 'Regla de Negocio');
      } else if (error.status === 500) {
        // Internal server error
        toastr.error('Error interno del servidor. Contacte al administrador.', 'Error');
      } else if (error.status === 0) {
        // Network error
        toastr.error('No se pudo conectar con el servidor. Verifique su conexión a internet.', 'Error de Red');
      } else {
        // Otros errores
        toastr.error(
          error.error?.message || `Error ${error.status}: ${error.statusText}`,
          'Error'
        );
      }

      return throwError(() => error);
    })
  );
};

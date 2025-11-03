import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { PermissionsService } from '../services/permissions.service';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const permissionsService = inject(PermissionsService);
    const router = inject(Router);

    // Si no está autenticado, redirigir a login
    if (!authService.isAuthenticated()) {
      router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }

    // Verificar si tiene alguno de los roles permitidos
    if (!permissionsService.hasAnyRole(allowedRoles)) {
      // Redirigir a una página de acceso denegado o al dashboard
      router.navigate(['/dashboard']);
      return false;
    }

    return true;
  };
};


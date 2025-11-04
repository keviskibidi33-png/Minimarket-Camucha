import { inject } from '@angular/core';
import { Router, CanActivateFn, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { PermissionsService } from '../services/permissions.service';

/**
 * Factory function para crear un guard de roles
 * @param allowedRoles Lista de roles permitidos
 * @returns CanActivateFn que verifica si el usuario tiene alguno de los roles permitidos
 */
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
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

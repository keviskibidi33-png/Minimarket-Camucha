import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { SetupStatusService } from '../services/setup-status.service';
import { firstValueFrom } from 'rxjs';

export const setupGuard: CanActivateFn = async (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const setupStatusService = inject(SetupStatusService);

  // Si está intentando acceder a admin-setup, permitir siempre
  if (state.url.includes('/auth/admin-setup')) {
    return true;
  }

  // Verificar si el usuario está autenticado
  const currentUser = authService.currentUser();
  if (!currentUser) {
    router.navigate(['/auth/login']);
    return false;
  }

  // Verificar si es administrador
  const user = authService.currentUser();
  if (!user || !user.roles?.includes('Administrador')) {
    // Si no es admin, permitir acceso normal
    return true;
  }

  // Verificar si el setup está completo
  try {
    const isComplete = await firstValueFrom(setupStatusService.checkSetupComplete());
    
    if (!isComplete) {
      // Si no está completo, redirigir al setup
      router.navigate(['/auth/admin-setup']);
      return false;
    }
    
    return true;
  } catch (error) {
    console.error('Error checking setup status:', error);
    // En caso de error, permitir acceso pero marcar como incompleto
    setupStatusService.markSetupIncomplete();
    router.navigate(['/auth/admin-setup']);
    return false;
  }
};


import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { SetupStatusService } from '../services/setup-status.service';
import { firstValueFrom } from 'rxjs';

export const setupGuard: CanActivateFn = async (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const setupStatusService = inject(SetupStatusService);

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

  // Verificar primero el estado guardado en localStorage/signal
  // Si está marcado como completo manualmente (por ejemplo, al omitir), permitir acceso
  const savedStatus = setupStatusService.getSetupStatus();
  if (savedStatus === true) {
    // Si está marcado como completo en localStorage, permitir acceso sin verificar backend
    return true;
  }

  // Si no está marcado como completo, verificar contra el backend
  try {
    const isComplete = await firstValueFrom(setupStatusService.checkSetupComplete());
    
    if (!isComplete) {
      // Si no está completo, permitir acceso de todas formas (el setup ya no existe)
      // El admin puede configurar desde el panel de administración
      return true;
    }
    
    return true;
  } catch (error) {
    console.error('Error checking setup status:', error);
    // En caso de error, permitir acceso de todas formas
    return true;
  }
};


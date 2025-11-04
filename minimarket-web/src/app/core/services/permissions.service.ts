import { Injectable, signal, effect, EffectRef, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService, User } from './auth.service';

/**
 * Servicio para gestión de permisos y roles de usuario
 * Sincroniza automáticamente con el estado de autenticación
 */
@Injectable({
  providedIn: 'root'
})
export class PermissionsService {
  private readonly currentUser = signal<User | null>(null);
  private effectCleanup?: EffectRef;
  private readonly destroyRef = inject(DestroyRef);

  constructor(private authService: AuthService) {
    // Sincronizar con el signal del AuthService usando effect
    // El effect se ejecuta en el constructor cuando tenemos el contexto de inyección válido
    // Usamos allowSignalWrites para permitir actualizar el signal dentro del effect
    this.effectCleanup = effect(() => {
      const user = this.authService.currentUser();
      this.currentUser.set(user);
    }, { allowSignalWrites: true });

    // Limpiar el effect cuando el servicio se destruya (aunque es singleton, es buena práctica)
    this.destroyRef.onDestroy(() => {
      this.effectCleanup?.destroy();
    });
  }

  // Verificar si el usuario tiene un rol específico
  hasRole(role: string): boolean {
    const user = this.currentUser();
    return user?.roles?.includes(role) ?? false;
  }

  // Verificar si el usuario tiene alguno de los roles especificados
  hasAnyRole(roles: string[]): boolean {
    const user = this.currentUser();
    if (!user?.roles) return false;
    return roles.some(role => user.roles.includes(role));
  }

  // Verificar si el usuario tiene todos los roles especificados
  hasAllRoles(roles: string[]): boolean {
    const user = this.currentUser();
    if (!user?.roles) return false;
    return roles.every(role => user.roles.includes(role));
  }

  // Verificar si es administrador (acceso completo)
  isAdmin(): boolean {
    return this.hasRole('Administrador');
  }

  // Verificar si puede acceder a productos/categorías
  canManageProducts(): boolean {
    return this.hasAnyRole(['Administrador', 'Almacenero']);
  }

  // Verificar si puede acceder a clientes
  canManageCustomers(): boolean {
    return this.hasRole('Administrador');
  }

  // Verificar si puede acceder a ventas
  canManageSales(): boolean {
    return this.hasAnyRole(['Administrador', 'Cajero']);
  }

  // Verificar si puede acceder al dashboard
  canViewDashboard(): boolean {
    return this.hasRole('Administrador');
  }

  // Verificar si puede usar POS
  canUsePOS(): boolean {
    return this.hasAnyRole(['Administrador', 'Cajero']);
  }
}


import { Injectable, computed, signal, effect } from '@angular/core';
import { AuthService, User } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class PermissionsService {
  private readonly currentUser = signal<User | null>(null);

  constructor(private authService: AuthService) {
    // Sincronizar con el signal del AuthService usando effect
    effect(() => {
      const user = this.authService.currentUser();
      this.currentUser.set(user);
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


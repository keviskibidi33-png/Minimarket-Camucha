import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

/**
 * Servicio para gestión de permisos y roles de usuario
 * Lee directamente del AuthService para evitar problemas con effect() en servicios singleton
 */
@Injectable({
  providedIn: 'root'
})
export class PermissionsService {
  constructor(private authService: AuthService) {}

  // Obtener el usuario actual directamente del AuthService
  private getCurrentUser() {
    return this.authService.currentUser();
  }

  // Verificar si el usuario tiene un rol específico
  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.roles?.includes(role) ?? false;
  }

  // Verificar si el usuario tiene alguno de los roles especificados
  hasAnyRole(roles: string[]): boolean {
    const user = this.getCurrentUser();
    if (!user?.roles) return false;
    return roles.some(role => user.roles.includes(role));
  }

  // Verificar si el usuario tiene todos los roles especificados
  hasAllRoles(roles: string[]): boolean {
    const user = this.getCurrentUser();
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


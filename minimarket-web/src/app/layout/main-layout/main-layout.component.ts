import { Component, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { PermissionsService } from '../../core/services/permissions.service';
import { ConcentrationModeService } from '../../core/services/concentration-mode.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterOutlet],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css'
})
export class MainLayoutComponent {
  sidebarOpen = true;

  constructor(
    public authService: AuthService,
    public permissionsService: PermissionsService,
    public concentrationModeService: ConcentrationModeService
  ) {
    // Sincronizar el sidebar con el modo de concentración
    effect(() => {
      if (this.concentrationModeService.isConcentrationMode()) {
        this.sidebarOpen = false;
      } else {
        // Al salir del modo concentración, restaurar el estado anterior del sidebar
        // (mantenerlo cerrado si estaba cerrado antes, o abrirlo si estaba abierto)
        // Por defecto, lo dejamos abierto al salir del modo concentración
        if (!this.sidebarOpen) {
          this.sidebarOpen = true;
        }
      }
    });
  }

  // Permissions helper - usando PermissionsService
  get permissions() {
    return {
      canViewDashboard: () => this.permissionsService.canViewDashboard(),
      canUsePOS: () => this.permissionsService.canUsePOS(),
      canManageSales: () => this.permissionsService.canManageSales(),
      canManageProducts: () => this.permissionsService.canManageProducts(),
      canManageCustomers: () => this.permissionsService.canManageCustomers(),
      isAdmin: () => this.permissionsService.isAdmin()
    };
  }

  toggleSidebar(): void {
    // Si está en modo concentración, no permitir abrir el sidebar
    if (this.concentrationModeService.isConcentrationMode()) {
      return;
    }
    this.sidebarOpen = !this.sidebarOpen;
  }

  logout(): void {
    this.authService.logout();
  }
}


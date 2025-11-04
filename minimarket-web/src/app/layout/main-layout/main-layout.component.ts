import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { PermissionsService } from '../../core/services/permissions.service';

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
    public permissionsService: PermissionsService
  ) {}

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
    this.sidebarOpen = !this.sidebarOpen;
  }

  logout(): void {
    this.authService.logout();
  }
}


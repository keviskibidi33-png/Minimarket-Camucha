import { Component, computed, effect, afterNextRender, OnInit, signal, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, RouterOutlet } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';
import { PermissionsService } from '../../core/services/permissions.service';
import { ConcentrationModeService } from '../../core/services/concentration-mode.service';
import { BrandSettingsService, BrandSettings } from '../../core/services/brand-settings.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterOutlet],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.css'
})
export class MainLayoutComponent implements OnInit {
  sidebarOpen = true;
  brandSettings = signal<BrandSettings | null>(null);
  private readonly destroyRef = inject(DestroyRef);
  private concentrationEffectCleanup?: ReturnType<typeof effect>;

  constructor(
    public authService: AuthService,
    public permissionsService: PermissionsService,
    public concentrationModeService: ConcentrationModeService,
    private brandSettingsService: BrandSettingsService,
    private titleService: Title
  ) {}

  ngOnInit(): void {
    this.loadBrandSettings();
    
    // Sincronizar el sidebar con el modo de concentración
    // Usar afterNextRender para asegurar contexto de inyección válido
    afterNextRender(() => {
      this.concentrationEffectCleanup = effect(() => {
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

      // Limpiar el effect cuando el componente se destruya
      this.destroyRef.onDestroy(() => {
        this.concentrationEffectCleanup?.destroy();
      });
    });
  }

  loadBrandSettings(): void {
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        this.brandSettings.set(settings);
        // Actualizar título de la página
        if (settings?.storeName) {
          this.titleService.setTitle(`${settings.storeName} - Admin`);
        } else {
          this.titleService.setTitle('Minimarket Camucha - Admin');
        }
      },
      error: (error) => {
        console.error('Error loading brand settings:', error);
        this.titleService.setTitle('Minimarket Camucha - Admin');
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


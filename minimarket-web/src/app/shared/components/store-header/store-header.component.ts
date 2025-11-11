import { Component, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
import { BrandSettingsService, BrandSettings } from '../../../core/services/brand-settings.service';
import { ProductAutocompleteComponent } from '../product-autocomplete/product-autocomplete.component';
import { ClickOutsideDirective } from '../../directives/click-outside.directive';
import { Product } from '../../../core/services/products.service';

@Component({
  selector: 'app-store-header',
  standalone: true,
  imports: [CommonModule, RouterModule, ProductAutocompleteComponent, ClickOutsideDirective],
  templateUrl: './store-header.component.html',
  styleUrl: './store-header.component.css'
})
export class StoreHeaderComponent implements OnInit {
  // Usar computed para obtener el contador del carrito directamente
  // Es más eficiente que effect() y se actualiza automáticamente
  cartItemsCount = computed(() => {
    const items = this.cartService.getCartItems()();
    return items.reduce((sum, item) => sum + item.quantity, 0);
  });

  // Estado de autenticación
  isAuthenticated = computed(() => this.authService.isAuthenticated());
  currentUser = computed(() => this.authService.currentUser());
  showUserMenu = signal(false);
  brandSettings = signal<BrandSettings | null>(null);

  constructor(
    private cartService: CartService,
    private authService: AuthService,
    private router: Router,
    private brandSettingsService: BrandSettingsService
  ) {
    // No necesitamos effect aquí, computed es suficiente y más eficiente
    // El computed se actualizará automáticamente cuando cambie el carrito
  }

  ngOnInit(): void {
    this.loadBrandSettings();
  }

  loadBrandSettings(): void {
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        this.brandSettings.set(settings);
      },
      error: (error) => {
        console.error('Error loading brand settings:', error);
      }
    });
  }

  toggleUserMenu() {
    this.showUserMenu.set(!this.showUserMenu());
  }

  goToLogin() {
    this.router.navigate(['/auth/login']);
    this.showUserMenu.set(false);
  }

  goToRegister() {
    this.router.navigate(['/auth/register']);
    this.showUserMenu.set(false);
  }

  goToProfile() {
    this.router.navigate(['/perfil']);
    this.showUserMenu.set(false);
  }

  logout() {
    this.authService.logout();
    this.showUserMenu.set(false);
    this.router.navigate(['/']);
  }

  onSearch(term: string) {
    if (term.trim()) {
      // Navegar a la página de productos con el término de búsqueda
      this.router.navigate(['/tienda/productos'], {
        queryParams: { search: term.trim() }
      });
    } else {
      // Si no hay término, solo ir a productos
      this.router.navigate(['/tienda/productos']);
    }
  }

  onProductSelected(product: Product) {
    // Cuando se selecciona un producto, navegar a su página de detalle
    this.router.navigate(['/tienda/producto', product.id]);
  }
}


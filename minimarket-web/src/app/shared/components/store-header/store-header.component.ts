import { Component, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
import { BrandSettingsService, BrandSettings } from '../../../core/services/brand-settings.service';
import { PagesService, Page } from '../../../core/services/pages.service';
import { SettingsService } from '../../../core/services/settings.service';
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
  pages = signal<Page[]>([]);

  constructor(
    private cartService: CartService,
    private authService: AuthService,
    private router: Router,
    private brandSettingsService: BrandSettingsService,
    private pagesService: PagesService,
    private settingsService: SettingsService
  ) {
    // No necesitamos effect aquí, computed es suficiente y más eficiente
    // El computed se actualizará automáticamente cuando cambie el carrito
  }

  ngOnInit(): void {
    this.loadBrandSettings();
    this.loadPages();
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

  loadPages(): void {
    // Primero verificar si la funcionalidad global está activada
    this.settingsService.getByKey('enable_news_in_navbar').subscribe({
      next: (setting) => {
        const isEnabled = !setting || setting.value.toLowerCase() === 'true' || setting.value === '1';
        
        if (!isEnabled) {
          // Si está desactivado globalmente, no mostrar ninguna noticia
          this.pages.set([]);
          return;
        }
        
        // Si está activado, cargar las noticias
        this.pagesService.getAll(true).subscribe({
          next: (pages) => {
            // Filtrar solo las que deben mostrarse en el navbar (mostrarEnNavbar = true)
            // y que estén activas
            const sortedPages = pages
              .filter(page => {
                const isActive = page.activa === true;
                const showInNavbar = page.mostrarEnNavbar === true;
                return isActive && showInNavbar;
              })
              .sort((a, b) => {
                if (a.orden !== b.orden) {
                  return a.orden - b.orden;
                }
                return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
              });
            console.log('Páginas cargadas para navbar:', {
              total: pages.length,
              activas: pages.filter(p => p.activa).length,
              mostrarEnNavbar: pages.filter(p => p.mostrarEnNavbar).length,
              filtradas: sortedPages.length,
              pages: sortedPages.map(p => ({ titulo: p.titulo, activa: p.activa, mostrarEnNavbar: p.mostrarEnNavbar }))
            });
            this.pages.set(sortedPages);
          },
          error: (error) => {
            // Silenciar errores de conexión en el header para no molestar al usuario
            // Solo loguear en consola para debugging
            if (error.status !== 0) {
              console.error('Error loading pages:', error);
            }
            this.pages.set([]);
          }
        });
      },
      error: () => {
        // Si hay error al cargar la configuración, asumir que está activado (comportamiento por defecto)
        this.pagesService.getAll(true).subscribe({
          next: (pages) => {
            const sortedPages = pages
              .filter(page => {
                const isActive = page.activa === true;
                const showInNavbar = page.mostrarEnNavbar === true;
                return isActive && showInNavbar;
              })
              .sort((a, b) => {
                if (a.orden !== b.orden) {
                  return a.orden - b.orden;
                }
                return new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
              });
            this.pages.set(sortedPages);
          },
          error: (error) => {
            if (error.status !== 0) {
              console.error('Error loading pages:', error);
            }
            this.pages.set([]);
          }
        });
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

  onLogoError(event: Event): void {
    const img = event.target as HTMLImageElement | null;
    if (img) {
      img.src = 'assets/logo.png';
    }
  }
}


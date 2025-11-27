import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService, CartItem } from '../../../core/services/cart.service';
import { SettingsService } from '../../../core/services/settings.service';
import { StoreHeaderComponent } from '../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../shared/components/store-footer/store-footer.component';
import { SedesService, Sede } from '../../../core/services/sedes.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, StoreHeaderComponent, StoreFooterComponent],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent implements OnInit {
  discountCode = signal('');
  
  // Configuración de IGV
  applyIgvToCart = signal(false); // Por defecto false
  igvRate = signal(0.18); // 18% IGV por defecto
  
  // Configuración de envío simplificado
  fixedShippingPrice = signal(8.00); // Precio fijo por defecto
  freeShippingThreshold = signal(20.00); // Umbral de envío gratis por defecto

  // Información de sede guardada (si hay retiro en tienda)
  selectedSede = signal<Sede | null>(null);
  shippingMethod = signal<'delivery' | 'pickup'>('delivery');

  cartItems!: ReturnType<typeof CartService.prototype.getCartItems>;

  constructor(
    private cartService: CartService,
    private settingsService: SettingsService,
    private sedesService: SedesService
  ) {
    // Inicializar cartItems después del constructor
    this.cartItems = this.cartService.getCartItems();
  }

  ngOnInit() {
    this.loadIgvSettings();
    this.loadShippingSettings();
    this.loadSavedShippingData();
  }

  private loadSavedShippingData(): void {
    try {
      const savedShipping = localStorage.getItem('checkout-shipping');
      if (savedShipping) {
        const shippingData = JSON.parse(savedShipping);
        if (shippingData?.shippingMethod) {
          this.shippingMethod.set(shippingData.shippingMethod);
        }
        if (shippingData?.selectedSede) {
          // Cargar la sede completa desde el servicio
          this.sedesService.getById(shippingData.selectedSede.id).subscribe({
            next: (sede) => {
              this.selectedSede.set(sede);
            },
            error: () => {
              // Si no se puede cargar, usar los datos guardados
              this.selectedSede.set(shippingData.selectedSede as Sede);
            }
          });
        }
      }
    } catch (error) {
      console.error('Error loading saved shipping data:', error);
    }
  }

  private loadIgvSettings(): void {
    this.settingsService.getByKey('apply_igv_to_cart').subscribe({
      next: (setting) => {
        if (setting) {
          const value = setting.value?.trim() || '';
          const enabled = value.toLowerCase() === 'true' || value === '1';
          this.applyIgvToCart.set(enabled);
        } else {
          this.applyIgvToCart.set(false);
        }
      },
      error: () => {
        this.applyIgvToCart.set(false);
      }
    });
    
    this.settingsService.getByKey('igv_rate').subscribe({
      next: (setting) => {
        if (setting) {
          const value = setting.value?.trim() || '0.18';
          const rate = parseFloat(value) || 0.18;
          this.igvRate.set(rate);
        } else {
          this.igvRate.set(0.18);
        }
      },
      error: () => {
        this.igvRate.set(0.18);
      }
    });
  }

  private loadShippingSettings(): void {
    // Cargar precio fijo de envío
    this.settingsService.getByKey('fixed_shipping_price').subscribe({
      next: (setting) => {
        if (setting && setting.value) {
          const price = parseFloat(setting.value) || 8.00;
          this.fixedShippingPrice.set(price);
        }
      },
      error: () => {
        // Mantener valor por defecto
      }
    });

    // Cargar umbral de envío gratis
    this.settingsService.getByKey('free_shipping_threshold').subscribe({
      next: (setting) => {
        if (setting && setting.value) {
          const threshold = parseFloat(setting.value) || 20.00;
          this.freeShippingThreshold.set(threshold);
        }
      },
      error: () => {
        // Mantener valor por defecto
      }
    });
  }

  // Helper methods for template
  parseInt = parseInt;

  subtotal = computed(() => {
    return this.cartItems().reduce((sum, item) => sum + item.subtotal, 0);
  });

  igv = computed(() => {
    if (!this.applyIgvToCart()) {
      return 0;
    }
    return this.subtotal() * this.igvRate();
  });

  discount = computed(() => {
    // TODO: Implementar lógica de descuento
    return 0;
  });

  // Calcular envío basado en el sistema simplificado
  shipping = computed(() => {
    const subtotalValue = this.subtotal();
    const threshold = this.freeShippingThreshold();
    const fixedPrice = this.fixedShippingPrice();
    
    // Si el subtotal es mayor o igual al umbral, envío gratis
    if (subtotalValue >= threshold) {
      return 0;
    }
    
    // Si el subtotal es menor al umbral, aplicar precio fijo
    return fixedPrice;
  });

  total = computed(() => {
    return this.subtotal() + this.igv() + this.shipping() - this.discount();
  });

  updateQuantity(item: CartItem, quantity: number) {
    if (quantity <= 0) {
      this.removeItem(item.productId);
    } else {
      this.cartService.updateQuantity(item.productId, quantity);
    }
  }

  removeItem(productId: number) {
    this.cartService.removeFromCart(productId);
  }

  clearAll() {
    this.cartService.clearCart();
  }

  applyDiscount() {
    // TODO: Implementar lógica de aplicación de descuento
    const code = this.discountCode().trim();
    if (code) {
      // Aquí se implementará la lógica de descuento
      // Por ahora solo validamos que haya un código
    }
  }

  proceedToPayment() {
    // La navegación se hace con routerLink en el botón
  }

  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }
}


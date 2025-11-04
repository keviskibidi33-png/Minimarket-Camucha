import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService, CartItem } from '../../../core/services/cart.service';
import { SettingsService } from '../../../core/services/settings.service';
import { StoreHeaderComponent } from '../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../shared/components/store-footer/store-footer.component';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, StoreHeaderComponent, StoreFooterComponent],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent implements OnInit {
  discountCode = signal('');
  // El shipping se calculará en el paso de checkout/envio
  // Por ahora no se muestra en el carrito hasta que se seleccione el método
  shipping = signal(0); // Se calculará en checkout/envio
  
  // Configuración de IGV
  applyIgvToCart = signal(false); // Por defecto false
  igvRate = signal(0.18); // 18% IGV

  cartItems!: ReturnType<typeof CartService.prototype.getCartItems>;

  constructor(
    private cartService: CartService,
    private settingsService: SettingsService
  ) {
    // Inicializar cartItems después del constructor
    this.cartItems = this.cartService.getCartItems();
  }

  ngOnInit() {
    // Verificar si se debe aplicar IGV al carrito
    this.settingsService.isSettingEnabled('apply_igv_to_cart', false).subscribe({
      next: (enabled) => {
        this.applyIgvToCart.set(enabled);
      },
      error: () => {
        this.applyIgvToCart.set(false);
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
    console.log('Applying discount code:', this.discountCode());
  }

  proceedToPayment() {
    // La navegación se hace con routerLink en el botón
  }

  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }
}


import { Component, OnInit, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService, CartItem } from '../../../core/services/cart.service';
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
  cartItems = this.cartService.getCartItems();
  discountCode = signal('');
  shipping = signal(2.00); // Shipping fijo por ahora

  constructor(private cartService: CartService) {}

  ngOnInit() {
    // El cartItems ya está asignado directamente al signal del servicio
  }

  subtotal = computed(() => {
    return this.cartItems().reduce((sum, item) => sum + item.subtotal, 0);
  });

  discount = computed(() => {
    // TODO: Implementar lógica de descuento
    return 0;
  });

  total = computed(() => {
    return this.subtotal() + this.shipping() - this.discount();
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


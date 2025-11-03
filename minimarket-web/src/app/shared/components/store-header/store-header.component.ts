import { Component, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-store-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './store-header.component.html',
  styleUrl: './store-header.component.css'
})
export class StoreHeaderComponent {
  searchTerm = signal('');
  cartItemsCount = signal(0);

  constructor(private cartService: CartService) {
    // Actualizar contador cuando cambia el carrito
    effect(() => {
      const items = this.cartService.getCartItems()();
      this.cartItemsCount.set(items.reduce((sum, item) => sum + item.quantity, 0));
    });
  }

  onSearch() {
    // TODO: Implementar búsqueda
    console.log('Searching for:', this.searchTerm());
  }
}


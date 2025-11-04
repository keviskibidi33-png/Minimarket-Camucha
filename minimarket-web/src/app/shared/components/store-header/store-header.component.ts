import { Component, signal, computed } from '@angular/core';
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

  // Usar computed para obtener el contador del carrito directamente
  // Es más eficiente que effect() y se actualiza automáticamente
  cartItemsCount = computed(() => {
    const items = this.cartService.getCartItems()();
    return items.reduce((sum, item) => sum + item.quantity, 0);
  });

  constructor(private cartService: CartService) {
    // No necesitamos effect aquí, computed es suficiente y más eficiente
    // El computed se actualizará automáticamente cuando cambie el carrito
  }

  onSearch() {
    // TODO: Implementar búsqueda
    console.log('Searching for:', this.searchTerm());
  }
}


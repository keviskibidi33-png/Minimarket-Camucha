import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { ProductAutocompleteComponent } from '../product-autocomplete/product-autocomplete.component';
import { Product } from '../../../core/services/products.service';

@Component({
  selector: 'app-store-header',
  standalone: true,
  imports: [CommonModule, RouterModule, ProductAutocompleteComponent],
  templateUrl: './store-header.component.html',
  styleUrl: './store-header.component.css'
})
export class StoreHeaderComponent {
  // Usar computed para obtener el contador del carrito directamente
  // Es más eficiente que effect() y se actualiza automáticamente
  cartItemsCount = computed(() => {
    const items = this.cartService.getCartItems()();
    return items.reduce((sum, item) => sum + item.quantity, 0);
  });

  constructor(
    private cartService: CartService,
    private router: Router
  ) {
    // No necesitamos effect aquí, computed es suficiente y más eficiente
    // El computed se actualizará automáticamente cuando cambie el carrito
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


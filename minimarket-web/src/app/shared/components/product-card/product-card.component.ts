import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Product } from '../../../core/services/products.service';
import { CartService } from '../../../core/services/cart.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.css'
})
export class ProductCardComponent {
  @Input() product!: Product;
  @Input() showAddButton: boolean = true;
  private isAdding = false;

  constructor(
    private cartService: CartService,
    private toastService: ToastService
  ) {}

  addToCart() {
    // Prevenir doble click
    if (this.isAdding || this.product.stock === 0) {
      return;
    }

    this.isAdding = true;
    
    if (this.product.stock > 0) {
      // Pasar el GUID directamente, el CartService lo convertirá de forma consistente
      this.cartService.addToCart({
        id: this.product.id, // Pasar el GUID string directamente
        name: this.product.name,
        imageUrl: this.product.imageUrl,
        salePrice: this.product.salePrice,
        stock: this.product.stock
      }, 1);
      
      // Obtener cantidad total de productos en el carrito
      const totalItems = this.cartService.getTotalItems();
      const message = totalItems === 1 
        ? `"${this.product.name}" agregado al carrito`
        : `"${this.product.name}" agregado al carrito (Total: ${totalItems} productos)`;
      
      // Mostrar notificación de éxito
      this.toastService.success(message, 3000);
    }

    // Resetear el flag después de un breve delay
    setTimeout(() => {
      this.isAdding = false;
    }, 500);
  }

  getPriceFormatted(): string {
    return `S/ ${this.product.salePrice.toFixed(2)}`;
  }
}


import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Product } from '../../../core/services/products.service';
import { CartService } from '../../../core/services/cart.service';

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

  constructor(private cartService: CartService) {}

  addToCart() {
    if (this.product.stock > 0) {
      this.cartService.addToCart({
        id: parseInt(this.product.id),
        name: this.product.name,
        imageUrl: this.product.imageUrl,
        salePrice: this.product.salePrice,
        stock: this.product.stock
      });
    }
  }

  getPriceFormatted(): string {
    return `S/ ${this.product.salePrice.toFixed(2)}`;
  }
}


import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { ProductsService, Product } from '../../../core/services/products.service';
import { CartService } from '../../../core/services/cart.service';
import { StoreHeaderComponent } from '../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../shared/components/store-footer/store-footer.component';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    StoreHeaderComponent, 
    StoreFooterComponent
  ],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css'
})
export class ProductDetailComponent implements OnInit {
  product = signal<Product | null>(null);
  isLoading = signal(true);
  quantity = signal(1);
  selectedImage = signal(0);

  constructor(
    private productsService: ProductsService,
    private cartService: CartService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    const productId = this.route.snapshot.paramMap.get('id');
    if (productId) {
      this.loadProduct(productId);
    } else {
      this.router.navigate(['/tienda/productos']);
    }
  }

  loadProduct(id: string) {
    this.isLoading.set(true);
    this.productsService.getById(id).subscribe({
      next: (product) => {
        this.product.set(product);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading product:', error);
        this.isLoading.set(false);
        this.router.navigate(['/tienda/productos']);
      }
    });
  }

  increaseQuantity() {
    const current = this.quantity();
    const maxStock = this.product()?.stock || 0;
    if (current < maxStock) {
      this.quantity.set(current + 1);
    }
  }

  decreaseQuantity() {
    const current = this.quantity();
    if (current > 1) {
      this.quantity.set(current - 1);
    }
  }

  addToCart() {
    const product = this.product();
    if (product && product.stock > 0) {
      this.cartService.addToCart({
        id: parseInt(product.id),
        name: product.name,
        imageUrl: product.imageUrl,
        salePrice: product.salePrice,
        stock: product.stock
      }, this.quantity());
      
      // Opcional: Mostrar mensaje de éxito
      alert(`Se agregaron ${this.quantity()} ${product.name} al carrito`);
    }
  }

  getPriceFormatted(): string {
    const product = this.product();
    return product ? `S/ ${product.salePrice.toFixed(2)}` : '';
  }

  getCategoryName(): string {
    return this.product()?.categoryName || 'Productos';
  }
}


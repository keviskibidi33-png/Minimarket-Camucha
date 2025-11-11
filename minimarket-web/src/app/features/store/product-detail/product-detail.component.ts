import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { ProductsService, Product } from '../../../core/services/products.service';
import { CartService } from '../../../core/services/cart.service';
import { ToastService } from '../../../shared/services/toast.service';
import { AnalyticsService } from '../../../core/services/analytics.service';
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

  // Helper methods for template
  parseInt = parseInt;

  constructor(
    private productsService: ProductsService,
    private cartService: CartService,
    private toastService: ToastService,
    private analyticsService: AnalyticsService,
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
        
        // Trackear vista de producto
        this.analyticsService.trackProductView(id).subscribe({
          error: (error) => console.error('Error tracking product view:', error)
        });
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

  private isAdding = false;

  addToCart() {
    // Prevenir doble click
    if (this.isAdding) {
      return;
    }

    const product = this.product();
    if (product && product.stock > 0) {
      this.isAdding = true;
      
      // IMPORTANTE: Usar precio normal (sin descuento) cuando se agrega desde aquí
      // El descuento solo se aplica cuando se agrega desde la página de detalle de oferta
      // Pasar el GUID directamente, el CartService lo convertirá de forma consistente
      this.cartService.addToCart({
        id: product.id, // Pasar el GUID string directamente
        name: product.name,
        imageUrl: product.imageUrl,
        salePrice: product.salePrice, // Precio normal - sin descuento
        stock: product.stock
      }, this.quantity());
      
      // Obtener cantidad total de productos en el carrito
      const totalItems = this.cartService.getTotalItems();
      const quantityText = this.quantity() === 1 ? 'producto' : 'productos';
      const message = totalItems === this.quantity()
        ? `Se agregaron ${this.quantity()} ${quantityText} de "${product.name}" al carrito`
        : `Se agregaron ${this.quantity()} ${quantityText} de "${product.name}" al carrito (Total: ${totalItems} productos)`;
      
      // Mostrar notificación de éxito
      this.toastService.success(message, 3000);
      
      // Resetear el flag después de un breve delay
      setTimeout(() => {
        this.isAdding = false;
      }, 500);
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


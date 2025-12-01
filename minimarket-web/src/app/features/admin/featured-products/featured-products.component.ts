import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ProductsService, Product, UpdateProductDto } from '../../../core/services/products.service';
import { ToastService } from '../../../shared/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-featured-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './featured-products.component.html',
  styleUrl: './featured-products.component.css'
})
export class FeaturedProductsComponent implements OnInit {
  allProducts = signal<Product[]>([]);
  featuredProducts = signal<Product[]>([]);
  isLoading = signal(false);
  searchTerm = signal('');
  private hasShownAuthError = false; // Para evitar múltiples toasts de error de autenticación

  constructor(
    private productsService: ProductsService,
    private toastService: ToastService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.productsService.getAll({ 
      isActive: true,
      pageSize: 1000
    }).subscribe({
      next: (result) => {
        const products = result.items || [];
        this.allProducts.set(products);
        
        // Separar productos destacados
        const featured = products
          .filter(p => p.paginas && p.paginas['home'] === true)
          .sort((a, b) => {
            const ordenA = a.paginas?.['home_orden'] || 999;
            const ordenB = b.paginas?.['home_orden'] || 999;
            return ordenA - ordenB;
          });
        this.featuredProducts.set(featured);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.toastService.error('Error al cargar los productos');
        this.isLoading.set(false);
      }
    });
  }

  getAvailableProducts(): Product[] {
    const term = this.searchTerm().toLowerCase().trim();
    const featuredIds = new Set(this.featuredProducts().map(p => p.id));
    
    let available = this.allProducts().filter(p => !featuredIds.has(p.id));
    
    if (term) {
      available = available.filter(p => 
        p.name.toLowerCase().includes(term) ||
        p.code.toLowerCase().includes(term)
      );
    }
    
    return available;
  }

  addToFeatured(product: Product): void {
    // Verificar autenticación antes de actualizar
    if (!this.authService.isAuthenticated()) {
      this.toastService.error('Debes iniciar sesión para realizar esta acción');
      this.router.navigate(['/auth/login']);
      return;
    }

    const featured = this.featuredProducts();
    const maxOrden = featured.length > 0 
      ? Math.max(...featured.map(p => p.paginas?.['home_orden'] || 0))
      : 0;
    
    const updatedProduct: UpdateProductDto = {
      id: product.id,
      code: product.code,
      name: product.name,
      description: product.description,
      purchasePrice: product.purchasePrice,
      salePrice: product.salePrice,
      stock: product.stock,
      minimumStock: product.minimumStock,
      categoryId: product.categoryId,
      imageUrl: product.imageUrl,
      isActive: product.isActive,
      paginas: {
        ...product.paginas,
        home: true,
        home_orden: maxOrden + 1
      }
    };

    this.productsService.update(updatedProduct).subscribe({
      next: () => {
        this.toastService.success('Producto agregado a destacados');
        this.loadProducts();
      },
      error: (error) => {
        // Manejar errores 401 específicamente
        if (error?.status === 401) {
          this.toastService.error('Tu sesión ha expirado. Por favor, inicia sesión nuevamente');
          this.authService.logout();
          this.router.navigate(['/auth/login']);
        } else {
          console.error('Error updating product:', error);
          this.toastService.error('Error al agregar el producto');
        }
      }
    });
  }

  removeFromFeatured(product: Product): void {
    // Verificar autenticación antes de actualizar
    if (!this.authService.isAuthenticated()) {
      this.toastService.error('Debes iniciar sesión para realizar esta acción');
      this.router.navigate(['/auth/login']);
      return;
    }

    const updatedProduct: UpdateProductDto = {
      id: product.id,
      code: product.code,
      name: product.name,
      description: product.description,
      purchasePrice: product.purchasePrice,
      salePrice: product.salePrice,
      stock: product.stock,
      minimumStock: product.minimumStock,
      categoryId: product.categoryId,
      imageUrl: product.imageUrl,
      isActive: product.isActive,
      paginas: {
        ...product.paginas,
        home: false
      }
    };

    this.productsService.update(updatedProduct).subscribe({
      next: () => {
        this.toastService.success('Producto removido de destacados');
        this.loadProducts();
      },
      error: (error) => {
        // Manejar errores 401 específicamente
        if (error?.status === 401) {
          this.toastService.error('Tu sesión ha expirado. Por favor, inicia sesión nuevamente');
          this.authService.logout();
          this.router.navigate(['/auth/login']);
        } else {
          console.error('Error updating product:', error);
          this.toastService.error('Error al remover el producto');
        }
      }
    });
  }

  moveUp(product: Product): void {
    const featured = [...this.featuredProducts()];
    const index = featured.findIndex(p => p.id === product.id);
    
    if (index <= 0) return;

    const currentOrden = product.paginas?.['home_orden'] || index + 1;
    const prevOrden = featured[index - 1].paginas?.['home_orden'] || index;

    // Intercambiar órdenes
    this.updateProductOrder(product, prevOrden);
    this.updateProductOrder(featured[index - 1], currentOrden);
  }

  moveDown(product: Product): void {
    const featured = [...this.featuredProducts()];
    const index = featured.findIndex(p => p.id === product.id);
    
    if (index >= featured.length - 1) return;

    const currentOrden = product.paginas?.['home_orden'] || index + 1;
    const nextOrden = featured[index + 1].paginas?.['home_orden'] || index + 2;

    // Intercambiar órdenes
    this.updateProductOrder(product, nextOrden);
    this.updateProductOrder(featured[index + 1], currentOrden);
  }

  private updateProductOrder(product: Product, orden: number): void {
    // Verificar autenticación antes de actualizar
    if (!this.authService.isAuthenticated()) {
      // No mostrar toast para cada actualización individual, solo loguear
      console.warn('Usuario no autenticado, no se puede actualizar el orden');
      return;
    }

    const updatedProduct: UpdateProductDto = {
      id: product.id,
      code: product.code,
      name: product.name,
      description: product.description,
      purchasePrice: product.purchasePrice,
      salePrice: product.salePrice,
      stock: product.stock,
      minimumStock: product.minimumStock,
      categoryId: product.categoryId,
      imageUrl: product.imageUrl,
      isActive: product.isActive,
      paginas: {
        ...product.paginas,
        home: true,
        home_orden: orden
      }
    };

    this.productsService.update(updatedProduct).subscribe({
      next: () => {
        this.loadProducts();
      },
      error: (error) => {
        // Manejar errores 401 específicamente sin mostrar múltiples toasts
        if (error?.status === 401) {
          // Solo mostrar el toast una vez si es el primer error 401
          if (!this.hasShownAuthError) {
            this.toastService.error('Tu sesión ha expirado. Por favor, inicia sesión nuevamente');
            this.hasShownAuthError = true;
            setTimeout(() => {
              this.authService.logout();
              this.router.navigate(['/auth/login']);
            }, 1000);
          }
        } else {
          console.error('Error updating product order:', error);
          // No mostrar toast para errores de orden individual para evitar spam
        }
      }
    });
  }
}


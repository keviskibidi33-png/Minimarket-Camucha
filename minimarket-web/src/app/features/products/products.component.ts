import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ProductsService, Product, UpdateProductDto } from '../../core/services/products.service';
import { CategoriesService } from '../../core/services/categories.service';
import { ToastService } from '../../shared/services/toast.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ConfirmDialogComponent],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class ProductsComponent implements OnInit, OnDestroy {
  products = signal<Product[]>([]);
  filteredProducts = signal<Product[]>([]);
  categories = signal<{id: string; name: string}[]>([]);
  isLoading = signal(false);
  searchTerm = signal('');
  selectedCategory = signal<string>('');
  selectedProducts = signal<Set<string>>(new Set());
  currentPage = signal(1);
  pageSize = 10;
  totalProducts = signal(0);

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  // Modal de confirmación
  showDeleteModal = signal(false);
  productToDelete = signal<Product | null>(null);
  isTogglingActive = signal<Set<string>>(new Set());

  constructor(
    private productsService: ProductsService,
    private categoriesService: CategoriesService,
    private toastService: ToastService,
    private router: Router
  ) {
    // Configurar búsqueda en tiempo real con debounce
    this.searchSubject.pipe(
      debounceTime(300), // Esperar 300ms después de que el usuario deje de escribir
      distinctUntilChanged(), // Solo buscar si el término cambió
      takeUntil(this.destroy$)
    ).subscribe(searchTerm => {
      this.searchTerm.set(searchTerm);
      this.currentPage.set(1);
      this.loadProducts();
    });
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories(): void {
    this.categoriesService.getAllWithoutPagination().subscribe({
      next: (categories) => this.categories.set(categories.map((c: any) => ({ id: c.id, name: c.name }))),
      error: (error) => console.error('Error loading categories:', error)
    });
  }

  loadProducts(): void {
    this.isLoading.set(true);
    // Limpiar productos anteriores antes de cargar nuevos
    this.products.set([]);
    this.filteredProducts.set([]);
    
    this.productsService.getAll({
      page: this.currentPage(),
      pageSize: this.pageSize,
      searchTerm: this.searchTerm() || undefined,
      categoryId: this.selectedCategory() || undefined
    }).subscribe({
      next: (pagedResult) => {
        this.products.set(pagedResult.items);
        this.filteredProducts.set(pagedResult.items);
        this.totalProducts.set(pagedResult.totalCount);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.isLoading.set(false);
        // El error se maneja con toast desde el interceptor si existe
        // Aquí podríamos agregar un signal de error si es necesario
      }
    });
  }

  onSearch(): void {
    // Este método ya no es necesario porque la búsqueda se hace automáticamente
    // Pero lo mantenemos por si se necesita para el evento keyup.enter
    this.currentPage.set(1);
    this.loadProducts();
  }

  onSearchInput(value: string): void {
    // Emitir el valor al Subject para búsqueda en tiempo real
    this.searchSubject.next(value);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onCategoryChange(): void {
    this.currentPage.set(1);
    this.loadProducts();
  }

  toggleSelectProduct(productId: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    const selected = new Set(this.selectedProducts());
    
    if (checked) {
      selected.add(productId);
    } else {
      selected.delete(productId);
    }
    
    this.selectedProducts.set(selected);
  }

  toggleSelectAll(event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    const selected = new Set<string>();
    
    if (checked) {
      this.products().forEach(p => selected.add(p.id));
    }
    
    this.selectedProducts.set(selected);
  }

  editProduct(productId: string): void {
    this.router.navigate(['/admin/productos/editar', productId]);
  }

  openDeleteModal(product: Product): void {
    this.productToDelete.set(product);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal(): void {
    this.showDeleteModal.set(false);
    this.productToDelete.set(null);
  }

  confirmDelete(): void {
    const product = this.productToDelete();
    if (!product) return;

    this.productsService.delete(product.id).subscribe({
      next: () => {
        this.toastService.success('Producto eliminado exitosamente');
        this.closeDeleteModal();
        this.loadProducts();
      },
      error: (error) => {
        console.error('Error deleting product:', error);
        const errorMessage = error.error?.message || error.error?.errors?.[0] || 'Error al eliminar el producto';
        this.toastService.error(errorMessage);
        this.closeDeleteModal();
      }
    });
  }

  toggleProductActive(product: Product, event: Event): void {
    event.stopPropagation(); // Evitar que se active el checkbox de selección
    
    // Si ya está procesando este producto, no hacer nada
    if (this.isTogglingActive().has(product.id)) {
      return;
    }

    const newActiveState = !product.isActive;
    
    // Agregar a la lista de productos siendo procesados
    const toggling = new Set(this.isTogglingActive());
    toggling.add(product.id);
    this.isTogglingActive.set(toggling);

    // Crear el DTO de actualización con todos los datos del producto
    const updateDto: UpdateProductDto = {
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
      expirationDate: product.expirationDate,
      isActive: newActiveState
    };

    this.productsService.update(updateDto).subscribe({
      next: (updatedProduct) => {
        // Actualizar el producto en la lista local
        const products = this.products().map(p => 
          p.id === updatedProduct.id ? updatedProduct : p
        );
        this.products.set(products);
        this.filteredProducts.set(products);
        
        // Remover de la lista de productos siendo procesados
        const toggling = new Set(this.isTogglingActive());
        toggling.delete(product.id);
        this.isTogglingActive.set(toggling);

        const message = newActiveState 
          ? 'Producto activado exitosamente' 
          : 'Producto desactivado exitosamente';
        this.toastService.success(message);
      },
      error: (error) => {
        console.error('Error toggling product active state:', error);
        const errorMessage = error.error?.message || error.error?.errors?.[0] || 'Error al actualizar el estado del producto';
        this.toastService.error(errorMessage);
        
        // Remover de la lista de productos siendo procesados
        const toggling = new Set(this.isTogglingActive());
        toggling.delete(product.id);
        this.isTogglingActive.set(toggling);
      }
    });
  }

  getStockStatus(stock: number, minimumStock: number): { text: string; class: string } {
    if (stock === 0) {
      return { text: 'Agotado', class: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300' };
    } else if (stock <= minimumStock) {
      return { text: 'Bajo Stock', class: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300' };
    } else {
      return { text: 'Activo', class: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300' };
    }
  }

  getStockColor(stock: number, minimumStock: number): string {
    if (stock === 0) return 'text-red-500';
    if (stock <= minimumStock) return 'text-yellow-500';
    return '';
  }

  Math = Math;
}


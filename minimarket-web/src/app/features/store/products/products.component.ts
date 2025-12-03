import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { ProductsService, Product } from '../../../core/services/products.service';
import { CategoriesService, CategoryDto } from '../../../core/services/categories.service';
import { CartService } from '../../../core/services/cart.service';
import { ToastService } from '../../../shared/services/toast.service';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { StoreHeaderComponent } from '../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../shared/components/store-footer/store-footer.component';
import { ProductCardComponent } from '../../../shared/components/product-card/product-card.component';

@Component({
  selector: 'app-store-products',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    StoreHeaderComponent, 
    StoreFooterComponent,
    ProductCardComponent
  ],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css',
  animations: [
    trigger('viewTransition', [
      transition('grid <=> list', [
        style({ opacity: 0, transform: 'scale(0.95)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'scale(1)' }))
      ])
    ])
  ]
})
export class StoreProductsComponent implements OnInit {
  products = signal<Product[]>([]);
  filteredProducts = signal<Product[]>([]);
  categories = signal<CategoryDto[]>([]);
  isLoading = signal(true);

  // Filters
  selectedCategory = signal<string | null>(null);
  minPrice = signal(0);
  maxPrice = signal(1000);
  priceRangeMin = signal(0); // Rango mínimo disponible (se calculará dinámicamente)
  priceRangeMax = signal(1000); // Rango máximo disponible (se calculará dinámicamente)
  searchTerm = signal('');
  
  // Slider state
  isDraggingMin = signal(false);
  isDraggingMax = signal(false);
  
  // View mode
  viewMode = signal<'grid' | 'list'>('grid');
  sortBy = signal<string>('relevance');
  
  // Pagination
  currentPage = signal(1);
  pageSize = 12;
  totalItems = signal(0);

  constructor(
    private productsService: ProductsService,
    private categoriesService: CategoriesService,
    private cartService: CartService,
    private toastService: ToastService,
    private analyticsService: AnalyticsService,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    // Trackear vista de página
    this.analyticsService.trackPageView('tienda/productos').subscribe({
      error: (error) => console.error('Error tracking page view:', error)
    });
    
    // Obtener categoría y término de búsqueda de query params
    this.route.queryParams.subscribe(params => {
      if (params['category']) {
        this.selectedCategory.set(params['category']);
      }
      if (params['search']) {
        this.searchTerm.set(params['search']);
      }
      this.loadProducts();
    });
    
    this.loadCategories();
  }

  loadProducts() {
    this.isLoading.set(true);
    // Limpiar productos anteriores antes de cargar nuevos
    this.products.set([]);
    this.filteredProducts.set([]);
    
    // Primero obtener TODOS los productos (sin paginación) para calcular el rango completo
    // Esto permite que el slider se adapte a todos los productos disponibles, no solo a la página actual
    this.productsService.getAll({
      isActive: true,
      categoryId: this.selectedCategory() || undefined,
      searchTerm: this.searchTerm() || undefined,
      page: 1,
      pageSize: 10000 // Obtener todos los productos para calcular el rango
    }).subscribe({
      next: (result) => {
        // Asegurar que siempre sea un array válido
        const allProducts = Array.isArray(result?.items) ? result.items : (Array.isArray(result) ? result : []);
        
        // Calcular rango de precios disponible de forma inteligente
        if (allProducts.length > 0) {
          const prices = allProducts.map(p => p.salePrice);
          const minAvailable = Math.floor(Math.min(...prices));
          const maxAvailable = Math.ceil(Math.max(...prices));
          
          // Calcular márgenes (5% del rango o mínimo 5 unidades)
          const range = maxAvailable - minAvailable;
          const margin = Math.max(5, Math.ceil(range * 0.05));
          
          const newMinRange = Math.max(0, minAvailable - margin);
          const newMaxRange = maxAvailable + margin;
          
          // Actualizar rango disponible de forma inteligente
          const isFirstLoad = this.priceRangeMin() === 0 && this.priceRangeMax() === 1000;
          
          if (isFirstLoad) {
            // Primera carga: establecer el rango completo
            this.priceRangeMin.set(newMinRange);
            this.priceRangeMax.set(newMaxRange);
            // Ajustar valores iniciales al rango completo
            this.minPrice.set(newMinRange);
            this.maxPrice.set(newMaxRange);
          } else {
            // Cargas posteriores: adaptar el rango si es necesario
            // Expandir el rango si los nuevos productos tienen precios fuera del rango actual
            if (newMinRange < this.priceRangeMin()) {
              this.priceRangeMin.set(newMinRange);
            }
            if (newMaxRange > this.priceRangeMax()) {
              this.priceRangeMax.set(newMaxRange);
            }
            
            // Ajustar valores del usuario si están fuera del nuevo rango
            if (this.minPrice() < this.priceRangeMin()) {
              this.minPrice.set(this.priceRangeMin());
            }
            if (this.maxPrice() > this.priceRangeMax()) {
              this.maxPrice.set(this.priceRangeMax());
            }
          }
        }
        
        // Ahora aplicar paginación y filtros de precio a los productos
        let filtered = [...allProducts];
        
        // Filtrar por precio
        filtered = filtered.filter(p => 
          p.salePrice >= this.minPrice() && p.salePrice <= this.maxPrice()
        );
        
        // Ordenar
        filtered = this.sortProducts(filtered);
        
        // Aplicar paginación
        const startIndex = (this.currentPage() - 1) * this.pageSize;
        const endIndex = startIndex + this.pageSize;
        const paginatedProducts = filtered.slice(startIndex, endIndex);
        
        this.products.set(paginatedProducts);
        this.filteredProducts.set(filtered); // Guardar todos los productos filtrados para el total
        this.totalItems.set(filtered.length);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.isLoading.set(false);
      }
    });
  }

  loadCategories() {
    this.categoriesService.getAllWithoutPagination().subscribe({
      next: (categories) => {
        this.categories.set(categories.filter((c: any) => c.isActive));
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  sortProducts(products: Product[]): Product[] {
    const sorted = [...products];
    switch (this.sortBy()) {
      case 'price-asc':
        return sorted.sort((a, b) => a.salePrice - b.salePrice);
      case 'price-desc':
        return sorted.sort((a, b) => b.salePrice - a.salePrice);
      case 'name-asc':
        return sorted.sort((a, b) => a.name.localeCompare(b.name));
      case 'name-desc':
        return sorted.sort((a, b) => b.name.localeCompare(a.name));
      default:
        return sorted;
    }
  }

  applyFilters() {
    this.currentPage.set(1);
    this.loadProducts();
  }

  clearFilters() {
    this.selectedCategory.set(null);
    this.minPrice.set(this.priceRangeMin());
    this.maxPrice.set(this.priceRangeMax());
    this.searchTerm.set('');
    this.applyFilters();
  }

  // Métodos para el slider de rango de precios
  getMinPricePercent(): number {
    const range = this.priceRangeMax() - this.priceRangeMin();
    if (range === 0) return 0;
    return ((this.minPrice() - this.priceRangeMin()) / range) * 100;
  }

  getMaxPricePercent(): number {
    const range = this.priceRangeMax() - this.priceRangeMin();
    if (range === 0) return 100;
    return ((this.maxPrice() - this.priceRangeMin()) / range) * 100;
  }

  getRangeWidth(): number {
    return this.getMaxPricePercent() - this.getMinPricePercent();
  }

  onMinPriceInput(event: Event) {
    const value = parseFloat((event.target as HTMLInputElement).value) || this.priceRangeMin();
    const clampedValue = Math.max(this.priceRangeMin(), Math.min(value, this.maxPrice() - 1));
    this.minPrice.set(clampedValue);
    this.applyFilters();
  }

  onMaxPriceInput(event: Event) {
    const value = parseFloat((event.target as HTMLInputElement).value) || this.priceRangeMax();
    const clampedValue = Math.max(this.minPrice() + 1, Math.min(value, this.priceRangeMax()));
    this.maxPrice.set(clampedValue);
    this.applyFilters();
  }

  onSliderMouseDown(event: MouseEvent, handle: 'min' | 'max') {
    event.preventDefault();
    event.stopPropagation();
    if (handle === 'min') {
      this.isDraggingMin.set(true);
    } else {
      this.isDraggingMax.set(true);
    }
    
    const slider = (event.currentTarget as HTMLElement).closest('.price-slider-container')?.querySelector('.relative') as HTMLElement;
    if (!slider) return;

    const handleMouseMove = (e: MouseEvent) => {
      const rect = slider.getBoundingClientRect();
      const percent = Math.max(0, Math.min(100, ((e.clientX - rect.left) / rect.width) * 100));
      const range = this.priceRangeMax() - this.priceRangeMin();
      const value = this.priceRangeMin() + (percent / 100) * range;

      if (handle === 'min') {
        const clampedValue = Math.max(this.priceRangeMin(), Math.min(value, this.maxPrice() - 1));
        this.minPrice.set(Math.round(clampedValue * 100) / 100);
      } else {
        const clampedValue = Math.max(this.minPrice() + 1, Math.min(value, this.priceRangeMax()));
        this.maxPrice.set(Math.round(clampedValue * 100) / 100);
      }
      this.applyFilters();
    };

    const handleMouseUp = () => {
      this.isDraggingMin.set(false);
      this.isDraggingMax.set(false);
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
    };

    document.addEventListener('mousemove', handleMouseMove);
    document.addEventListener('mouseup', handleMouseUp);
  }

  onSliderTrackClick(event: MouseEvent) {
    // Evitar que el click en los handles active este método
    if ((event.target as HTMLElement).closest('.absolute.z-10')) {
      return;
    }
    
    const slider = (event.currentTarget as HTMLElement);
    if (!slider) return;

    const rect = slider.getBoundingClientRect();
    const percent = Math.max(0, Math.min(100, ((event.clientX - rect.left) / rect.width) * 100));
    const range = this.priceRangeMax() - this.priceRangeMin();
    const value = this.priceRangeMin() + (percent / 100) * range;

    // Determinar qué handle mover (el más cercano)
    const minDistance = Math.abs(value - this.minPrice());
    const maxDistance = Math.abs(value - this.maxPrice());

    if (minDistance < maxDistance) {
      const clampedValue = Math.max(this.priceRangeMin(), Math.min(value, this.maxPrice() - 1));
      this.minPrice.set(Math.round(clampedValue * 100) / 100);
    } else {
      const clampedValue = Math.max(this.minPrice() + 1, Math.min(value, this.priceRangeMax()));
      this.maxPrice.set(Math.round(clampedValue * 100) / 100);
    }
    this.applyFilters();
  }

  onCategoryChange(categoryId: string | null) {
    this.selectedCategory.set(categoryId);
    this.applyFilters();
  }

  onSortChange(sortBy: string) {
    this.sortBy.set(sortBy);
    this.loadProducts();
  }

  getCategoryName(): string {
    if (this.selectedCategory()) {
      const category = this.categories().find(c => c.id === this.selectedCategory());
      return category?.name || 'Productos';
    }
    return 'Todos los Productos';
  }

  getPages(): (number | string)[] {
    const totalPages = Math.ceil(this.totalItems() / this.pageSize);
    const current = this.currentPage();
    const pages: (number | string)[] = [];
    
    if (totalPages <= 7) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      if (current <= 3) {
        for (let i = 1; i <= 3; i++) pages.push(i);
        pages.push('...');
        pages.push(totalPages);
      } else if (current >= totalPages - 2) {
        pages.push(1);
        pages.push('...');
        for (let i = totalPages - 2; i <= totalPages; i++) pages.push(i);
      } else {
        pages.push(1);
        pages.push('...');
        pages.push(current - 1);
        pages.push(current);
        pages.push(current + 1);
        pages.push('...');
        pages.push(totalPages);
      }
    }
    
    return pages;
  }

  // Exponer Math y parseFloat para el template
  readonly Math = Math;
  readonly parseFloat = parseFloat;

  // Helper para verificar si es número
  isNumber(value: number | string): value is number {
    return typeof value === 'number';
  }

  // Formatear precio
  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }

  // Agregar al carrito
  addToCart(product: Product) {
    if (product.stock === 0) {
      return;
    }
    
    // IMPORTANTE: Usar precio normal (sin descuento) cuando se agrega desde aquí
    // El descuento solo se aplica cuando se agrega desde la página de detalle de oferta
    // Pasar el GUID directamente, el CartService lo convertirá de forma consistente
    this.cartService.addToCart({
      id: product.id, // Pasar el GUID string directamente
      name: product.name,
      imageUrl: product.imageUrl || '',
      salePrice: product.salePrice, // Precio normal - sin descuento
      stock: product.stock
    }, 1);
    
    // Obtener cantidad total de productos en el carrito
    const totalItems = this.cartService.getTotalItems();
    const message = totalItems === 1 
      ? `"${product.name}" agregado al carrito`
      : `"${product.name}" agregado al carrito (Total: ${totalItems} productos)`;
    
    // Mostrar notificación de éxito
    this.toastService.success(message, 3000);
  }
}


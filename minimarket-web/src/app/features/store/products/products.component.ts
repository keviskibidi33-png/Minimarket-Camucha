import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { ProductsService, Product } from '../../../core/services/products.service';
import { CategoriesService, CategoryDto } from '../../../core/services/categories.service';
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
  styleUrl: './products.component.css'
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
  searchTerm = signal('');
  
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
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    // Obtener categoría de query params
    this.route.queryParams.subscribe(params => {
      if (params['category']) {
        this.selectedCategory.set(params['category']);
      }
      this.loadProducts();
    });
    
    this.loadCategories();
  }

  loadProducts() {
    this.isLoading.set(true);
    this.productsService.getAll({
      isActive: true,
      categoryId: this.selectedCategory() || undefined,
      searchTerm: this.searchTerm() || undefined,
      page: this.currentPage(),
      pageSize: this.pageSize
    }).subscribe({
      next: (products) => {
        let filtered = [...products];
        
        // Filtrar por precio
        filtered = filtered.filter(p => 
          p.salePrice >= this.minPrice() && p.salePrice <= this.maxPrice()
        );
        
        // Ordenar
        filtered = this.sortProducts(filtered);
        
        this.products.set(filtered);
        this.filteredProducts.set(filtered);
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
    this.categoriesService.getAll().subscribe({
      next: (categories) => {
        this.categories.set(categories.filter(c => c.isActive));
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
    this.minPrice.set(0);
    this.maxPrice.set(1000);
    this.searchTerm.set('');
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

  // Exponer Math para el template
  Math = Math;
}


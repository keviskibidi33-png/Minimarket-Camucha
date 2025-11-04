import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ProductsService, Product } from '../../core/services/products.service';
import { CategoriesService } from '../../core/services/categories.service';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class ProductsComponent implements OnInit {
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

  constructor(
    private productsService: ProductsService,
    private categoriesService: CategoriesService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadProducts();
  }

  loadCategories(): void {
    this.categoriesService.getAll().subscribe({
      next: (categories) => this.categories.set(categories.map(c => ({ id: c.id, name: c.name }))),
      error: (error) => console.error('Error loading categories:', error)
    });
  }

  loadProducts(): void {
    this.isLoading.set(true);
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
    this.currentPage.set(1);
    this.loadProducts();
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

  deleteProduct(id: string): void {
    if (confirm('¿Está seguro de eliminar este producto?')) {
      this.productsService.delete(id).subscribe({
        next: () => {
          this.loadProducts();
        },
        error: (error) => {
          console.error('Error deleting product:', error);
          alert('Error al eliminar el producto');
        }
      });
    }
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


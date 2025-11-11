import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { CategoriesService, CategoryDto } from '../../../core/services/categories.service';
import { ProductsService, Product } from '../../../core/services/products.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-category-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './category-detail.component.html',
  styleUrl: './category-detail.component.css'
})
export class CategoryDetailComponent implements OnInit {
  category = signal<CategoryDto | null>(null);
  products = signal<Product[]>([]);
  isLoading = signal(false);
  categoryId = signal<string | null>(null);

  constructor(
    private route: ActivatedRoute,
    public router: Router,
    private categoriesService: CategoriesService,
    private productsService: ProductsService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.categoryId.set(id);
      this.loadCategory(id);
      this.loadProducts(id);
    }
  }

  loadCategory(id: string): void {
    this.isLoading.set(true);
    this.categoriesService.getById(id).subscribe({
      next: (category) => {
        this.category.set(category);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading category:', error);
        this.toastService.error('Error al cargar la categoría');
        this.router.navigate(['/admin/categorias']);
        this.isLoading.set(false);
      }
    });
  }

  loadProducts(categoryId: string): void {
    this.isLoading.set(true);
    this.productsService.getAll({ 
      categoryId: categoryId || undefined, 
      isActive: true, 
      page: 1, 
      pageSize: 100 
    }).subscribe({
      next: (result) => {
        this.products.set(result.items);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.toastService.error('Error al cargar los productos');
        this.products.set([]);
        this.isLoading.set(false);
      }
    });
  }

  formatDate(dateString?: string): string {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleDateString('es-PE', { year: 'numeric', month: '2-digit', day: '2-digit' });
  }

  formatPrice(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }

  isExpired(expirationDate?: string): boolean {
    if (!expirationDate) return false;
    return new Date(expirationDate) < new Date();
  }

  isExpiringSoon(expirationDate?: string, days: number = 7): boolean {
    if (!expirationDate) return false;
    const expDate = new Date(expirationDate);
    const today = new Date();
    const diffTime = expDate.getTime() - today.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays >= 0 && diffDays <= days;
  }

  getExpirationClass(expirationDate?: string): string {
    if (!expirationDate) return '';
    if (this.isExpired(expirationDate)) return 'text-red-600 dark:text-red-400 font-semibold';
    if (this.isExpiringSoon(expirationDate)) return 'text-yellow-600 dark:text-yellow-400 font-semibold';
    return 'text-gray-700 dark:text-gray-300';
  }
}


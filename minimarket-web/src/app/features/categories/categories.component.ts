import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CategoriesService, CategoryDto } from '../../core/services/categories.service';
import { ToastService } from '../../shared/services/toast.service';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ConfirmDialogComponent],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.css'
})
export class CategoriesComponent implements OnInit {
  categories = signal<CategoryDto[]>([]);
  filteredCategories = signal<CategoryDto[]>([]);
  isLoading = signal(false);
  searchTerm = signal('');
  showConfirmDialog = signal(false);
  categoryToDelete = signal<string | null>(null);
  
  // Paginación
  currentPage = signal(1);
  pageSize = 20;
  totalCategories = signal(0);
  totalPages = computed(() => {
    return Math.ceil(this.totalCategories() / this.pageSize);
  });
  
  Math = Math;

  constructor(
    private categoriesService: CategoriesService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading.set(true);
    
    const params: any = {
      page: this.currentPage(),
      pageSize: this.pageSize
    };
    
    if (this.searchTerm().trim()) {
      params.searchTerm = this.searchTerm().trim();
    }
    
    this.categoriesService.getAll(params).subscribe({
      next: (pagedResult) => {
        this.categories.set(pagedResult.items || []);
        this.filteredCategories.set(pagedResult.items || []);
        this.totalCategories.set(pagedResult.totalCount || 0);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.toastService.error('Error al cargar las categorías');
        this.categories.set([]);
        this.filteredCategories.set([]);
        this.totalCategories.set(0);
        this.isLoading.set(false);
      }
    });
  }

  onSearch(): void {
    this.currentPage.set(1);
    this.loadCategories();
  }
  
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadCategories();
    }
  }
  
  getRangeStart(): number {
    return ((this.currentPage() - 1) * this.pageSize) + 1;
  }
  
  getRangeEnd(): number {
    return Math.min(this.currentPage() * this.pageSize, this.totalCategories());
  }

  confirmDelete(categoryId: string): void {
    this.categoryToDelete.set(categoryId);
    this.showConfirmDialog.set(true);
  }

  onDeleteConfirmed(): void {
    const categoryId = this.categoryToDelete();
    if (!categoryId) return;

    this.categoriesService.delete(categoryId).subscribe({
      next: () => {
        this.toastService.success('Categoría eliminada exitosamente');
        this.loadCategories();
        this.showConfirmDialog.set(false);
        this.categoryToDelete.set(null);
      },
      error: (error) => {
        console.error('Error deleting category:', error);
        this.toastService.error(error.error?.errors?.[0] || 'Error al eliminar la categoría');
        this.showConfirmDialog.set(false);
        this.categoryToDelete.set(null);
      }
    });
  }

  onDeleteCanceled(): void {
    this.showConfirmDialog.set(false);
    this.categoryToDelete.set(null);
  }

  viewCategoryDetail(categoryId: string): void {
    this.router.navigate(['/admin/categorias', categoryId]);
  }
}


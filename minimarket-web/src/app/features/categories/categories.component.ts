import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
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

  constructor(
    private categoriesService: CategoriesService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading.set(true);
    this.categoriesService.getAll().subscribe({
      next: (categories) => {
        this.categories.set(categories);
        this.filteredCategories.set(categories);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.toastService.error('Error al cargar las categorías');
        this.isLoading.set(false);
      }
    });
  }

  onSearch(): void {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) {
      this.filteredCategories.set(this.categories());
      return;
    }

    const filtered = this.categories().filter(c =>
      c.name.toLowerCase().includes(term) ||
      (c.description && c.description.toLowerCase().includes(term))
    );
    this.filteredCategories.set(filtered);
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
}


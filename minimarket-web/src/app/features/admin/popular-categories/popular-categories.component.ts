import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoriesService, CategoryDto, UpdateCategoryDto } from '../../../core/services/categories.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-popular-categories',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './popular-categories.component.html',
  styleUrl: './popular-categories.component.css'
})
export class PopularCategoriesComponent implements OnInit {
  allCategories = signal<CategoryDto[]>([]);
  popularCategories = signal<CategoryDto[]>([]);
  isLoading = signal(false);
  searchTerm = signal('');

  constructor(
    private categoriesService: CategoriesService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading.set(true);
    this.categoriesService.getAllWithoutPagination().subscribe({
      next: (categories) => {
        const activeCategories = categories.filter((cat: any) => cat.isActive);
        this.allCategories.set(activeCategories);
        
        // Separar categorías populares (con orden > 0)
        const popular = activeCategories
          .filter((cat: any) => cat.orden !== undefined && cat.orden > 0)
          .sort((a: any, b: any) => (a.orden || 999) - (b.orden || 999));
        this.popularCategories.set(popular);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.toastService.error('Error al cargar las categorías');
        this.isLoading.set(false);
      }
    });
  }

  getAvailableCategories(): CategoryDto[] {
    const term = this.searchTerm().toLowerCase().trim();
    const popularIds = new Set(this.popularCategories().map(c => c.id));
    
    let available = this.allCategories().filter(c => !popularIds.has(c.id));
    
    if (term) {
      available = available.filter(c => 
        c.name.toLowerCase().includes(term) ||
        (c.description && c.description.toLowerCase().includes(term))
      );
    }
    
    return available;
  }

  addToPopular(category: CategoryDto): void {
    const popular = this.popularCategories();
    const maxOrden = popular.length > 0 
      ? Math.max(...popular.map(c => c.orden || 0))
      : 0;
    
    const updatedCategory: UpdateCategoryDto = {
      name: category.name,
      description: category.description,
      imageUrl: category.imageUrl,
      isActive: category.isActive,
      orden: maxOrden + 1
    };

    this.categoriesService.update(category.id, updatedCategory).subscribe({
      next: () => {
        this.toastService.success('Categoría agregada a populares');
        this.loadCategories();
      },
      error: (error) => {
        console.error('Error updating category:', error);
        this.toastService.error('Error al agregar la categoría');
      }
    });
  }

  removeFromPopular(category: CategoryDto): void {
    const updatedCategory: UpdateCategoryDto = {
      name: category.name,
      description: category.description,
      imageUrl: category.imageUrl,
      isActive: category.isActive,
      orden: 0 // Establecer orden en 0 para remover de populares
    };

    this.categoriesService.update(category.id, updatedCategory).subscribe({
      next: () => {
        this.toastService.success('Categoría removida de populares');
        this.loadCategories();
      },
      error: (error) => {
        console.error('Error updating category:', error);
        this.toastService.error('Error al remover la categoría');
      }
    });
  }

  moveUp(category: CategoryDto): void {
    const popular = [...this.popularCategories()];
    const index = popular.findIndex(c => c.id === category.id);
    
    if (index <= 0) return;

    const currentOrden = category.orden || index + 1;
    const prevOrden = popular[index - 1].orden || index;

    // Intercambiar órdenes
    this.updateCategoryOrder(category, prevOrden);
    this.updateCategoryOrder(popular[index - 1], currentOrden);
  }

  moveDown(category: CategoryDto): void {
    const popular = [...this.popularCategories()];
    const index = popular.findIndex(c => c.id === category.id);
    
    if (index >= popular.length - 1) return;

    const currentOrden = category.orden || index + 1;
    const nextOrden = popular[index + 1].orden || index + 2;

    // Intercambiar órdenes
    this.updateCategoryOrder(category, nextOrden);
    this.updateCategoryOrder(popular[index + 1], currentOrden);
  }

  private updateCategoryOrder(category: CategoryDto, orden: number): void {
    const updatedCategory: UpdateCategoryDto = {
      name: category.name,
      description: category.description,
      imageUrl: category.imageUrl,
      isActive: category.isActive,
      orden: orden
    };

    this.categoriesService.update(category.id, updatedCategory).subscribe({
      next: () => {
        this.loadCategories();
      },
      error: (error) => {
        console.error('Error updating category order:', error);
        this.toastService.error('Error al actualizar el orden');
      }
    });
  }
}


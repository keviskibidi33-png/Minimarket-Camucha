import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SettingsService, SystemSettings, UpdateSystemSettings } from '../../../core/services/settings.service';
import { CategoriesService } from '../../../core/services/categories.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent implements OnInit {
  settings = signal<SystemSettings[]>([]);
  categories = signal<any[]>([]);
  isLoading = signal(false);
  activeTab = signal<'cart' | 'shipping' | 'banners' | 'categories'>('cart');
  
  // Configuraciones del carrito
  applyIgvToCart = signal(false);
  
  // Configuraciones de categorías
  selectedCategory = signal<string | null>(null);
  categoryImageUrl = signal('');
  
  // Computed para obtener el nombre de la categoría seleccionada
  selectedCategoryName = computed(() => {
    const categoryId = this.selectedCategory();
    if (!categoryId) return '';
    const category = this.categories().find(c => c.id === categoryId);
    return category?.name || '';
  });

  constructor(
    private settingsService: SettingsService,
    private categoriesService: CategoriesService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadSettings();
    this.loadCategories();
  }

  loadSettings(): void {
    this.isLoading.set(true);
    this.settingsService.getAll().subscribe({
      next: (settings) => {
        this.settings.set(settings);
        // Cargar configuración de IGV
        const igvSetting = settings.find(s => s.key === 'apply_igv_to_cart');
        if (igvSetting) {
          this.applyIgvToCart.set(igvSetting.value === 'true' || igvSetting.value === '1');
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading settings:', error);
        this.toastService.error('Error al cargar configuraciones');
        this.isLoading.set(false);
      }
    });
  }

  loadCategories(): void {
    this.categoriesService.getAll().subscribe({
      next: (categories) => {
        this.categories.set(categories);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  saveCartSetting(): void {
    const setting: UpdateSystemSettings = {
      key: 'apply_igv_to_cart',
      value: this.applyIgvToCart() ? 'true' : 'false',
      description: 'Aplicar IGV al carrito de compras',
      isActive: true
    };

    this.settingsService.update('apply_igv_to_cart', setting).subscribe({
      next: () => {
        this.toastService.success('Configuración guardada correctamente');
        this.loadSettings();
      },
      error: (error) => {
        console.error('Error saving setting:', error);
        this.toastService.error('Error al guardar configuración');
      }
    });
  }

  onCategorySelected(categoryId: string): void {
    this.selectedCategory.set(categoryId);
    const category = this.categories().find(c => c.id === categoryId);
    if (category) {
      this.categoryImageUrl.set(category.imageUrl || '');
    }
  }

  saveCategoryImage(): void {
    const categoryId = this.selectedCategory();
    if (!categoryId) {
      this.toastService.error('Selecciona una categoría');
      return;
    }

    const category = this.categories().find(c => c.id === categoryId);
    if (!category) {
      return;
    }

    const updateDto = {
      name: category.name,
      description: category.description,
      imageUrl: this.categoryImageUrl(),
      isActive: category.isActive
    };

    this.categoriesService.update(categoryId, updateDto).subscribe({
      next: () => {
        this.toastService.success('Imagen de categoría guardada correctamente');
        this.loadCategories();
      },
      error: (error) => {
        console.error('Error saving category image:', error);
        this.toastService.error('Error al guardar imagen de categoría');
      }
    });
  }

  setTab(tab: 'cart' | 'shipping' | 'banners' | 'categories'): void {
    this.activeTab.set(tab);
  }
}


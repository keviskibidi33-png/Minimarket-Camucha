import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProductsService, Product } from '../../../core/services/products.service';
import { CategoriesService, CategoryDto } from '../../../core/services/categories.service';
import { StoreHeaderComponent } from '../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../shared/components/store-footer/store-footer.component';
import { ProductCardComponent } from '../../../shared/components/product-card/product-card.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    StoreHeaderComponent, 
    StoreFooterComponent, 
    ProductCardComponent
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  featuredProducts = signal<Product[]>([]);
  categories = signal<CategoryDto[]>([]);
  isLoading = signal(true);

  constructor(
    private productsService: ProductsService,
    private categoriesService: CategoriesService
  ) {}

  ngOnInit() {
    this.loadFeaturedProducts();
    this.loadCategories();
  }

  loadFeaturedProducts() {
    this.isLoading.set(true);
    this.productsService.getAll({ 
      isActive: true, 
      pageSize: 5 
    }).subscribe({
      next: (result) => {
        const products = result.items || (Array.isArray(result) ? result : []);
        this.featuredProducts.set(products);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading featured products:', error);
        this.isLoading.set(false);
      }
    });
  }

  loadCategories() {
    this.categoriesService.getAll().subscribe({
      next: (categories) => {
        // Filtrar solo categorías activas
        const activeCategories = categories.filter(cat => cat.isActive);
        this.categories.set(activeCategories);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  getCategoryIcon(categoryName: string): string {
    const name = categoryName.toLowerCase();
    if (name.includes('bebida') || name.includes('drink') || name.includes('licor')) {
      return 'local_bar';
    } else if (name.includes('snack') || name.includes('dulce')) {
      return 'icecream';
    } else if (name.includes('grocer') || name.includes('abarrote') || name.includes('comida')) {
      return 'local_mall';
    } else if (name.includes('limpieza') || name.includes('household') || name.includes('hogar')) {
      return 'cleaning_services';
    }
    return 'shopping_bag';
  }
}


import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { ProductsService, CreateProductDto, UpdateProductDto } from '../../../core/services/products.service';
import { CategoriesService } from '../../../core/services/categories.service';
import { ImageUploadComponent } from '../../../shared/components/image-upload/image-upload.component';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ImageUploadComponent],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.css'
})
export class ProductFormComponent implements OnInit {
  productForm: FormGroup;
  categories = signal<any[]>([]);
  isLoading = signal(false);
  isEditMode = signal(false);
  productId = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  constructor(
    private fb: FormBuilder,
    private productsService: ProductsService,
    private categoriesService: CategoriesService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.productForm = this.fb.group({
      code: ['', [Validators.required, Validators.maxLength(50)]],
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(1000)]],
      purchasePrice: [0, [Validators.required, Validators.min(0.01)]],
      salePrice: [0, [Validators.required, Validators.min(0.01)]],
      stock: [0, [Validators.required, Validators.min(0)]],
      minimumStock: [0, [Validators.required, Validators.min(0)]],
      categoryId: ['', [Validators.required]],
      imageUrl: ['', [Validators.maxLength(500)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadCategories();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.productId.set(id);
      this.loadProduct(id);
    }
  }

  loadCategories(): void {
    this.categoriesService.getAll().subscribe({
      next: (categories) => this.categories.set(categories),
      error: (error) => console.error('Error loading categories:', error)
    });
  }

  loadProduct(id: string): void {
    this.isLoading.set(true);
    this.productsService.getById(id).subscribe({
      next: (product) => {
        this.productForm.patchValue({
          code: product.code,
          name: product.name,
          description: product.description,
          purchasePrice: product.purchasePrice,
          salePrice: product.salePrice,
          stock: product.stock,
          minimumStock: product.minimumStock,
          categoryId: product.categoryId,
          imageUrl: product.imageUrl || '',
          isActive: product.isActive
        });
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading product:', error);
        this.errorMessage.set('Error al cargar el producto');
        this.isLoading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const formValue = this.productForm.value;

    if (this.isEditMode()) {
      const updateDto: UpdateProductDto = {
        id: this.productId()!,
        ...formValue
      };

      this.productsService.update(updateDto).subscribe({
        next: () => {
          this.router.navigate(['/productos']);
        },
        error: (error) => {
          this.isLoading.set(false);
          this.errorMessage.set(error.error?.errors?.[0] || 'Error al actualizar el producto');
        }
      });
    } else {
      const createDto: CreateProductDto = {
        ...formValue
      };

      this.productsService.create(createDto).subscribe({
        next: () => {
          this.router.navigate(['/productos']);
        },
        error: (error) => {
          this.isLoading.set(false);
          this.errorMessage.set(error.error?.errors?.[0] || 'Error al crear el producto');
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/productos']);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.productForm.controls).forEach(key => {
      const control = this.productForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const control = this.productForm.get(fieldName);
    if (control?.errors && control.touched) {
      if (control.errors['required']) {
        return `${this.getFieldLabel(fieldName)} es requerido`;
      }
      if (control.errors['maxlength']) {
        return `Máximo ${control.errors['maxlength'].requiredLength} caracteres`;
      }
      if (control.errors['min']) {
        return `Debe ser mayor a ${control.errors['min'].min}`;
      }
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      code: 'Código',
      name: 'Nombre',
      description: 'Descripción',
      purchasePrice: 'Precio de compra',
      salePrice: 'Precio de venta',
      stock: 'Stock',
      minimumStock: 'Stock mínimo',
      categoryId: 'Categoría'
    };
    return labels[fieldName] || fieldName;
  }
}


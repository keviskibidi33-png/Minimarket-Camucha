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
  categories = signal<{id: string; name: string; productCount?: number}[]>([]);
  isLoading = signal(false);
  isEditMode = signal(false);
  productId = signal<string | null>(null);
  errorMessage = signal<string | null>(null);
  showCreateCategoryModal = signal(false);
  newCategoryName = signal('');
  newCategoryDescription = signal('');
  isCreatingCategory = signal(false);

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
      expirationDate: [null], // Fecha de vencimiento (opcional)
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
    this.categoriesService.getAllWithoutPagination().subscribe({
      next: (categories) => {
        // Mapear categorías asegurando que productCount esté presente
        const categoriesWithCount = categories.map((c: any) => ({
          id: c.id,
          name: c.name,
          productCount: c.productCount !== undefined ? c.productCount : 0
        }));
        this.categories.set(categoriesWithCount);
        // Debug: verificar que productCount esté llegando
        console.log('Categorías cargadas con conteo:', categoriesWithCount);
        if (categoriesWithCount.length > 0) {
          console.log('Primera categoría ejemplo:', categoriesWithCount[0]);
          console.log('ProductCount de primera categoría:', categoriesWithCount[0].productCount);
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        // En caso de error, mantener las categorías existentes o cargar vacío
        if (this.categories().length === 0) {
          this.categories.set([]);
        }
      }
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
          expirationDate: product.expirationDate ? new Date(product.expirationDate).toISOString().split('T')[0] : null,
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

    // Validar que la categoría existe
    const categoryId = this.productForm.get('categoryId')?.value;
    const categoryExists = this.categories().some(c => c.id === categoryId);
    if (!categoryExists) {
      this.errorMessage.set('La categoría seleccionada no existe');
      return;
    }

    // Validar que la imagen URL no sea un data URL (base64)
    const imageUrl = this.productForm.get('imageUrl')?.value;
    if (imageUrl && imageUrl.startsWith('data:')) {
      this.errorMessage.set('Por favor, espera a que la imagen se suba completamente antes de guardar');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const formValue = this.productForm.value;
    
    // Asegurarse de que imageUrl no sea un data URL
    if (formValue.imageUrl && formValue.imageUrl.startsWith('data:')) {
      formValue.imageUrl = '';
    }
    
    // Formatear expirationDate si existe
    if (formValue.expirationDate) {
      formValue.expirationDate = new Date(formValue.expirationDate).toISOString();
    } else {
      formValue.expirationDate = null;
    }

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
          console.error('Error updating product:', error);
          
          // Manejar diferentes tipos de errores
          let errorMsg = 'Error al actualizar el producto';
          if (error.error) {
            if (error.error.errors && Array.isArray(error.error.errors) && error.error.errors.length > 0) {
              errorMsg = error.error.errors[0].message || error.error.errors[0];
            } else if (error.error.message) {
              errorMsg = error.error.message;
            } else if (error.error.error) {
              errorMsg = error.error.error;
            }
          } else if (error.message) {
            errorMsg = error.message;
          }
          
          this.errorMessage.set(errorMsg);
        }
      });
    } else {
      // Crear payload limpio solo con los campos del CreateProductDto
      // No incluir campos como isActive, id, createdAt, etc.
      const createDto: CreateProductDto = {
        code: String(formValue.code || '').trim(),
        name: String(formValue.name || '').trim(),
        description: String(formValue.description || '').trim(),
        purchasePrice: Number(formValue.purchasePrice) || 0,
        salePrice: Number(formValue.salePrice) || 0,
        stock: Number(formValue.stock) || 0,
        minimumStock: Number(formValue.minimumStock) || 0,
        categoryId: String(formValue.categoryId || '').trim(),
        imageUrl: formValue.imageUrl && !formValue.imageUrl.startsWith('data:') 
          ? String(formValue.imageUrl).trim() 
          : undefined,
        expirationDate: formValue.expirationDate 
          ? new Date(formValue.expirationDate).toISOString() 
          : undefined
      };

      // Validar que los campos requeridos estén presentes
      if (!createDto.code || !createDto.name || !createDto.categoryId) {
        this.errorMessage.set('Por favor complete todos los campos requeridos');
        this.isLoading.set(false);
        return;
      }

      // Validar que los precios sean válidos
      if (createDto.purchasePrice <= 0 || createDto.salePrice <= 0) {
        this.errorMessage.set('Los precios deben ser mayores a 0');
        this.isLoading.set(false);
        return;
      }

      // Validar que el precio de venta sea mayor al de compra
      if (createDto.salePrice <= createDto.purchasePrice) {
        this.errorMessage.set('El precio de venta debe ser mayor al precio de compra');
        this.isLoading.set(false);
        return;
      }

      this.productsService.create(createDto).subscribe({
        next: () => {
          this.router.navigate(['/productos']);
        },
        error: (error) => {
          this.isLoading.set(false);
          console.error('Error creating product:', error);
          
          // Manejar diferentes tipos de errores
          let errorMsg = 'Error al crear el producto';
          if (error.error) {
            if (error.error.errors && Array.isArray(error.error.errors) && error.error.errors.length > 0) {
              errorMsg = error.error.errors[0].message || error.error.errors[0];
            } else if (error.error.message) {
              errorMsg = error.error.message;
            } else if (error.error.error) {
              errorMsg = error.error.error;
            }
          } else if (error.message) {
            errorMsg = error.message;
          }
          
          this.errorMessage.set(errorMsg);
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

  openCreateCategoryModal(): void {
    this.showCreateCategoryModal.set(true);
    this.newCategoryName.set('');
    this.newCategoryDescription.set('');
  }

  closeCreateCategoryModal(): void {
    this.showCreateCategoryModal.set(false);
    this.newCategoryName.set('');
    this.newCategoryDescription.set('');
  }

  createCategory(): void {
    const name = this.newCategoryName().trim();
    if (!name) {
      this.errorMessage.set('El nombre de la categoría es requerido');
      return;
    }

    this.isCreatingCategory.set(true);
    this.errorMessage.set(null);

    this.categoriesService.create({
      name: name,
      description: this.newCategoryDescription().trim() || undefined
    }).subscribe({
      next: (newCategory) => {
        // Recargar categorías desde el backend (incluye conteo actualizado)
        this.loadCategories();
        // Seleccionar la nueva categoría automáticamente
        this.productForm.patchValue({ categoryId: newCategory.id });
        // Cerrar modal
        this.closeCreateCategoryModal();
        this.isCreatingCategory.set(false);
        // Nota: La categoría se reflejará en otros módulos cuando recarguen las categorías
        // Todos los módulos usan el mismo servicio CategoriesService que consulta el backend
      },
      error: (error) => {
        this.isCreatingCategory.set(false);
        this.errorMessage.set(error.error?.errors?.[0] || 'Error al crear la categoría');
      }
    });
  }
}


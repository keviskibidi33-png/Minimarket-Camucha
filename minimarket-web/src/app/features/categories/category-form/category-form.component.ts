import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CategoriesService, CreateCategoryDto, UpdateCategoryDto } from '../../../core/services/categories.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './category-form.component.html',
  styleUrl: './category-form.component.css'
})
export class CategoryFormComponent implements OnInit {
  categoryForm: FormGroup;
  isEditMode = signal(false);
  isLoading = signal(false);
  categoryId = signal<string | null>(null);

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private categoriesService: CategoriesService,
    private toastService: ToastService
  ) {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'nuevo') {
      this.isEditMode.set(true);
      this.categoryId.set(id);
      this.loadCategory(id);
    }
  }

  loadCategory(id: string): void {
    this.isLoading.set(true);
    this.categoriesService.getById(id).subscribe({
      next: (category) => {
        this.categoryForm.patchValue({
          name: category.name,
          description: category.description,
          isActive: category.isActive
        });
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading category:', error);
        this.toastService.error('Error al cargar la categoría');
        this.isLoading.set(false);
        this.router.navigate(['/categorias']);
      }
    });
  }

  onSubmit(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);

    if (this.isEditMode()) {
      const updateDto: UpdateCategoryDto = {
        name: this.categoryForm.get('name')?.value,
        description: this.categoryForm.get('description')?.value || '',
        isActive: this.categoryForm.get('isActive')?.value
      };

      this.categoriesService.update(this.categoryId()!, updateDto).subscribe({
        next: () => {
          this.toastService.success('Categoría actualizada exitosamente');
          this.router.navigate(['/categorias']);
        },
        error: (error) => {
          console.error('Error updating category:', error);
          this.toastService.error(error.error?.errors?.[0] || 'Error al actualizar la categoría');
          this.isLoading.set(false);
        }
      });
    } else {
      const createDto: CreateCategoryDto = {
        name: this.categoryForm.get('name')?.value,
        description: this.categoryForm.get('description')?.value || ''
      };

      this.categoriesService.create(createDto).subscribe({
        next: () => {
          this.toastService.success('Categoría creada exitosamente');
          this.router.navigate(['/categorias']);
        },
        error: (error) => {
          console.error('Error creating category:', error);
          this.toastService.error(error.error?.errors?.[0] || 'Error al crear la categoría');
          this.isLoading.set(false);
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/categorias']);
  }
}


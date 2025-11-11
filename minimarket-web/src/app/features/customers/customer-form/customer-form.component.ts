import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CustomersService, CreateCustomerDto, UpdateCustomerDto } from '../../../core/services/customers.service';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.css'
})
export class CustomerFormComponent implements OnInit {
  customerForm: FormGroup;
  isLoading = signal(false);
  isEditMode = signal(false);
  customerId = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  documentTypes = [
    { value: 'DNI', label: 'DNI' },
    { value: 'RUC', label: 'RUC' }
  ];

  constructor(
    private fb: FormBuilder,
    private customersService: CustomersService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.customerForm = this.fb.group({
      documentType: ['DNI', [Validators.required]],
      documentNumber: ['', [Validators.required, Validators.pattern(/^\d+$/)]],
      name: ['', [Validators.required, Validators.maxLength(200)]],
      email: ['', [Validators.email, Validators.maxLength(100)]],
      phone: ['+51 ', [Validators.maxLength(20)]],
      address: ['', [Validators.maxLength(500)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.customerId.set(id);
      this.loadCustomer(id);
    }

    // Validación dinámica de longitud de documento según tipo
    this.customerForm.get('documentType')?.valueChanges.subscribe(type => {
      const docControl = this.customerForm.get('documentNumber');
      if (type === 'DNI') {
        docControl?.setValidators([Validators.required, Validators.pattern(/^\d{8}$/)]);
      } else if (type === 'RUC') {
        docControl?.setValidators([Validators.required, Validators.pattern(/^\d{11}$/)]);
      }
      docControl?.updateValueAndValidity();
    });
  }

  loadCustomer(id: string): void {
    this.isLoading.set(true);
    this.customersService.getById(id).subscribe({
      next: (customer) => {
        this.customerForm.patchValue({
          documentType: customer.documentType,
          documentNumber: customer.documentNumber,
          name: customer.name,
          email: customer.email || '',
          phone: customer.phone || '',
          address: customer.address || '',
          isActive: customer.isActive
        });
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading customer:', error);
        this.errorMessage.set('Error al cargar el cliente');
        this.isLoading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.customerForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const formValue = this.customerForm.value;

    if (this.isEditMode()) {
      const updateDto: UpdateCustomerDto = {
        id: this.customerId()!,
        ...formValue
      };

      this.customersService.update(updateDto).subscribe({
        next: () => {
          this.router.navigate(['/clientes']);
        },
        error: (error) => {
          this.isLoading.set(false);
          this.errorMessage.set(error.error?.errors?.[0] || 'Error al actualizar el cliente');
        }
      });
    } else {
      const createDto: CreateCustomerDto = {
        ...formValue
      };

      this.customersService.create(createDto).subscribe({
        next: () => {
          this.router.navigate(['/clientes']);
        },
        error: (error) => {
          this.isLoading.set(false);
          this.errorMessage.set(error.error?.errors?.[0] || 'Error al crear el cliente');
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/clientes']);
  }

  private markFormGroupTouched(): void {
    Object.keys(this.customerForm.controls).forEach(key => {
      const control = this.customerForm.get(key);
      control?.markAsTouched();
    });
  }

  getFieldError(fieldName: string): string {
    const control = this.customerForm.get(fieldName);
    if (control?.errors && control.touched) {
      if (control.errors['required']) {
        return `${this.getFieldLabel(fieldName)} es requerido`;
      }
      if (control.errors['email']) {
        return 'El email no es válido';
      }
      if (control.errors['pattern']) {
        if (fieldName === 'documentNumber') {
          const docType = this.customerForm.get('documentType')?.value;
          return docType === 'DNI' ? 'El DNI debe tener 8 dígitos' : 'El RUC debe tener 11 dígitos';
        }
      }
      if (control.errors['maxlength']) {
        return `Máximo ${control.errors['maxlength'].requiredLength} caracteres`;
      }
    }
    return '';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      documentType: 'Tipo de documento',
      documentNumber: 'Número de documento',
      name: 'Nombre',
      email: 'Email',
      phone: 'Teléfono',
      address: 'Dirección'
    };
    return labels[fieldName] || fieldName;
  }
}


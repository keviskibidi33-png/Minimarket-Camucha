import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CustomersService, Customer } from '../../core/services/customers.service';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.css'
})
export class CustomersComponent implements OnInit {
  customers = signal<Customer[]>([]);
  isLoading = signal(false);
  searchTerm = signal('');
  selectedDocumentType = signal<string>('');
  currentPage = signal(1);
  pageSize = 10;
  totalCustomers = signal(0);

  documentTypes = [
    { value: '', label: 'Todos' },
    { value: 'DNI', label: 'DNI' },
    { value: 'RUC', label: 'RUC' }
  ];

  constructor(private customersService: CustomersService) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.isLoading.set(true);
    this.customersService.getAll({
      page: this.currentPage(),
      pageSize: this.pageSize,
      searchTerm: this.searchTerm() || undefined,
      documentType: this.selectedDocumentType() || undefined
    }).subscribe({
      next: (pagedResult) => {
        this.customers.set(pagedResult.items);
        this.totalCustomers.set(pagedResult.totalCount);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading customers:', error);
        this.isLoading.set(false);
        // El error se maneja con toast desde el interceptor si existe
        // Aquí podríamos agregar un signal de error si es necesario
      }
    });
  }

  onSearch(): void {
    this.currentPage.set(1);
    this.loadCustomers();
  }

  onDocumentTypeChange(): void {
    this.currentPage.set(1);
    this.loadCustomers();
  }

  deleteCustomer(id: string): void {
    if (confirm('¿Está seguro de eliminar este cliente? Esta acción no se puede deshacer.')) {
      this.isLoading.set(true);
      this.customersService.delete(id).subscribe({
        next: () => {
          this.isLoading.set(false);
          this.loadCustomers();
        },
        error: (error) => {
          console.error('Error deleting customer:', error);
          this.isLoading.set(false);
          const errorMessage = error.error?.errors?.[0] || error.error?.message || 'Error al eliminar el cliente. Por favor, intente nuevamente.';
          alert(errorMessage);
        }
      });
    }
  }

  formatDocument(documentType: string, documentNumber: string): string {
    return `${documentType}: ${documentNumber}`;
  }

  Math = Math;
}


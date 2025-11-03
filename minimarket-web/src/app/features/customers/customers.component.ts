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
      next: (customers) => {
        this.customers.set(customers);
        this.totalCustomers.set(customers.length);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading customers:', error);
        this.isLoading.set(false);
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
    if (confirm('¿Está seguro de eliminar este cliente?')) {
      this.customersService.delete(id).subscribe({
        next: () => {
          this.loadCustomers();
        },
        error: (error) => {
          console.error('Error deleting customer:', error);
          alert('Error al eliminar el cliente');
        }
      });
    }
  }

  formatDocument(documentType: string, documentNumber: string): string {
    return `${documentType}: ${documentNumber}`;
  }
}


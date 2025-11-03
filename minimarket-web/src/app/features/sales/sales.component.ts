import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SalesService, Sale } from '../../core/services/sales.service';
import { ToastService } from '../../shared/services/toast.service';

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './sales.component.html',
  styleUrl: './sales.component.css'
})
export class SalesComponent implements OnInit {
  sales = signal<Sale[]>([]);
  isLoading = signal(false);
  searchTerm = signal('');
  startDate = signal<string>('');
  endDate = signal<string>('');
  currentPage = signal(1);
  pageSize = 10;
  totalSales = signal(0);

  constructor(
    private salesService: SalesService,
    private toastService: ToastService
  ) {
    // Establecer fechas por defecto (últimos 30 días)
    const today = new Date();
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(today.getDate() - 30);
    
    this.endDate.set(today.toISOString().split('T')[0]);
    this.startDate.set(thirtyDaysAgo.toISOString().split('T')[0]);
  }

  ngOnInit(): void {
    this.loadSales();
  }

  loadSales(): void {
    this.isLoading.set(true);
    
    const params: any = {
      page: this.currentPage(),
      pageSize: this.pageSize
    };

    if (this.searchTerm()) {
      params.documentNumber = this.searchTerm();
    }

    if (this.startDate()) {
      params.startDate = new Date(this.startDate()).toISOString();
    }

    if (this.endDate()) {
      params.endDate = new Date(this.endDate()).toISOString();
    }

    this.salesService.getAll(params).subscribe({
      next: (sales) => {
        this.sales.set(sales);
        this.totalSales.set(sales.length);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading sales:', error);
        this.toastService.error('Error al cargar las ventas');
        this.isLoading.set(false);
      }
    });
  }

  onSearch(): void {
    this.currentPage.set(1);
    this.loadSales();
  }

  onDateChange(): void {
    this.currentPage.set(1);
    this.loadSales();
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pagado':
        return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300';
      case 'Anulado':
        return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300';
      case 'Pendiente':
        return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300';
    }
  }

  getPaymentMethodText(method: string): string {
    const methods: { [key: string]: string } = {
      'Efectivo': 'Efectivo',
      'Tarjeta': 'Tarjeta',
      'YapePlin': 'Yape/Plin',
      'Transferencia': 'Transferencia'
    };
    return methods[method] || method;
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('es-PE', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}


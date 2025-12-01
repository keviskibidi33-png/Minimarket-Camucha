import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SalesService, Sale } from '../../core/services/sales.service';
import { CashClosureService, CashClosureSummary, CashClosureHistory } from '../../core/services/cash-closure.service';
import { ToastService } from '../../shared/services/toast.service';
import { SendReceiptDialogComponent } from '../../shared/components/send-receipt-dialog/send-receipt-dialog.component';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SendReceiptDialogComponent, BaseChartDirective],
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
  
  // Modal de reenvío
  showSendReceiptDialog = signal(false);
  showConfirmSendEmailDialog = signal(false);
  emailToConfirm = signal<string | null>(null);
  selectedSaleForResend = signal<Sale | null>(null);
  isResendingEmail = signal(false);
  
  // Cierre de caja
  cashClosureData = signal<{
    totalPaid: number;
    totalCount: number;
    byPaymentMethod: Array<{ method: string; total: number; count: number }>;
  } | null>(null);
  isLoadingCashClosure = signal(false);
  isGeneratingCashClosurePdf = signal(false);
  
  // Historial de cierres
  cashClosureHistory = signal<Array<{
    closureDate: string;
    salesStartDate: string;
    salesEndDate: string;
    totalSales: number;
    totalAmount: number;
    byPaymentMethod: Array<{ method: string; total: number; count: number }>;
  }>>([]);
  isLoadingHistory = signal(false);
  showHistory = signal(false);

  constructor(
    private salesService: SalesService,
    private cashClosureService: CashClosureService,
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
    this.loadCashClosure();
  }

  loadSales(): void {
    this.isLoading.set(true);
    
    const params: any = {
      page: this.currentPage(),
      pageSize: this.pageSize
    };

    // El backend espera DocumentNumber (con mayúscula)
    if (this.searchTerm() && this.searchTerm().trim()) {
      params.documentNumber = this.searchTerm().trim();
    }

    if (this.startDate()) {
      // El backend espera DateTime, enviar como ISO string
      params.startDate = new Date(this.startDate()).toISOString();
    }

    if (this.endDate()) {
      // El backend espera DateTime, enviar como ISO string
      // Agregar un día completo para incluir todo el día final
      const endDate = new Date(this.endDate());
      endDate.setHours(23, 59, 59, 999);
      params.endDate = endDate.toISOString();
    }

    this.salesService.getAll(params).subscribe({
      next: (pagedResult) => {
        this.sales.set(pagedResult.items || []);
        this.totalSales.set(pagedResult.totalCount || 0);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading sales:', error);
        this.toastService.error('Error al cargar las ventas');
        this.sales.set([]);
        this.totalSales.set(0);
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
    this.loadCashClosure();
  }
  
  loadCashClosure(): void {
    this.isLoadingCashClosure.set(true);
    
    // Obtener ventas pagadas del rango seleccionado (solo las no cerradas)
    const startDate = this.startDate() 
      ? new Date(this.startDate())
      : new Date();
    startDate.setHours(0, 0, 0, 0);
    
    const endDate = this.endDate()
      ? new Date(this.endDate())
      : new Date();
    endDate.setHours(23, 59, 59, 999);
    
    this.cashClosureService.getSummary(startDate, endDate).subscribe({
      next: (summary: CashClosureSummary) => {
        this.cashClosureData.set({
          totalPaid: summary.totalPaid,
          totalCount: summary.totalCount,
          byPaymentMethod: summary.byPaymentMethod.map((p: { method: string; total: number; count: number }) => ({
            method: p.method,
            total: p.total,
            count: p.count
          }))
        });
        
        this.isLoadingCashClosure.set(false);
      },
      error: (error) => {
        console.error('Error loading cash closure:', error);
        this.cashClosureData.set(null);
        this.isLoadingCashClosure.set(false);
        this.toastService.error('Error al cargar el resumen de cierre de caja');
      }
    });
  }
  
  generateCashClosurePdf(): void {
    if (!this.cashClosureData() || this.cashClosureData()!.totalCount === 0) {
      this.toastService.warning('No hay ventas para generar el cierre de caja');
      return;
    }

    this.isGeneratingCashClosurePdf.set(true);

    const startDate = this.startDate() 
      ? new Date(this.startDate())
      : new Date();
    startDate.setHours(0, 0, 0, 0);
    
    const endDate = this.endDate()
      ? new Date(this.endDate())
      : new Date();
    endDate.setHours(23, 59, 59, 999);

    this.cashClosureService.generatePdf({
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString()
    }).subscribe({
      next: (blob) => {
        // Crear URL del blob y descargar
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Cierre_Caja_${startDate.toISOString().split('T')[0]}_${endDate.toISOString().split('T')[0]}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        this.toastService.success('PDF de cierre de caja generado y descargado exitosamente. Las ventas han sido marcadas como cerradas.');
        this.isGeneratingCashClosurePdf.set(false);
        
        // Recargar datos para actualizar (las ventas ahora estarán marcadas como cerradas)
        this.loadCashClosure();
        this.loadSales(); // También recargar la tabla de ventas
        if (this.showHistory()) {
          this.loadCashClosureHistory(); // Actualizar historial si está visible
        }
      },
      error: (error) => {
        console.error('Error generando PDF de cierre de caja:', error);
        this.toastService.error('Error al generar el PDF de cierre de caja');
        this.isGeneratingCashClosurePdf.set(false);
      }
    });
  }
  
  // Gráfico de dona para métodos de pago
  paymentMethodChartData = computed<ChartConfiguration<'doughnut'>['data']>(() => {
    const data = this.cashClosureData();
    if (!data || !data.byPaymentMethod || data.byPaymentMethod.length === 0) {
      return {
        labels: [],
        datasets: []
      };
    }
    
    return {
      labels: data.byPaymentMethod.map(p => this.getPaymentMethodText(p.method)),
      datasets: [{
        data: data.byPaymentMethod.map(p => p.total),
        backgroundColor: [
          'rgba(34, 197, 94, 0.8)',   // Efectivo - Verde
          'rgba(59, 130, 246, 0.8)',  // Tarjeta - Azul
          'rgba(168, 85, 247, 0.8)',  // YapePlin - Púrpura
          'rgba(245, 101, 101, 0.8)'  // Transferencia - Rojo
        ],
        borderColor: [
          'rgb(34, 197, 94)',
          'rgb(59, 130, 246)',
          'rgb(168, 85, 247)',
          'rgb(245, 101, 101)'
        ],
        borderWidth: 2
      }]
    };
  });
  
  paymentMethodChartOptions: ChartConfiguration<'doughnut'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    animation: {
      duration: 1000,
      easing: 'easeInOutQuart'
    },
    plugins: {
      legend: {
        position: 'bottom',
        labels: {
          padding: 15,
          font: {
            size: 12,
            weight: 'normal'
          },
          usePointStyle: true
        }
      },
      tooltip: {
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        padding: 12,
        titleFont: {
          size: 14,
          weight: 'bold'
        },
        bodyFont: {
          size: 13
        },
        borderColor: 'rgba(255, 255, 255, 0.1)',
        borderWidth: 1,
        cornerRadius: 8,
        callbacks: {
          label: (context) => {
            const label = context.label || '';
            const value = context.parsed;
            const total = context.dataset.data.reduce((a: number, b: number) => a + b, 0);
            const percentage = ((value / total) * 100).toFixed(1);
            return `${label}: S/ ${value.toFixed(2)} (${percentage}%)`;
          }
        }
      }
    }
  };
  
  paymentMethodChartType = 'doughnut' as const;

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

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-PE', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    });
  }
  
  formatTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleTimeString('es-PE', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }
  
  loadCashClosureHistory(): void {
    this.isLoadingHistory.set(true);
    
    const startDate = this.startDate() ? new Date(this.startDate()) : undefined;
    const endDate = this.endDate() ? new Date(this.endDate()) : undefined;
    
    this.cashClosureService.getHistory(startDate, endDate).subscribe({
      next: (history) => {
        this.cashClosureHistory.set(history);
        this.isLoadingHistory.set(false);
      },
      error: (error) => {
        console.error('Error loading cash closure history:', error);
        const errorMessage = error?.error?.message || error?.message || 'Error desconocido al cargar el historial';
        this.toastService.error(`Error al cargar el historial de cierres de caja: ${errorMessage}`);
        this.isLoadingHistory.set(false);
      }
    });
  }
  
  toggleHistory(): void {
    this.showHistory.set(!this.showHistory());
    if (this.showHistory() && this.cashClosureHistory().length === 0) {
      this.loadCashClosureHistory();
    }
  }

  downloadCashClosurePdf(closureDate: string): void {
    const date = new Date(closureDate);
    
    this.cashClosureService.downloadPdf(date).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Cierre_Caja_${date.toISOString().split('T')[0]}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);

        this.toastService.success('PDF de cierre de caja descargado exitosamente');
      },
      error: (error) => {
        console.error('Error descargando PDF de cierre de caja:', error);
        const errorMessage = error?.error?.message || error?.message || 'Error desconocido';
        this.toastService.error(`Error al descargar el PDF: ${errorMessage}`);
      }
    });
  }

  Math = Math;
  
  totalPages = computed(() => {
    return Math.ceil(this.totalSales() / this.pageSize);
  });
  
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadSales();
    }
  }
  
  resendReceipt(sale: Sale): void {
    if (!sale) return;
    
    // Validar que la venta no esté anulada
    if (sale.status === 'Anulado') {
      this.toastService.error('No se puede reenviar el comprobante de una venta anulada');
      return;
    }
    
    this.selectedSaleForResend.set(sale);
    const customerEmail = sale.customerEmail;
    
    // Si el cliente tiene email registrado, mostrar modal de confirmación simple
    if (customerEmail && customerEmail.trim() !== '') {
      this.emailToConfirm.set(customerEmail);
      this.showConfirmSendEmailDialog.set(true);
    } else {
      // Si no hay email, mostrar el modal para ingresar el correo
      this.showSendReceiptDialog.set(true);
    }
  }
  
  confirmSendEmail(): void {
    const sale = this.selectedSaleForResend();
    const email = this.emailToConfirm();
    
    if (!sale || !email) return;
    
    this.sendReceiptDirectly(sale, email);
  }
  
  sendReceiptDirectly(sale: Sale, email: string): void {
    if (!sale || !email || email.trim() === '') {
      this.toastService.error('El correo electrónico es requerido');
      return;
    }
    
    // Cerrar modales
    this.showConfirmSendEmailDialog.set(false);
    this.showSendReceiptDialog.set(false);
    this.emailToConfirm.set(null);
    
    // Mostrar indicador de carga
    this.isResendingEmail.set(true);
    
    this.salesService.sendReceipt(sale.id, email.trim(), sale.documentType).subscribe({
      next: () => {
        this.isResendingEmail.set(false);
        this.selectedSaleForResend.set(null);
        this.toastService.success(`Comprobante reenviado exitosamente a ${email}`);
        // Recargar la lista de ventas para actualizar cualquier cambio
        this.loadSales();
      },
      error: (error) => {
        console.error('Error sending receipt:', error);
        this.isResendingEmail.set(false);
        // Si falla, mostrar el modal para que el usuario pueda ingresar otro correo
        this.showConfirmSendEmailDialog.set(false);
        this.showSendReceiptDialog.set(true);
        this.toastService.error(error.error?.errors?.[0] || error.error?.message || 'Error al reenviar el comprobante. Puedes intentar con otro correo.');
      }
    });
  }
  
  cancelConfirmSendEmail(): void {
    this.showConfirmSendEmailDialog.set(false);
    this.emailToConfirm.set(null);
    this.selectedSaleForResend.set(null);
  }
  
  onSendReceipt(event: { email: string }): void {
    const sale = this.selectedSaleForResend();
    if (!sale) return;
    this.sendReceiptDirectly(sale, event.email);
  }
}


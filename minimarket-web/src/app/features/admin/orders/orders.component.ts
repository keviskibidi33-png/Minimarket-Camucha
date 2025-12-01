import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrdersService, WebOrder, PagedResult } from '../../../core/services/orders.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent implements OnInit {
  orders = signal<WebOrder[]>([]);
  isLoading = signal(false);
  
  // Filtros
  selectedStatus = signal<string>('');
  searchTerm = signal('');
  startDate = signal('');
  endDate = signal('');

  // Estadísticas rápidas
  pendingCount = computed(() => this.orders().filter(o => o.status === 'pending').length);
  pendingWithProof = computed(() => this.orders().filter(o => o.status === 'pending' && o.requiresPaymentProof && o.paymentProofUrl).length);
  pendingWithoutProof = computed(() => this.orders().filter(o => o.status === 'pending' && o.requiresPaymentProof && !o.paymentProofUrl).length);
  confirmedCount = computed(() => this.orders().filter(o => o.status === 'confirmed').length);
  
  // Paginación
  currentPage = signal(1);
  pageSize = 10;
  totalCount = signal(0);
  totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize));
  
  // Detalles del pedido
  selectedOrder = signal<WebOrder | null>(null);
  showOrderDetails = signal(false);
  
  // Cambiar estado
  showStatusModal = signal(false);
  newStatus = signal('');
  trackingUrl = signal('');
  estimatedDelivery = signal('');

  // Modal de aprobación/rechazo
  showApproveModal = signal(false);
  showRejectModal = signal(false);
  rejectionReason = signal('');
  isProcessing = signal(false);

  // Estados disponibles
  statuses = [
    { value: '', label: 'Todos' },
    { value: 'pending', label: 'Pendiente' },
    { value: 'confirmed', label: 'Confirmado' },
    { value: 'preparing', label: 'Preparando' },
    { value: 'shipped', label: 'Enviado' },
    { value: 'delivered', label: 'Entregado' },
    { value: 'ready_for_pickup', label: 'Listo para recoger' },
    { value: 'picked_up', label: 'Recogido' },
    { value: 'cancelled', label: 'Cancelado' }
  ];

  constructor(
    private ordersService: OrdersService,
    private toastService: ToastService
  ) {
    // Establecer fechas por defecto (últimos 30 días)
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - 30);
    
    this.endDate.set(endDate.toISOString().split('T')[0]);
    this.startDate.set(startDate.toISOString().split('T')[0]);
  }

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading.set(true);
    
    const params: any = {
      page: this.currentPage(),
      pageSize: this.pageSize
    };

    if (this.selectedStatus()) {
      params.status = this.selectedStatus();
    }

    if (this.searchTerm().trim()) {
      params.searchTerm = this.searchTerm().trim();
    }

    if (this.startDate()) {
      params.startDate = new Date(this.startDate()).toISOString();
    }

    if (this.endDate()) {
      const end = new Date(this.endDate());
      end.setHours(23, 59, 59, 999);
      params.endDate = end.toISOString();
    }

    this.ordersService.getAllOrders(params).subscribe({
      next: (result: PagedResult<WebOrder>) => {
        this.orders.set(result.items || []);
        this.totalCount.set(result.totalCount || 0);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading orders:', error);
        this.toastService.error('Error al cargar los pedidos');
        this.orders.set([]);
        this.totalCount.set(0);
        this.isLoading.set(false);
      }
    });
  }

  applyFilters(): void {
    this.currentPage.set(1);
    this.loadOrders();
  }

  viewOrderDetails(order: WebOrder): void {
    this.selectedOrder.set(order);
    this.showOrderDetails.set(true);
  }

  closeOrderDetails(): void {
    this.showOrderDetails.set(false);
    this.selectedOrder.set(null);
  }

  openStatusModal(order: WebOrder): void {
    this.selectedOrder.set(order);
    this.newStatus.set(order.status);
    this.trackingUrl.set(order.trackingUrl || '');
    if (order.estimatedDelivery) {
      const date = new Date(order.estimatedDelivery);
      this.estimatedDelivery.set(date.toISOString().split('T')[0]);
    } else {
      this.estimatedDelivery.set('');
    }
    this.showStatusModal.set(true);
  }

  closeStatusModal(): void {
    this.showStatusModal.set(false);
    this.selectedOrder.set(null);
    this.newStatus.set('');
    this.trackingUrl.set('');
    this.estimatedDelivery.set('');
  }

  updateOrderStatus(): void {
    const order = this.selectedOrder();
    if (!order) return;

    const estimatedDelivery = this.estimatedDelivery() 
      ? new Date(this.estimatedDelivery()).toISOString() 
      : undefined;

    this.ordersService.updateOrderStatus(
      order.id,
      this.newStatus(),
      this.trackingUrl() || undefined,
      estimatedDelivery
    ).subscribe({
      next: () => {
        this.toastService.success('Estado del pedido actualizado correctamente');
        this.closeStatusModal();
        this.loadOrders();
      },
      error: (error) => {
        console.error('Error updating order status:', error);
        this.toastService.error('Error al actualizar el estado del pedido');
      }
    });
  }

  getStatusLabel(status: string): string {
    const statusObj = this.statuses.find(s => s.value === status);
    return statusObj?.label || status;
  }

  getStatusColor(status: string): string {
    const colors: { [key: string]: string } = {
      'pending': 'bg-yellow-100 text-yellow-800',
      'confirmed': 'bg-blue-100 text-blue-800',
      'preparing': 'bg-purple-100 text-purple-800',
      'shipped': 'bg-indigo-100 text-indigo-800',
      'delivered': 'bg-green-100 text-green-800',
      'ready_for_pickup': 'bg-teal-100 text-teal-800',
      'picked_up': 'bg-green-100 text-green-800',
      'cancelled': 'bg-red-100 text-red-800'
    };
    return colors[status] || 'bg-gray-100 text-gray-800';
  }

  formatDate(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('es-PE', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  previousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.set(this.currentPage() - 1);
      this.loadOrders();
    }
  }

  nextPage(): void {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.set(this.currentPage() + 1);
      this.loadOrders();
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadOrders();
    }
  }

  Math = Math;

  getPagesArray(): number[] {
    const pages: number[] = [];
    const total = this.totalPages();
    const current = this.currentPage();
    
    if (total <= 7) {
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      if (current <= 3) {
        for (let i = 1; i <= 5; i++) {
          pages.push(i);
        }
        pages.push(-1); // Separador
        pages.push(total);
      } else if (current >= total - 2) {
        pages.push(1);
        pages.push(-1); // Separador
        for (let i = total - 4; i <= total; i++) {
          pages.push(i);
        }
      } else {
        pages.push(1);
        pages.push(-1); // Separador
        for (let i = current - 1; i <= current + 1; i++) {
          pages.push(i);
        }
        pages.push(-1); // Separador
        pages.push(total);
      }
    }
    
    return pages;
  }

  getStatusesForModal(): Array<{ value: string; label: string }> {
    return this.statuses.filter(s => s.value !== '');
  }

  // Métodos de aprobación rápida
  canApproveQuickly(order: WebOrder): boolean {
    // Se puede aprobar rápidamente si:
    // 1. Está pendiente
    // 2. Es efectivo (no requiere comprobante) O tiene comprobante subido
    if (order.status !== 'pending') {
      return false;
    }
    
    // Si es efectivo y no requiere comprobante, siempre se puede aprobar
    if (order.paymentMethod === 'cash' && !order.requiresPaymentProof) {
      return true;
    }
    
    // Si requiere comprobante (bank/wallet), solo se puede aprobar si ya tiene comprobante
    if (order.requiresPaymentProof) {
      return !!order.paymentProofUrl;
    }
    
    // Por defecto, si no requiere comprobante y no es efectivo, también se puede aprobar
    return true;
  }

  canRejectQuickly(order: WebOrder): boolean {
    // Se puede rechazar si está pendiente
    return order.status === 'pending';
  }

  needsPaymentProof(order: WebOrder): boolean {
    // Necesita comprobante si requiere comprobante y no lo tiene
    return order.requiresPaymentProof && !order.paymentProofUrl;
  }

  isImageFile(url: string): boolean {
    if (!url) return false;
    const lowerUrl = url.toLowerCase();
    return lowerUrl.endsWith('.jpg') || 
           lowerUrl.endsWith('.jpeg') || 
           lowerUrl.endsWith('.png') || 
           lowerUrl.endsWith('.gif') || 
           lowerUrl.endsWith('.webp') ||
           lowerUrl.includes('/payment-proofs/') && !lowerUrl.endsWith('.pdf');
  }

  isPdfFile(url: string): boolean {
    if (!url) return false;
    const lowerUrl = url.toLowerCase();
    return lowerUrl.endsWith('.pdf') || lowerUrl.includes('.pdf');
  }

  onProofImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    img.style.display = 'none';
    console.warn('Error al cargar la imagen del comprobante');
  }

  openApproveModal(order: WebOrder): void {
    if (!this.canApproveQuickly(order)) {
      if (order.requiresPaymentProof && !order.paymentProofUrl) {
        this.toastService.error('Este pedido requiere comprobante de pago antes de ser aprobado');
      } else {
        this.toastService.error('Este pedido no puede ser aprobado en este momento');
      }
      return;
    }
    this.selectedOrder.set(order);
    this.showApproveModal.set(true);
  }

  closeApproveModal(): void {
    this.showApproveModal.set(false);
    this.selectedOrder.set(null);
  }

  openRejectModal(order: WebOrder): void {
    if (!this.canRejectQuickly(order)) {
      this.toastService.error('Solo se pueden rechazar pedidos en estado "Pendiente"');
      return;
    }
    this.selectedOrder.set(order);
    this.rejectionReason.set('');
    this.showRejectModal.set(true);
  }

  closeRejectModal(): void {
    this.showRejectModal.set(false);
    this.rejectionReason.set('');
    this.selectedOrder.set(null);
  }

  approveOrder(): void {
    const order = this.selectedOrder();
    if (!order) return;

    this.isProcessing.set(true);
    const sendPaymentVerifiedEmail = order.requiresPaymentProof && !!order.paymentProofUrl;

    this.ordersService.approveOrder(order.id, sendPaymentVerifiedEmail).subscribe({
      next: () => {
        this.toastService.success(`Pedido ${order.orderNumber} aprobado correctamente. Se ha enviado un correo al cliente.`);
        this.closeApproveModal();
        this.loadOrders();
        this.isProcessing.set(false);
      },
      error: (error) => {
        console.error('Error approving order:', error);
        this.toastService.error('Error al aprobar el pedido');
        this.isProcessing.set(false);
      }
    });
  }

  rejectOrder(): void {
    const order = this.selectedOrder();
    if (!order) return;

    if (!this.rejectionReason().trim()) {
      this.toastService.error('Por favor ingresa el motivo del rechazo');
      return;
    }

    this.isProcessing.set(true);

    this.ordersService.rejectOrder(order.id, this.rejectionReason().trim()).subscribe({
      next: () => {
        this.toastService.success(`Pedido ${order.orderNumber} rechazado. Se ha enviado un correo al cliente con el motivo.`);
        this.closeRejectModal();
        this.loadOrders();
        this.isProcessing.set(false);
      },
      error: (error) => {
        console.error('Error rejecting order:', error);
        this.toastService.error('Error al rechazar el pedido');
        this.isProcessing.set(false);
      }
    });
  }

  markAsReadyForPickup(order: WebOrder): void {
    this.ordersService.updateOrderStatus(order.id, 'ready_for_pickup').subscribe({
      next: () => {
        this.toastService.success(`Pedido ${order.orderNumber} marcado como listo para recoger. Se ha enviado un correo al cliente.`);
        this.loadOrders();
        if (this.showOrderDetails()) {
          this.closeOrderDetails();
        }
      },
      error: (error) => {
        console.error('Error updating order status:', error);
        this.toastService.error('Error al actualizar el estado del pedido');
      }
    });
  }

  markAsShipped(order: WebOrder): void {
    this.ordersService.updateOrderStatus(order.id, 'shipped').subscribe({
      next: () => {
        this.toastService.success(`Pedido ${order.orderNumber} marcado como enviado. Se ha enviado un correo al cliente.`);
        this.loadOrders();
        if (this.showOrderDetails()) {
          this.closeOrderDetails();
        }
      },
      error: (error) => {
        console.error('Error updating order status:', error);
        this.toastService.error('Error al actualizar el estado del pedido');
      }
    });
  }

  getPaymentMethodLabel(paymentMethod: string): string {
    const methods: { [key: string]: string } = {
      'cash': 'Efectivo',
      'bank': 'Transferencia Bancaria',
      'wallet': 'Yape/Plin'
    };
    return methods[paymentMethod] || paymentMethod;
  }

  getPaymentMethodIcon(paymentMethod: string): string {
    const icons: { [key: string]: string } = {
      'cash': 'money',
      'bank': 'account_balance',
      'wallet': 'account_balance_wallet'
    };
    return icons[paymentMethod] || 'payment';
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text).then(() => {
      this.toastService.success('Número copiado al portapapeles');
    }).catch(() => {
      this.toastService.error('Error al copiar el número');
    });
  }

  getWhatsAppUrl(phone: string | null | undefined): string {
    if (!phone) {
      return '#';
    }
    // Limpiar el número de teléfono (solo números)
    const cleanPhone = phone.replace(/[^0-9]/g, '');
    return `https://wa.me/${cleanPhone}`;
  }
}


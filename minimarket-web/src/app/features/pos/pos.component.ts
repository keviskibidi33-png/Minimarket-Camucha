import { Component, OnInit, signal, computed, effect, afterNextRender, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductsService, Product } from '../../core/services/products.service';
import { CustomersService, Customer, CreateCustomerDto } from '../../core/services/customers.service';
import { SalesService, CartItem, CreateSaleDto, Sale } from '../../core/services/sales.service';
import { CategoriesService, CategoryDto } from '../../core/services/categories.service';
import { CashClosureService, CashClosureSummary } from '../../core/services/cash-closure.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../shared/services/toast.service';
import { Router } from '@angular/router';
import { DocumentSettingsService, DocumentViewSettings } from '../../core/services/document-settings.service';
import { SendReceiptDialogComponent } from '../../shared/components/send-receipt-dialog/send-receipt-dialog.component';
import { ConcentrationModeService } from '../../core/services/concentration-mode.service';
import { QRPaymentModalComponent, QRPaymentData } from '../../shared/components/qr-payment-modal/qr-payment-modal.component';
import { BrandSettingsService, BrandSettings } from '../../core/services/brand-settings.service';
import { DocumentTypeDialogComponent } from '../../shared/components/document-type-dialog/document-type-dialog.component';

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, SendReceiptDialogComponent, QRPaymentModalComponent, DocumentTypeDialogComponent],
  templateUrl: './pos.component.html',
  styleUrl: './pos.component.css'
})
export class PosComponent implements OnInit {
  // Products
  products = signal<Product[]>([]);
  filteredProducts = signal<Product[]>([]);
  searchTerm = signal('');
  selectedCategory = signal<string | null>(null);
  
  // Categories
  categories = signal<CategoryDto[]>([]);

  // Cart
  cartItems = signal<CartItem[]>([]);

  // Customer
  customers = signal<Customer[]>([]);
  selectedCustomer = signal<Customer | null>(null);
  customerSearchTerm = signal('');

  // Sale Configuration
  documentType = signal<'Boleta' | 'Factura'>('Boleta');
  paymentMethod = signal<'Efectivo' | 'YapePlin' | 'Transferencia'>('Efectivo');
  amountPaid = signal(0);
  discount = signal(0);

  // UI State
  isLoading = signal(false);
  showCustomerSearch = signal(false);
  showSendReceiptDialog = signal(false);
  showConfirmSendEmailDialog = signal(false);
  emailToConfirm = signal<string | null>(null);
  showQRPaymentModal = signal(false);
  showCreateCustomerModal = signal(false);
  showDocumentTypeDialog = signal(false);
  lastSale = signal<Sale | null>(null);
  activeService = signal<'ventas' | 'consultas' | 'reportes'>('ventas');
  receiptEmailSent = signal(false); // Indica si el email se envió automáticamente
  lastCustomerEmail = signal<string | null>(null); // Guarda el email del cliente antes de limpiar
  documentViewSettings = signal<DocumentViewSettings | null>(null); // Configuración de visualización de documentos
  showSaleSummary = signal(false); // Muestra el resumen de venta exitosa
  
  // Brand Settings
  brandSettings = signal<BrandSettings | null>(null);
  yapeEnabled = signal<boolean>(true); // Estado de habilitación de Yape/Plin
  bankAccountEnabled = signal<boolean>(true); // Estado de habilitación de Transferencia Bancaria
  
  // QR Payment Data
  qrPaymentData = signal<QRPaymentData | null>(null);
  
  // Cierre de Caja
  cashClosureData = signal<{
    totalPaid: number;
    totalCount: number;
    byPaymentMethod: Array<{ method: string; total: number; count: number }>;
  } | null>(null);
  isLoadingCashClosure = signal(false);
  showCashClosureModal = signal(false);

  // Consultas State
  activeQueryTab = signal<'productos' | 'ventas' | 'clientes' | 'stock'>('productos');
  
  // Consultas de Productos
  queryProductSearch = signal('');
  queryProductCategory = signal<string | null>(null);
  queryProducts = signal<Product[]>([]);
  queryProductsLoading = signal(false);
  
  // Consultas de Ventas
  querySales = signal<Sale[]>([]);
  querySalesLoading = signal(false);
  querySaleSearch = signal('');
  querySaleStartDate = signal<string>('');
  querySaleEndDate = signal<string>('');
  querySalePage = signal(1);
  querySalePageSize = signal(20);
  querySaleTotal = signal(0);
  querySaleTotalPages = computed(() => {
    const total = this.querySaleTotal();
    const pageSize = this.querySalePageSize();
    return Math.ceil(total / pageSize);
  });
  
  // Exponer Math para usar en el template
  Math = Math;
  
  getQuerySaleRangeStart(): number {
    return ((this.querySalePage() - 1) * this.querySalePageSize()) + 1;
  }
  
  getQuerySaleRangeEnd(): number {
    return Math.min(this.querySalePage() * this.querySalePageSize(), this.querySaleTotal());
  }
  
  // Estados para reenvío de documentos
  resendingEmailForSale = signal<string | null>(null);
  
  // Consultas de Clientes
  queryCustomers = signal<Customer[]>([]);
  queryCustomersLoading = signal(false);
  queryCustomerSearch = signal('');
  
  // Consultas de Stock
  queryStockProducts = signal<Product[]>([]);
  queryStockLoading = signal(false);
  queryStockFilter = signal<'low' | 'expired' | 'expiring'>('low');

  // Reportes State
  activeReportTab = signal<'ventas' | 'productos' | 'ingresos' | 'pagos' | 'cierre'>('ventas');
  
  // Cierre de Caja State
  cashClosureStartDate = signal<string>('');
  cashClosureEndDate = signal<string>('');
  showCashClosureTotal = signal(false);
  isGeneratingCashClosurePdf = signal(false);
  
  // Reportes de Ventas
  reportSalesPeriod = signal<'today' | 'week' | 'month' | 'custom'>('today');
  reportSalesStartDate = signal<string>('');
  reportSalesEndDate = signal<string>('');
  reportSalesData = signal<Sale[]>([]);
  reportSalesLoading = signal(false);
  
  // Reportes de Productos
  reportProductsPeriod = signal<'today' | 'week' | 'month' | 'custom'>('today');
  reportProductsStartDate = signal<string>('');
  reportProductsEndDate = signal<string>('');
  reportTopProducts = signal<{productId: string; productName: string; productCode: string; totalQuantity: number; totalRevenue: number}[]>([]);
  reportProductsLoading = signal(false);
  
  // Reportes de Ingresos
  reportIncomePeriod = signal<'today' | 'week' | 'month' | 'custom'>('today');
  reportIncomeStartDate = signal<string>('');
  reportIncomeEndDate = signal<string>('');
  reportIncomeData = signal<{totalSales: number; totalRevenue: number; totalTax: number; totalDiscount: number; averageSale: number} | null>(null);
  reportIncomeLoading = signal(false);
  
  // Reportes de Métodos de Pago
  reportPaymentPeriod = signal<'today' | 'week' | 'month' | 'custom'>('today');
  reportPaymentStartDate = signal<string>('');
  reportPaymentEndDate = signal<string>('');
  reportPaymentData = signal<{method: string; count: number; total: number}[]>([]);
  reportPaymentLoading = signal(false);

  // Computed values
  subtotal = computed(() => {
    return this.cartItems().reduce((sum, item) => sum + item.subtotal, 0);
  });

  tax = computed(() => {
    const subtotalAfterDiscount = this.subtotal() - this.discount();
    return subtotalAfterDiscount * 0.18; // IGV 18%
  });

  total = computed(() => {
    const subtotalAfterDiscount = this.subtotal() - this.discount();
    const tax = subtotalAfterDiscount * 0.18; // IGV 18%
    return subtotalAfterDiscount + tax;
  });

  change = computed(() => {
    const change = this.amountPaid() - this.total();
    return change > 0 ? change : 0;
  });

  totalProductsCount = computed(() => {
    return this.categories().reduce((sum, cat) => sum + (cat.productCount ?? 0), 0);
  });

  // Computed values para reportes
  reportSalesTotalRevenue = computed(() => {
    return this.reportSalesData().reduce((sum, s) => sum + s.total, 0);
  });

  reportSalesAverage = computed(() => {
    const sales = this.reportSalesData();
    return sales.length > 0 ? this.reportSalesTotalRevenue() / sales.length : 0;
  });

  reportSalesTotalDiscount = computed(() => {
    return this.reportSalesData().reduce((sum, s) => sum + s.discount, 0);
  });

  reportPaymentTotal = computed(() => {
    return this.reportPaymentData().reduce((sum, p) => sum + p.total, 0);
  });

  private readonly destroyRef = inject(DestroyRef);
  private paymentEffectCleanup?: ReturnType<typeof effect>;

  // Formulario de creación de cliente
  createCustomerForm: FormGroup;
  isCreatingCustomer = signal(false);

  constructor(
    private productsService: ProductsService,
    private customersService: CustomersService,
    private categoriesService: CategoriesService,
    private salesService: SalesService,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router,
    public concentrationModeService: ConcentrationModeService,
    private brandSettingsService: BrandSettingsService,
    private documentSettingsService: DocumentSettingsService,
    private cashClosureService: CashClosureService,
    private fb: FormBuilder
  ) {
    this.createCustomerForm = this.fb.group({
      documentType: ['DNI', [Validators.required]],
      documentNumber: ['', [Validators.required, Validators.pattern(/^\d+$/)]],
      name: ['', [Validators.required, Validators.maxLength(200)]],
      email: ['', [Validators.email, Validators.maxLength(100)]],
      phone: ['+51 ', [Validators.maxLength(20), Validators.pattern(/^(\+51)?\s*9\d{8}$/)]],
      address: ['', [Validators.maxLength(500)]]
    });

    // Validación dinámica de longitud de documento según tipo
    this.createCustomerForm.get('documentType')?.valueChanges.subscribe(type => {
      const docControl = this.createCustomerForm.get('documentNumber');
      if (type === 'DNI') {
        docControl?.setValidators([Validators.required, Validators.pattern(/^\d{8}$/)]);
      } else if (type === 'RUC') {
        docControl?.setValidators([Validators.required, Validators.pattern(/^\d{11}$/)]);
      }
      docControl?.updateValueAndValidity();
    });
  }

  // Helper methods for template
  parseFloat = parseFloat;

  ngOnInit(): void {
    // Cargar el servicio activo desde localStorage
    const savedService = localStorage.getItem('pos_active_service');
    if (savedService && ['ventas', 'consultas', 'reportes'].includes(savedService)) {
      this.activeService.set(savedService as 'ventas' | 'consultas' | 'reportes');
    }

    this.loadProducts();
    this.loadCustomers();
    this.loadCategories();
    this.loadBrandSettings();
    this.loadDocumentViewSettings();
    
    // Inicializar fechas de cierre de caja con el día actual
    this.setTodayDateRange();

    // Cargar datos si el servicio activo es consultas o reportes
    if (this.activeService() === 'consultas') {
      this.loadQueryData();
    } else if (this.activeService() === 'reportes') {
      this.loadReportData();
    }
    
    // Observar cambios en total para actualizar amountPaid automáticamente
    // Se ejecuta después del siguiente renderizado para asegurar que todo esté inicializado
    afterNextRender(() => {
      // Observar cambios en total y cartItems para actualizar amountPaid automáticamente
      // Para métodos que no son efectivo, siempre debe ser igual al total
      this.paymentEffectCleanup = effect(() => {
        // Observar cartItems para que el effect se dispare cuando cambie
        const items = this.cartItems();
        const total = this.total();
        const method = this.paymentMethod();
        
        if (method !== 'Efectivo') {
          // Para métodos que no son efectivo, siempre igual al total
          this.amountPaid.set(total);
        } else {
          // Para efectivo, actualizar solo si el monto pagado es menor al total
          // Esto permite que el usuario ingrese un monto mayor manualmente
          if (this.amountPaid() < total) {
            this.amountPaid.set(total);
          }
        }
      }, { allowSignalWrites: true });
    });

    // Limpiar los effects cuando el componente se destruya (fuera del callback de afterNextRender)
    this.destroyRef.onDestroy(() => {
      this.paymentEffectCleanup?.destroy();
    });
  }

  setActiveService(service: 'ventas' | 'consultas' | 'reportes'): void {
    this.activeService.set(service);
    localStorage.setItem('pos_active_service', service);
    
    // Cargar datos cuando se activa consultas o reportes
    if (service === 'consultas') {
      this.loadQueryData();
    } else if (service === 'reportes') {
      this.loadReportData();
    }
  }

  setActiveQueryTab(tab: 'productos' | 'ventas' | 'clientes' | 'stock'): void {
    this.activeQueryTab.set(tab);
    this.loadQueryData();
  }

  loadQueryData(): void {
    const tab = this.activeQueryTab();
    switch (tab) {
      case 'productos':
        this.loadQueryProducts();
        break;
      case 'ventas':
        this.loadQuerySales();
        break;
      case 'clientes':
        this.loadQueryCustomers();
        break;
      case 'stock':
        this.loadQueryStock();
        break;
    }
  }

  loadQueryProducts(): void {
    this.queryProductsLoading.set(true);
    const categoryId = this.queryProductCategory();
    const searchTerm = this.queryProductSearch().trim();
    
    this.productsService.getAll({
      isActive: true,
      categoryId: categoryId || undefined,
      searchTerm: searchTerm || undefined,
      page: 1,
      pageSize: 100
    }).subscribe({
      next: (result) => {
        const products = result.items || result;
        this.queryProducts.set(Array.isArray(products) ? products : []);
        this.queryProductsLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading query products:', error);
        this.toastService.error('Error al cargar productos');
        this.queryProductsLoading.set(false);
      }
    });
  }

  loadQuerySales(): void {
    this.querySalesLoading.set(true);
    const params: any = {
      page: this.querySalePage(),
      pageSize: this.querySalePageSize()
    };

    if (this.querySaleSearch().trim()) {
      params.documentNumber = this.querySaleSearch().trim();
    }

    if (this.querySaleStartDate()) {
      params.startDate = new Date(this.querySaleStartDate()).toISOString();
    }

    if (this.querySaleEndDate()) {
      params.endDate = new Date(this.querySaleEndDate()).toISOString();
    }

    this.salesService.getAll(params).subscribe({
      next: (pagedResult) => {
        this.querySales.set(pagedResult.items);
        this.querySaleTotal.set(pagedResult.totalCount);
        this.querySalesLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading query sales:', error);
        this.toastService.error('Error al cargar ventas');
        this.querySalesLoading.set(false);
      }
    });
  }

  goToQuerySalePage(page: number): void {
    const totalPages = this.querySaleTotalPages();
    if (page >= 1 && page <= totalPages) {
      this.querySalePage.set(page);
      this.loadQuerySales();
    }
  }

  resendReceiptFromTable(sale: Sale): void {
    if (!sale) return;

    // Obtener el email del cliente
    const customerEmail = sale.customerEmail;
    
    if (!customerEmail || customerEmail.trim() === '') {
      // Si no hay email, mostrar modal para ingresar
      this.showSendReceiptDialog.set(true);
      // Guardar la venta temporalmente para usar después
      this.lastSale.set(sale);
      return;
    }

    // Si hay email, enviar directamente con confirmación rápida
    if (!confirm(`¿Reenviar el comprobante ${sale.documentNumber} a ${customerEmail}?`)) {
      return;
    }

    // Si hay email, enviar directamente
    this.resendingEmailForSale.set(sale.id);
    this.salesService.sendReceipt(sale.id, customerEmail, sale.documentType).subscribe({
      next: () => {
        this.resendingEmailForSale.set(null);
        this.toastService.success(`✅ Comprobante reenviado exitosamente a ${customerEmail}`);
      },
      error: (error) => {
        console.error('Error resending receipt:', error);
        this.resendingEmailForSale.set(null);
        // Si falla, mostrar modal para ingresar otro correo
        this.showSendReceiptDialog.set(true);
        this.lastSale.set(sale);
        this.toastService.error(error.error?.errors?.[0] || 'Error al reenviar el comprobante. Puedes intentar con otro correo.');
      }
    });
  }

  loadQueryCustomers(): void {
    this.queryCustomersLoading.set(true);
    const searchTerm = this.queryCustomerSearch().trim();
    
    this.customersService.getAll({
      searchTerm: searchTerm || undefined,
      page: 1,
      pageSize: 100
    }).subscribe({
      next: (result) => {
        const customers = result.items || result;
        this.queryCustomers.set(Array.isArray(customers) ? customers : []);
        this.queryCustomersLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading query customers:', error);
        this.toastService.error('Error al cargar clientes');
        this.queryCustomersLoading.set(false);
      }
    });
  }

  loadQueryStock(): void {
    this.queryStockLoading.set(true);
    
    this.productsService.getAll({
      isActive: true,
      page: 1,
      pageSize: 200
    }).subscribe({
      next: (result) => {
        const products = result.items || result;
        const allProducts = Array.isArray(products) ? products : [];
        
        const filter = this.queryStockFilter();
        const today = new Date();
        const nextWeek = new Date();
        nextWeek.setDate(today.getDate() + 7);
        
        let filtered: Product[] = [];
        
        switch (filter) {
          case 'low':
            filtered = allProducts.filter(p => p.stock <= 10);
            break;
          case 'expired':
            filtered = allProducts.filter(p => 
              p.expirationDate && new Date(p.expirationDate) < today
            );
            break;
          case 'expiring':
            filtered = allProducts.filter(p => 
              p.expirationDate && 
              new Date(p.expirationDate) >= today && 
              new Date(p.expirationDate) <= nextWeek
            );
            break;
        }
        
        this.queryStockProducts.set(filtered);
        this.queryStockLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading query stock:', error);
        this.toastService.error('Error al cargar inventario');
        this.queryStockLoading.set(false);
      }
    });
  }

  onQueryProductSearch(): void {
    this.loadQueryProducts();
  }

  onQuerySaleSearch(): void {
    this.querySalePage.set(1);
    this.loadQuerySales();
  }

  onQueryCustomerSearch(): void {
    this.loadQueryCustomers();
  }

  onQueryStockFilterChange(): void {
    this.loadQueryStock();
  }

  formatDate(date: string | Date): string {
    const d = typeof date === 'string' ? new Date(date) : date;
    return d.toLocaleDateString('es-PE', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatCurrency(amount: number): string {
    return `S/ ${amount.toFixed(2)}`;
  }

  isExpired(expirationDate?: string): boolean {
    if (!expirationDate) return false;
    return new Date(expirationDate) < new Date();
  }

  isExpiringSoon(expirationDate?: string): boolean {
    if (!expirationDate) return false;
    const today = new Date();
    const nextWeek = new Date();
    nextWeek.setDate(today.getDate() + 7);
    const expDate = new Date(expirationDate);
    return expDate >= today && expDate <= nextWeek;
  }

  setActiveReportTab(tab: 'ventas' | 'productos' | 'ingresos' | 'pagos' | 'cierre'): void {
    this.activeReportTab.set(tab);
    this.loadReportData();
  }

  loadReportData(): void {
    const tab = this.activeReportTab();
    switch (tab) {
      case 'ventas':
        this.loadSalesReport();
        break;
      case 'productos':
        this.loadProductsReport();
        break;
      case 'ingresos':
        this.loadIncomeReport();
        break;
      case 'pagos':
        this.loadPaymentReport();
        break;
      case 'cierre':
        this.loadCashClosure();
        break;
    }
  }

  getDateRange(period: 'today' | 'week' | 'month' | 'custom', startDate?: string, endDate?: string): { start: Date; end: Date } {
    const today = new Date();
    today.setHours(23, 59, 59, 999);
    const start = new Date();
    start.setHours(0, 0, 0, 0);

    if (period === 'custom' && startDate && endDate) {
      return {
        start: new Date(startDate),
        end: new Date(endDate + 'T23:59:59')
      };
    }

    switch (period) {
      case 'today':
        return { start, end: today };
      case 'week':
        start.setDate(today.getDate() - 7);
        return { start, end: today };
      case 'month':
        start.setDate(1);
        return { start, end: today };
      default:
        return { start, end: today };
    }
  }

  loadSalesReport(): void {
    this.reportSalesLoading.set(true);
    const period = this.reportSalesPeriod();
    const { start, end } = this.getDateRange(
      period,
      this.reportSalesStartDate(),
      this.reportSalesEndDate()
    );

    this.salesService.getAll({
      startDate: start.toISOString(),
      endDate: end.toISOString(),
      page: 1,
      pageSize: 1000
    }).subscribe({
      next: (pagedResult) => {
        this.reportSalesData.set(pagedResult.items || []);
        this.reportSalesLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading sales report:', error);
        this.toastService.error('Error al cargar reporte de ventas');
        this.reportSalesData.set([]);
        this.reportSalesLoading.set(false);
      }
    });
  }

  loadProductsReport(): void {
    this.reportProductsLoading.set(true);
    const period = this.reportProductsPeriod();
    const { start, end } = this.getDateRange(
      period,
      this.reportProductsStartDate(),
      this.reportProductsEndDate()
    );

    this.salesService.getAll({
      startDate: start.toISOString(),
      endDate: end.toISOString(),
      page: 1,
      pageSize: 1000
    }).subscribe({
      next: (pagedResult) => {
        // Agrupar productos por ventas
        const productMap = new Map<string, {productId: string; productName: string; productCode: string; totalQuantity: number; totalRevenue: number}>();
        
        const sales = pagedResult.items || [];
        sales.forEach(sale => {
          if (sale.saleDetails && sale.saleDetails.length > 0) {
            sale.saleDetails.forEach(detail => {
              const existing = productMap.get(detail.productId);
              if (existing) {
                existing.totalQuantity += detail.quantity;
                existing.totalRevenue += detail.subtotal;
              } else {
                productMap.set(detail.productId, {
                  productId: detail.productId,
                  productName: detail.productName,
                  productCode: detail.productCode,
                  totalQuantity: detail.quantity,
                  totalRevenue: detail.subtotal
                });
              }
            });
          }
        });

        const topProducts = Array.from(productMap.values())
          .sort((a, b) => b.totalQuantity - a.totalQuantity)
          .slice(0, 20);

        this.reportTopProducts.set(topProducts);
        this.reportProductsLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading products report:', error);
        this.toastService.error('Error al cargar reporte de productos');
        this.reportTopProducts.set([]);
        this.reportProductsLoading.set(false);
      }
    });
  }

  loadIncomeReport(): void {
    this.reportIncomeLoading.set(true);
    const period = this.reportIncomePeriod();
    const { start, end } = this.getDateRange(
      period,
      this.reportIncomeStartDate(),
      this.reportIncomeEndDate()
    );

    this.salesService.getAll({
      startDate: start.toISOString(),
      endDate: end.toISOString(),
      page: 1,
      pageSize: 1000
    }).subscribe({
      next: (pagedResult) => {
        const sales = (pagedResult.items || []).filter(s => s.status === 'Pagado');
        const totalSales = sales.length;
        const totalRevenue = sales.reduce((sum, s) => sum + s.total, 0);
        const totalTax = sales.reduce((sum, s) => sum + s.tax, 0);
        const totalDiscount = sales.reduce((sum, s) => sum + s.discount, 0);
        const averageSale = totalSales > 0 ? totalRevenue / totalSales : 0;

        this.reportIncomeData.set({
          totalSales,
          totalRevenue,
          totalTax,
          totalDiscount,
          averageSale
        });
        this.reportIncomeLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading income report:', error);
        this.toastService.error('Error al cargar reporte de ingresos');
        this.reportIncomeData.set(null);
        this.reportIncomeLoading.set(false);
      }
    });
  }

  loadPaymentReport(): void {
    this.reportPaymentLoading.set(true);
    const period = this.reportPaymentPeriod();
    const { start, end } = this.getDateRange(
      period,
      this.reportPaymentStartDate(),
      this.reportPaymentEndDate()
    );

    this.salesService.getAll({
      startDate: start.toISOString(),
      endDate: end.toISOString(),
      page: 1,
      pageSize: 1000
    }).subscribe({
      next: (pagedResult) => {
        const sales = (pagedResult.items || []).filter(s => s.status === 'Pagado');
        const paymentMap = new Map<string, {method: string; count: number; total: number}>();

        sales.forEach(sale => {
          const existing = paymentMap.get(sale.paymentMethod);
          if (existing) {
            existing.count += 1;
            existing.total += sale.total;
          } else {
            paymentMap.set(sale.paymentMethod, {
              method: sale.paymentMethod,
              count: 1,
              total: sale.total
            });
          }
        });

        const paymentData = Array.from(paymentMap.values())
          .sort((a, b) => b.total - a.total);

        this.reportPaymentData.set(paymentData);
        this.reportPaymentLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading payment report:', error);
        this.toastService.error('Error al cargar reporte de métodos de pago');
        this.reportPaymentData.set([]);
        this.reportPaymentLoading.set(false);
      }
    });
  }

  onReportPeriodChange(tab: 'ventas' | 'productos' | 'ingresos' | 'pagos'): void {
    switch (tab) {
      case 'ventas':
        this.loadSalesReport();
        break;
      case 'productos':
        this.loadProductsReport();
        break;
      case 'ingresos':
        this.loadIncomeReport();
        break;
      case 'pagos':
        this.loadPaymentReport();
        break;
    }
  }

  loadCategories(): void {
    this.categoriesService.getAllWithoutPagination().subscribe({
      next: (categories) => {
        // Filtrar solo categorías activas y ordenar por nombre
        const activeCategories = categories
          .filter((cat: CategoryDto) => cat.isActive)
          .sort((a: CategoryDto, b: CategoryDto) => a.name.localeCompare(b.name));
        this.categories.set(activeCategories);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  loadProducts(): void {
    this.isLoading.set(true);
    const categoryId = this.selectedCategory();
    this.productsService.getAll({ 
      isActive: true,
      categoryId: categoryId || undefined
    }).subscribe({
      next: (result) => {
        const products = result.items || result;
        this.products.set(Array.isArray(products) ? products : []);
        this.applyFilters();
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.toastService.error('Error al cargar productos');
        this.isLoading.set(false);
      }
    });
  }

  onCategorySelect(categoryId: string | null): void {
    this.selectedCategory.set(categoryId);
    // Limpiar búsqueda al cambiar de categoría para evitar conflictos
    this.searchTerm.set('');
    this.loadProducts();
  }

  loadCustomers(): void {
    this.customersService.getAll().subscribe({
      next: (result) => {
        const customers = result.items || result;
        this.customers.set(Array.isArray(customers) ? customers : []);
      },
      error: (error) => {
        console.error('Error loading customers:', error);
      }
    });
  }

  loadBrandSettings(): void {
    this.brandSettingsService.get().subscribe({
      next: (settings: BrandSettings | null) => {
        this.brandSettings.set(settings);
        if (settings) {
          // Cargar estado de habilitación de Yape/Plin (unificado)
          const isYapeEnabled = settings.yapeEnabled ?? settings.plinEnabled ?? false;
          this.yapeEnabled.set(isYapeEnabled);
          
          // Si está deshabilitado y estaba seleccionado, cambiar a Efectivo
          if (!isYapeEnabled && this.paymentMethod() === 'YapePlin') {
            this.paymentMethod.set('Efectivo');
          }
          
          // Cargar estado de habilitación de Transferencia Bancaria
          const isBankEnabled = settings.bankAccountVisible ?? false;
          this.bankAccountEnabled.set(isBankEnabled);
          
          // Si está deshabilitado y estaba seleccionado, cambiar a Efectivo
          if (!isBankEnabled && this.paymentMethod() === 'Transferencia') {
            this.paymentMethod.set('Efectivo');
          }
        } else {
          // Si no hay settings, deshabilitar por defecto
          this.yapeEnabled.set(false);
          this.bankAccountEnabled.set(false);
          if (this.paymentMethod() === 'YapePlin' || this.paymentMethod() === 'Transferencia') {
            this.paymentMethod.set('Efectivo');
          }
        }
      },
      error: (error: any) => {
        console.error('Error loading brand settings:', error);
        // En caso de error, deshabilitar por defecto
        this.yapeEnabled.set(false);
        this.bankAccountEnabled.set(false);
        if (this.paymentMethod() === 'YapePlin' || this.paymentMethod() === 'Transferencia') {
          this.paymentMethod.set('Efectivo');
        }
      }
    });
  }

  applyFilters(): void {
    const term = this.searchTerm().toLowerCase().trim();
    let filtered = [...this.products()];

    // Filtrar por término de búsqueda
    if (term) {
      filtered = filtered.filter(p =>
        p.name.toLowerCase().includes(term) ||
        p.code.toLowerCase().includes(term)
      );
    }
    
    this.filteredProducts.set(filtered);
  }

  onSearch(): void {
    const term = this.searchTerm().toLowerCase().trim();
    
    // Si el término es solo números, buscar por código exacto primero
    if (term && /^\d+$/.test(term)) {
      const exactMatch = this.products().find(p => p.code.toLowerCase() === term);
      if (exactMatch) {
        this.addToCart(exactMatch);
        this.searchTerm.set('');
        this.applyFilters();
        return;
      }
    }

    // Aplicar filtros de búsqueda
    this.applyFilters();
  }

  addToCart(product: Product): void {
    if (product.stock === 0) {
      this.toastService.warning('Producto sin stock disponible');
      return;
    }

    const existingItem = this.cartItems().find(item => item.productId === product.id);

    if (existingItem) {
      // Incrementar cantidad si hay stock
      if (existingItem.quantity >= product.stock) {
        this.toastService.warning('Stock insuficiente');
        return;
      }
      this.updateCartItemQuantity(existingItem.productId, existingItem.quantity + 1);
    } else {
      const newItem: CartItem = {
        productId: product.id,
        productCode: product.code,
        productName: product.name,
        quantity: 1,
        unitPrice: product.salePrice,
        subtotal: product.salePrice
      };
      this.cartItems.set([...this.cartItems(), newItem]);
    }

    // Actualizar amountPaid si es necesario (solo si es efectivo)
    if (this.paymentMethod() === 'Efectivo' && this.amountPaid() < this.total()) {
      this.amountPaid.set(this.total());
    }
  }

  updateCartItemQuantity(productId: string, quantity: number): void {
    const product = this.products().find(p => p.id === productId);
    if (product && quantity > product.stock) {
      this.toastService.warning('Stock insuficiente');
      return;
    }

    if (quantity <= 0) {
      this.removeFromCart(productId);
      return;
    }

    const updatedItems = this.cartItems().map(item => {
      if (item.productId === productId) {
        return {
          ...item,
          quantity,
          subtotal: item.unitPrice * quantity
        };
      }
      return item;
    });

    this.cartItems.set(updatedItems);

    // Actualizar amountPaid automáticamente cuando cambia la cantidad
    // Para métodos que no son efectivo, siempre debe ser igual al total
    if (this.paymentMethod() !== 'Efectivo') {
      this.amountPaid.set(this.total());
    } else if (this.amountPaid() < this.total()) {
      // Para efectivo, solo actualizar si el monto pagado es menor al nuevo total
      this.amountPaid.set(this.total());
    }
  }

  removeFromCart(productId: string): void {
    this.cartItems.set(this.cartItems().filter(item => item.productId !== productId));
    
    // Actualizar amountPaid automáticamente cuando se elimina un producto
    if (this.paymentMethod() !== 'Efectivo') {
      this.amountPaid.set(this.total());
    } else if (this.amountPaid() < this.total()) {
      this.amountPaid.set(this.total());
    }
  }

  setQuickAmount(amount: number): void {
    // Solo permitir establecer montos rápidos si el método de pago es Efectivo
    if (this.paymentMethod() === 'Efectivo') {
      this.amountPaid.set(amount);
    }
  }

  clearCart(): void {
    this.cartItems.set([]);
    this.amountPaid.set(0);
    this.discount.set(0);
    this.selectedCustomer.set(null);
    this.searchTerm.set('');
    this.filteredProducts.set(this.products());
  }

  onDocumentTypeChange(): void {
    // Si cambia a Factura y el cliente actual no tiene RUC, limpiar selección
    if (this.documentType() === 'Factura') {
      const customer = this.selectedCustomer();
      if (customer && (customer.documentType !== 'RUC' || customer.documentNumber.length !== 11)) {
        this.selectedCustomer.set(null);
        this.toastService.info('Las facturas requieren un cliente con RUC válido. Por favor, seleccione un cliente con RUC.');
      }
      // Mostrar búsqueda de cliente si no hay uno seleccionado
      if (!this.selectedCustomer()) {
        this.showCustomerSearch.set(true);
      }
    }
    // Para Boleta, no es necesario limpiar el cliente (puede tener DNI o ser público general)
  }

  selectCustomer(customer: Customer): void {
    this.selectedCustomer.set(customer);
    this.showCustomerSearch.set(false);
    this.customerSearchTerm.set('');
  }

  openCreateCustomerModal(): void {
    // Determinar el tipo de documento por defecto según el tipo de comprobante
    const defaultDocumentType = this.documentType() === 'Factura' ? 'RUC' : 'DNI';
    
    this.createCustomerForm.reset({
      documentType: defaultDocumentType,
      documentNumber: '',
      name: '',
      email: '',
      phone: '+51 ',
      address: ''
    });
    this.showCreateCustomerModal.set(true);
  }

  closeCreateCustomerModal(): void {
    this.showCreateCustomerModal.set(false);
    this.createCustomerForm.reset({
      documentType: 'DNI',
      documentNumber: '',
      name: '',
      email: '',
      phone: '+51 ',
      address: ''
    });
  }

  onCreateCustomer(): void {
    if (this.createCustomerForm.invalid) {
      // Marcar todos los campos como touched para mostrar errores
      Object.keys(this.createCustomerForm.controls).forEach(key => {
        this.createCustomerForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.isCreatingCustomer.set(true);
    const customerData: CreateCustomerDto = this.createCustomerForm.value;

    this.customersService.create(customerData).subscribe({
      next: (newCustomer) => {
        this.toastService.success('Cliente creado exitosamente');
        // Agregar el nuevo cliente a la lista
        this.customers.set([...this.customers(), newCustomer]);
        // Seleccionar el cliente recién creado
        this.selectCustomer(newCustomer);
        // Cerrar el modal
        this.closeCreateCustomerModal();
        this.isCreatingCustomer.set(false);
      },
      error: (error) => {
        console.error('Error creating customer:', error);
        const errorMessage = error.error?.message || error.error?.errors?.[0] || 'Error al crear el cliente';
        this.toastService.error(errorMessage);
        this.isCreatingCustomer.set(false);
      }
    });
  }

  onPaymentMethodChange(): void {
    // Para todos los métodos, inicializar con el total
    // Para Efectivo, el usuario puede ajustar el monto
    // Para otros métodos, el monto debe ser igual al total
    this.amountPaid.set(this.total());
    
    // Si se selecciona YapePlin, mostrar modal QR
    if (this.paymentMethod() === 'YapePlin') {
      this.showQRPaymentModalForYapePlin();
    }
  }

  showQRPaymentModalForYapePlin(): void {
    const settings = this.brandSettings();
    if (!settings) {
      this.toastService.warning('Configuración de marca no disponible. Cargando...');
      this.loadBrandSettings();
      return;
    }

    // Determinar si usar Yape o Plin (prioridad a Yape si ambos están habilitados)
    const useYape = settings.yapeEnabled && settings.yapePhone;
    const usePlin = !useYape && settings.plinEnabled && settings.plinPhone;

    if (!useYape && !usePlin) {
      this.toastService.error('Yape/Plin no está configurado. Configure los datos de pago en Configuración.');
      this.paymentMethod.set('Efectivo');
      return;
    }

    const paymentType = useYape ? 'Yape' : 'Plin';
    const phoneNumber = useYape ? settings.yapePhone! : settings.plinPhone!;
    const qrImageUrl = useYape ? settings.yapeQRUrl : settings.plinQRUrl;

    // Generar referencia temporal (se reemplazará con el número real de documento al procesar)
    const tempReference = this.generateTempReference();

    const qrData: QRPaymentData = {
      amount: this.total(),
      phoneNumber: phoneNumber,
      qrImageUrl: qrImageUrl,
      reference: tempReference,
      paymentType: paymentType as 'Yape' | 'Plin'
    };

    this.qrPaymentData.set(qrData);
    this.showQRPaymentModal.set(true);
  }

  generateTempReference(): string {
    // Generar una referencia temporal basada en timestamp
    // Formato: TEMP-YYYYMMDD-HHMMSS
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');
    return `TEMP-${year}${month}${day}-${hours}${minutes}${seconds}`;
  }

  onQRPaymentConfirm(): void {
    // Cerrar modal y procesar venta
    this.showQRPaymentModal.set(false);
    this.processSale();
  }

  onQRPaymentCancel(): void {
    // Cerrar modal y cambiar método de pago a Efectivo
    this.showQRPaymentModal.set(false);
    this.paymentMethod.set('Efectivo');
  }

  processSale(): void {
    if (this.cartItems().length === 0) {
      this.toastService.warning('El carrito está vacío');
      return;
    }

    // Mostrar diálogo para seleccionar tipo de comprobante
    this.showDocumentTypeDialog.set(true);
  }

  onDocumentTypeSelected(type: 'Boleta' | 'Factura'): void {
    this.documentType.set(type);
    this.showDocumentTypeDialog.set(false);
    
    // Validar que si es Factura, el cliente tenga RUC válido
    if (type === 'Factura') {
      const customer = this.selectedCustomer();
      if (!customer) {
        this.toastService.warning('Las facturas requieren un cliente con RUC válido');
        this.showCustomerSearch.set(true);
        this.documentType.set('Boleta'); // Revertir a Boleta
        return;
      }

      // Validar que el cliente tenga RUC (11 dígitos)
      if (customer.documentType !== 'RUC' || customer.documentNumber.length !== 11) {
        this.toastService.error('Las facturas requieren un cliente con RUC válido (11 dígitos)');
        this.showCustomerSearch.set(true);
        this.documentType.set('Boleta'); // Revertir a Boleta
        return;
      }
    }
    // Para Boleta, no se requiere validación especial (puede tener cliente con DNI o ser público general)

    // Continuar con el proceso de venta
    this.continueSaleProcess();
  }

  onDocumentTypeCancelled(): void {
    this.showDocumentTypeDialog.set(false);
  }

  continueSaleProcess(): void {
    if (this.amountPaid() < this.total()) {
      this.toastService.warning('El monto pagado es menor al total');
      return;
    }

    // Si es YapePlin y el modal no está abierto, mostrar modal primero
    if (this.paymentMethod() === 'YapePlin' && !this.showQRPaymentModal()) {
      this.showQRPaymentModalForYapePlin();
      return;
    }

    // Validar límite de items (500 máximo para prevenir problemas de memoria en PDF)
    if (this.cartItems().length > 500) {
      this.toastService.error('El carrito no puede tener más de 500 productos. Por favor, divida la venta en múltiples transacciones.');
      return;
    }

    // Validar stock antes de procesar
    const stockErrors = this.cartItems().filter(item => {
      const product = this.products().find(p => p.id === item.productId);
      return product && item.quantity > product.stock;
    });

    if (stockErrors.length > 0) {
      this.toastService.error('Uno o más productos no tienen stock suficiente');
      return;
    }

    this.isLoading.set(true);

    const saleDto: CreateSaleDto = {
      documentType: this.documentType(),
      customerId: this.selectedCustomer()?.id,
      paymentMethod: this.paymentMethod(),
      amountPaid: this.amountPaid(),
      discount: this.discount(),
      saleDetails: this.cartItems().map(item => ({
        productId: item.productId,
        quantity: item.quantity,
        unitPrice: item.unitPrice
      }))
    };

    this.salesService.create(saleDto).subscribe({
      next: (sale) => {
        this.lastSale.set(sale);
        
        // Cerrar modal QR si estaba abierto
        this.showQRPaymentModal.set(false);
        
        // IMPORTANTE: Guardar el email del cliente ANTES de limpiar el carrito
        const customerEmail = this.selectedCustomer()?.email;
        const hasCustomerEmail = !!(customerEmail && customerEmail.trim() !== '');
        
        // Guardar el email para mostrarlo en el resumen
        this.lastCustomerEmail.set(hasCustomerEmail ? customerEmail! : null);
        
        // Recargar productos para actualizar stock
        this.loadProducts();
        this.clearCart();
        this.isLoading.set(false);
        
        // Actualizar cierre de caja después de una venta exitosa
        this.loadCashClosure();
        
        // Mostrar resumen de venta (sin interrumpir el flujo)
        // NO establecer receiptEmailSent como true automáticamente
        // El envío se hace en segundo plano en el backend, pero no sabemos si fue exitoso
        // Solo mostraremos "Enviado a..." cuando el usuario haga clic en "Enviar por Email" y se confirme
        this.receiptEmailSent.set(false);
        this.showSaleSummary.set(true);
        
        // Si el cliente tiene email, el backend intentará enviarlo automáticamente en segundo plano
        // Pero no mostramos confirmación hasta que el usuario lo solicite explícitamente
      },
      error: (error) => {
        console.error('Error creating sale:', error);
        
        // Manejo robusto de errores con mensajes específicos
        let errorMessage = 'Error al procesar la venta';
        
        // Errores de timeout o memoria (ventas grandes)
        if (error.status === 0 || error.status === 504) {
          errorMessage = 'La operación está tomando demasiado tiempo. Por favor, intente con menos productos o contacte al administrador.';
        } else if (error.status === 413 || error.message?.includes('memory') || error.message?.includes('out of memory')) {
          errorMessage = 'La venta es demasiado grande. Por favor, divida la venta en múltiples transacciones (máximo 500 productos por venta).';
        } else if (error.error?.errors && Array.isArray(error.error.errors) && error.error.errors.length > 0) {
          errorMessage = error.error.errors.join('. ');
        } else if (error.error?.message) {
          errorMessage = error.error.message;
        } else if (error.error?.errors && typeof error.error.errors === 'object') {
          // Si errors es un objeto con campos
          const errorMessages = Object.values(error.error.errors).flat();
          if (errorMessages.length > 0) {
            errorMessage = errorMessages.join('. ');
          }
        } else if (error.message) {
          errorMessage = error.message;
        }
        
        this.toastService.error(errorMessage);
        this.isLoading.set(false);
        
        // Si el error es crítico, no limpiar el carrito para que el usuario pueda reintentar
        if (error.status !== 400 && error.status !== 422) {
          // Errores de servidor: mantener el carrito
          console.warn('Error de servidor. El carrito se mantiene para permitir reintento.');
        }
      }
    });
  }

  onSendReceipt(event: { email: string }): void {
    const sale = this.lastSale();
    if (!sale) return;

    this.salesService.sendReceipt(sale.id, event.email, sale.documentType).subscribe({
      next: () => {
        // Actualizar los signals para mostrar "Enviado a..." en el resumen
        this.lastCustomerEmail.set(event.email);
        this.receiptEmailSent.set(true);
        this.toastService.success(`Comprobante enviado exitosamente a ${event.email}`);
        this.showSendReceiptDialog.set(false);
        // Recargar ventas si estamos en la vista de consultas
        if (this.activeService() === 'consultas' && this.activeQueryTab() === 'ventas') {
          this.loadQuerySales();
        }
      },
      error: (error) => {
        console.error('Error sending receipt:', error);
        // No actualizar los signals si hay error
        this.toastService.error(error.error?.errors?.[0] || error.error?.message || 'Error al enviar el comprobante');
      }
    });
  }

  handleSendReceiptClick(): void {
    const sale = this.lastSale();
    if (!sale) return;

    // Obtener el email del cliente de la venta o del cliente seleccionado
    const customerEmail = sale.customerEmail || this.lastCustomerEmail() || this.selectedCustomer()?.email;
    
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
    const sale = this.lastSale();
    const email = this.emailToConfirm();
    
    if (!sale || !email) return;

    this.salesService.sendReceipt(sale.id, email, sale.documentType).subscribe({
      next: () => {
        this.lastCustomerEmail.set(email);
        this.receiptEmailSent.set(true);
        this.showConfirmSendEmailDialog.set(false);
        this.emailToConfirm.set(null);
        this.toastService.success(`Comprobante enviado exitosamente a ${email}`);
      },
      error: (error) => {
        console.error('Error sending receipt:', error);
        // Si falla, mostrar el modal para que el usuario pueda ingresar otro correo
        this.showConfirmSendEmailDialog.set(false);
        this.showSendReceiptDialog.set(true);
        this.toastService.error(error.error?.errors?.[0] || error.error?.message || 'Error al enviar el comprobante');
      }
    });
  }

  cancelConfirmSendEmail(): void {
    this.showConfirmSendEmailDialog.set(false);
    this.emailToConfirm.set(null);
  }

  closeSaleSummary(): void {
    this.showSaleSummary.set(false);
    this.lastSale.set(null);
    this.lastCustomerEmail.set(null);
    this.receiptEmailSent.set(false);
  }

  downloadReceipt(): void {
    const sale = this.lastSale();
    if (!sale) return;

    this.viewDocument(sale.id, sale.documentType);
  }

  loadDocumentViewSettings(): void {
    this.documentSettingsService.getViewSettings().subscribe({
      next: (settings) => {
        this.documentViewSettings.set(settings);
      },
      error: (error) => {
        console.error('Error loading document view settings:', error);
        // Usar valores por defecto si hay error (incluyendo 404)
        this.documentViewSettings.set({
          defaultViewMode: 'preview',
          directPrint: false,
          boletaTemplateActive: true,
          facturaTemplateActive: true
        });
      }
    });
  }

  viewDocument(saleId: string, documentType: 'Boleta' | 'Factura'): void {
    const settings = this.documentViewSettings();
    
    // Validar que la plantilla esté activa
    if (documentType === 'Boleta' && settings && !settings.boletaTemplateActive) {
      this.toastService.error('La plantilla de Boleta no está disponible');
      return;
    }
    
    if (documentType === 'Factura' && settings && !settings.facturaTemplateActive) {
      this.toastService.error('La plantilla de Factura no está disponible');
      return;
    }

    const url = this.salesService.getPdfUrl(saleId, documentType);
    
    // Lógica condicional: Si está configurado para vista directa o impresión directa, abrir PDF directamente
    if (settings && (settings.defaultViewMode === 'direct' || settings.directPrint)) {
      // Abrir PDF directamente sin vista previa
      window.open(url, '_blank');
    } else {
      // Mantener comportamiento actual: abrir en nueva pestaña (puede mostrar vista previa del navegador)
      window.open(url, '_blank');
    }
  }

  getProductImageUrl(product: Product): string {
    return product.imageUrl || 'https://via.placeholder.com/100';
  }

  getFilteredCustomers(): Customer[] {
    const term = this.customerSearchTerm().toLowerCase().trim();
    if (!term) return this.customers().filter(c => c.isActive).slice(0, 10);
    
    // Limpiar el término de búsqueda (remover espacios y caracteres especiales para búsqueda de documento)
    const cleanTerm = term.replace(/\s+/g, '');
    
    return this.customers()
      .filter(c => c.isActive && (
        c.name.toLowerCase().includes(term) ||
        c.documentNumber.replace(/\s+/g, '').includes(cleanTerm) ||
        c.documentNumber.includes(term) ||
        (c.documentType && c.documentType.toLowerCase().includes(term))
      ))
      .slice(0, 10);
  }

  getFilteredCustomersWithRuc(): Customer[] {
    return this.getFilteredCustomers().filter(c => 
      c.documentType === 'RUC' && c.documentNumber.length === 11
    );
  }

  getFilteredCustomersWithDni(): Customer[] {
    return this.getFilteredCustomers().filter(c => 
      c.documentType === 'DNI' || !c.documentType || c.documentType !== 'RUC'
    );
  }

  // Shortcuts de teclado
  onKeyDown(event: KeyboardEvent): void {
    // ESC para cerrar búsqueda de cliente
    if (event.key === 'Escape' && this.showCustomerSearch()) {
      this.showCustomerSearch.set(false);
      this.customerSearchTerm.set('');
    }
    
    // Enter en búsqueda de productos para agregar el primero si hay resultados
    if (event.key === 'Enter' && event.target instanceof HTMLInputElement) {
      const input = event.target as HTMLInputElement;
      if (input.placeholder?.includes('Buscar por nombre')) {
        const firstProduct = this.filteredProducts()[0];
        if (firstProduct && this.filteredProducts().length === 1) {
          this.addToCart(firstProduct);
          this.searchTerm.set('');
          this.filteredProducts.set(this.products());
        }
      }
    }
  }
  
  setTodayDateRange(): void {
    const today = new Date();
    this.cashClosureStartDate.set(today.toISOString().split('T')[0]);
    this.cashClosureEndDate.set(today.toISOString().split('T')[0]);
    this.loadCashClosure();
  }
  
  loadCashClosure(): void {
    this.isLoadingCashClosure.set(true);
    
    // Obtener ventas pagadas del rango seleccionado
    const startDate = this.cashClosureStartDate() 
      ? new Date(this.cashClosureStartDate())
      : new Date();
    startDate.setHours(0, 0, 0, 0);
    
    const endDate = this.cashClosureEndDate()
      ? new Date(this.cashClosureEndDate())
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
  
  openCashClosureModal(): void {
    this.showCashClosureModal.set(true);
    // Recargar datos al abrir el modal
    this.loadCashClosure();
  }
  
  closeCashClosureModal(): void {
    this.showCashClosureModal.set(false);
  }
  
  getPaymentMethodText(method: string): string {
    const methods: { [key: string]: string } = {
      'Efectivo': 'Efectivo',
      'YapePlin': 'Yape/Plin',
      'Transferencia': 'Transferencia'
    };
    return methods[method] || method;
  }
  
  getCurrentDateFormatted(): string {
    return new Date().toLocaleDateString('es-PE', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
  }
  
  generateCashClosurePdf(): void {
    if (!this.cashClosureData() || this.cashClosureData()!.totalCount === 0) {
      this.toastService.warning('No hay ventas para generar el cierre de caja');
      return;
    }

    this.isGeneratingCashClosurePdf.set(true);

    const startDate = this.cashClosureStartDate() 
      ? new Date(this.cashClosureStartDate())
      : new Date();
    startDate.setHours(0, 0, 0, 0);
    
    const endDate = this.cashClosureEndDate()
      ? new Date(this.cashClosureEndDate())
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
      },
      error: (error) => {
        console.error('Error generando PDF de cierre de caja:', error);
        this.toastService.error('Error al generar el PDF de cierre de caja');
        this.isGeneratingCashClosurePdf.set(false);
      }
    });
  }
}


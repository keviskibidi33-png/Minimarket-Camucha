import { Component, OnInit, signal, computed, effect, afterNextRender, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductsService, Product } from '../../core/services/products.service';
import { CustomersService, Customer } from '../../core/services/customers.service';
import { SalesService, CartItem, CreateSaleDto, Sale } from '../../core/services/sales.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../shared/services/toast.service';
import { Router } from '@angular/router';
import { SendReceiptDialogComponent } from '../../shared/components/send-receipt-dialog/send-receipt-dialog.component';
import { PosHeaderComponent } from './pos-header/pos-header.component';

@Component({
  selector: 'app-pos',
  standalone: true,
  imports: [CommonModule, FormsModule, PosHeaderComponent, SendReceiptDialogComponent],
  templateUrl: './pos.component.html',
  styleUrl: './pos.component.css'
})
export class PosComponent implements OnInit {
  // Products
  products = signal<Product[]>([]);
  filteredProducts = signal<Product[]>([]);
  searchTerm = signal('');

  // Cart
  cartItems = signal<CartItem[]>([]);

  // Customer
  customers = signal<Customer[]>([]);
  selectedCustomer = signal<Customer | null>(null);
  customerSearchTerm = signal('');

  // Sale Configuration
  documentType = signal<'Boleta' | 'Factura'>('Boleta');
  paymentMethod = signal<'Efectivo' | 'Tarjeta' | 'YapePlin' | 'Transferencia'>('Efectivo');
  amountPaid = signal(0);
  discount = signal(0);

  // UI State
  isLoading = signal(false);
  showCustomerSearch = signal(false);
  showSendReceiptDialog = signal(false);
  lastSale = signal<Sale | null>(null);

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

  private readonly destroyRef = inject(DestroyRef);
  private paymentEffectCleanup?: ReturnType<typeof effect>;

  constructor(
    private productsService: ProductsService,
    private customersService: CustomersService,
    private salesService: SalesService,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {}

  // Helper methods for template
  parseFloat = parseFloat;

  ngOnInit(): void {
    this.loadProducts();
    this.loadCustomers();
    
    // Observar cambios en total para actualizar amountPaid automáticamente
    // Se ejecuta después del siguiente renderizado para asegurar que todo esté inicializado
    afterNextRender(() => {
      // Observar cambios en total para actualizar amountPaid automáticamente
      // Para métodos que no son efectivo, siempre debe ser igual al total
      this.paymentEffectCleanup = effect(() => {
        const total = this.total();
        const method = this.paymentMethod();
        
        if (method !== 'Efectivo') {
          this.amountPaid.set(total);
        } else if (this.amountPaid() < total) {
          this.amountPaid.set(total);
        }
      }, { allowSignalWrites: true });
    });

    // Limpiar el effect cuando el componente se destruya
    this.destroyRef.onDestroy(() => {
      this.paymentEffectCleanup?.destroy();
    });
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.productsService.getAll({ isActive: true }).subscribe({
      next: (result) => {
        const products = result.items || result;
        this.products.set(Array.isArray(products) ? products : []);
        this.filteredProducts.set(Array.isArray(products) ? products : []);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.toastService.error('Error al cargar productos');
        this.isLoading.set(false);
      }
    });
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

  onSearch(): void {
    const term = this.searchTerm().toLowerCase().trim();
    if (!term) {
      this.filteredProducts.set(this.products());
      return;
    }

    // Si el término es solo números, buscar por código exacto primero
    if (/^\d+$/.test(term)) {
      const exactMatch = this.products().find(p => p.code.toLowerCase() === term);
      if (exactMatch) {
        this.addToCart(exactMatch);
        this.searchTerm.set('');
        this.filteredProducts.set(this.products());
        return;
      }
    }

    const filtered = this.products().filter(p =>
      p.name.toLowerCase().includes(term) ||
      p.code.toLowerCase().includes(term)
    );
    this.filteredProducts.set(filtered);
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
  }

  removeFromCart(productId: string): void {
    this.cartItems.set(this.cartItems().filter(item => item.productId !== productId));
  }

  clearCart(): void {
    if (this.cartItems().length === 0) return;
    
    if (confirm('¿Está seguro de limpiar el carrito?')) {
      this.cartItems.set([]);
      this.amountPaid.set(0);
      this.discount.set(0);
      this.selectedCustomer.set(null);
      this.searchTerm.set('');
      this.filteredProducts.set(this.products());
    }
  }

  onDocumentTypeChange(): void {
    if (this.documentType() === 'Boleta') {
      this.selectedCustomer.set(null);
    } else {
      this.showCustomerSearch.set(true);
    }
  }

  selectCustomer(customer: Customer): void {
    this.selectedCustomer.set(customer);
    this.showCustomerSearch.set(false);
    this.customerSearchTerm.set('');
  }

  onPaymentMethodChange(): void {
    // Para todos los métodos, inicializar con el total
    // Para Efectivo, el usuario puede ajustar el monto
    // Para otros métodos, el monto debe ser igual al total
    this.amountPaid.set(this.total());
  }

  processSale(): void {
    if (this.cartItems().length === 0) {
      this.toastService.warning('El carrito está vacío');
      return;
    }

    if (this.documentType() === 'Factura' && !this.selectedCustomer()) {
      this.toastService.warning('Las facturas requieren un cliente');
      this.showCustomerSearch.set(true);
      return;
    }

    if (this.amountPaid() < this.total()) {
      this.toastService.warning('El monto pagado es menor al total');
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
        this.toastService.success(`Venta ${sale.documentNumber} creada exitosamente`);
        // Recargar productos para actualizar stock
        this.loadProducts();
        this.clearCart();
        this.isLoading.set(false);
        // Mostrar opciones para enviar comprobante
        this.showSendReceiptDialog.set(true);
      },
      error: (error) => {
        console.error('Error creating sale:', error);
        this.toastService.error(error.error?.errors?.[0] || 'Error al procesar la venta');
        this.isLoading.set(false);
      }
    });
  }

  onSendReceipt(event: { email: string }): void {
    const sale = this.lastSale();
    if (!sale) return;

    this.salesService.sendReceipt(sale.id, event.email, sale.documentType).subscribe({
      next: () => {
        this.toastService.success('Comprobante enviado exitosamente');
      },
      error: (error) => {
        console.error('Error sending receipt:', error);
        this.toastService.error(error.error?.errors?.[0] || 'Error al enviar el comprobante');
      }
    });
  }

  downloadReceipt(): void {
    const sale = this.lastSale();
    if (!sale) return;

    const url = this.salesService.getPdfUrl(sale.id, sale.documentType);
    window.open(url, '_blank');
  }

  getProductImageUrl(product: Product): string {
    return product.imageUrl || 'https://via.placeholder.com/100';
  }

  getFilteredCustomers(): Customer[] {
    const term = this.customerSearchTerm().toLowerCase().trim();
    if (!term) return this.customers().filter(c => c.isActive).slice(0, 5);
    return this.customers()
      .filter(c => c.isActive && (
        c.name.toLowerCase().includes(term) ||
        c.documentNumber.includes(term) ||
        (c.documentType && c.documentType.toLowerCase().includes(term))
      ))
      .slice(0, 5);
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
}


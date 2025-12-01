import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { SalesService, Sale } from '../../../core/services/sales.service';
import { ToastService } from '../../../shared/services/toast.service';
import { SendReceiptDialogComponent } from '../../../shared/components/send-receipt-dialog/send-receipt-dialog.component';
import { DocumentSettingsService, DocumentViewSettings } from '../../../core/services/document-settings.service';
import { ReceiptPrintComponent } from '../../../shared/components/receipt-print/receipt-print.component';
import { BrandSettingsService, BrandSettings } from '../../../core/services/brand-settings.service';

@Component({
  selector: 'app-sale-detail',
  standalone: true,
  imports: [CommonModule, SendReceiptDialogComponent, ReceiptPrintComponent],
  templateUrl: './sale-detail.component.html',
  styleUrl: './sale-detail.component.css'
})
export class SaleDetailComponent implements OnInit {
  sale = signal<Sale | null>(null);
  isLoading = signal(false);
  showSendReceiptDialog = signal(false);
  showConfirmSendEmailDialog = signal(false);
  emailToConfirm = signal<string | null>(null);
  documentViewSettings = signal<DocumentViewSettings | null>(null);
  brandSettings = signal<BrandSettings | null>(null);
  showReceiptPreview = signal(false);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private salesService: SalesService,
    private toastService: ToastService,
    private documentSettingsService: DocumentSettingsService,
    private brandSettingsService: BrandSettingsService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadSale(id);
    }
    // Cargar configuración de documentos
    this.loadDocumentViewSettings();
    // Cargar BrandSettings
    this.loadBrandSettings();
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

  loadSale(id: string): void {
    this.isLoading.set(true);
    this.salesService.getById(id).subscribe({
      next: (sale) => {
        this.sale.set(sale);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading sale:', error);
        this.toastService.error('Error al cargar la venta');
        this.isLoading.set(false);
        this.router.navigate(['/ventas']);
      }
    });
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
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  printReceipt(): void {
    // Si la vista previa HTML está visible, imprimir esa
    if (this.showReceiptPreview()) {
      window.print();
    } else {
      // Si no, descargar el PDF y abrirlo para imprimir
      this.downloadReceipt();
    }
  }

  downloadReceipt(): void {
    const sale = this.sale();
    if (!sale) return;

    const settings = this.documentViewSettings();
    
    // Validar que la plantilla esté activa
    if (sale.documentType === 'Boleta' && settings && !settings.boletaTemplateActive) {
      this.toastService.error('La plantilla de Boleta no está disponible');
      return;
    }
    
    if (sale.documentType === 'Factura' && settings && !settings.facturaTemplateActive) {
      this.toastService.error('La plantilla de Factura no está disponible');
      return;
    }

    const url = this.salesService.getPdfUrl(sale.id, sale.documentType);
    
    // Lógica condicional: Si está configurado para vista directa o impresión directa, abrir PDF directamente
    if (settings && (settings.defaultViewMode === 'direct' || settings.directPrint)) {
      // Abrir PDF directamente sin vista previa
      window.open(url, '_blank');
    } else {
      // Mantener comportamiento actual: abrir en nueva pestaña (puede mostrar vista previa del navegador)
      window.open(url, '_blank');
    }
  }

  isSendingEmail = signal(false);

  sendReceipt(): void {
    const sale = this.sale();
    if (!sale) {
      this.toastService.error('No se pudo cargar la información de la venta');
      return;
    }

    // Obtener el email del cliente de la venta
    const customerEmail = sale.customerEmail;
    
    // Si el cliente tiene email registrado, intentar enviar directamente con confirmación
    if (customerEmail && customerEmail.trim() !== '') {
      // Mostrar modal de confirmación, pero si falla, permitir reenvío directo
      try {
        this.emailToConfirm.set(customerEmail);
        this.showConfirmSendEmailDialog.set(true);
      } catch (error) {
        // Si el modal falla, enviar directamente como respaldo
        console.warn('Error al mostrar modal de confirmación, enviando directamente:', error);
        this.sendReceiptDirectly(customerEmail);
      }
    } else {
      // Si no hay email, mostrar el modal para ingresar el correo
      try {
        this.showSendReceiptDialog.set(true);
      } catch (error) {
        // Si el modal falla, mostrar mensaje y permitir reintento
        console.error('Error al mostrar modal de ingreso de correo:', error);
        this.toastService.error('No se pudo abrir el formulario. Por favor, recarga la página e intenta nuevamente.');
      }
    }
  }

  confirmSendEmail(): void {
    const email = this.emailToConfirm();
    if (!email) {
      this.toastService.error('No se encontró el correo electrónico');
      return;
    }
    this.sendReceiptDirectly(email);
  }

  sendReceiptDirectly(email: string): void {
    const sale = this.sale();
    if (!sale) {
      this.toastService.error('No se pudo cargar la información de la venta');
      return;
    }

    // Validar email
    if (!email || email.trim() === '') {
      this.toastService.error('El correo electrónico es requerido');
      return;
    }

    // Cerrar modales si están abiertos
    this.showConfirmSendEmailDialog.set(false);
    this.showSendReceiptDialog.set(false);
    this.emailToConfirm.set(null);

    // Mostrar indicador de carga
    this.isSendingEmail.set(true);

    this.salesService.sendReceipt(sale.id, email.trim(), sale.documentType).subscribe({
      next: () => {
        this.isSendingEmail.set(false);
        this.toastService.success(`Comprobante enviado exitosamente a ${email}`);
      },
      error: (error) => {
        console.error('Error sending receipt:', error);
        this.isSendingEmail.set(false);
        
        // Mensaje de error más descriptivo
        const errorMessage = error.error?.errors?.[0] || 
                            error.error?.message || 
                            'Error al enviar el comprobante. Por favor, verifica la conexión e intenta nuevamente.';
        
        this.toastService.error(errorMessage);
        
        // Si falla, ofrecer opción de reintentar o ingresar otro correo
        // No abrir modal automáticamente para evitar loops, pero permitir reintento manual
        console.log('El usuario puede hacer clic en "Enviar" nuevamente para reintentar');
      }
    });
  }

  cancelConfirmSendEmail(): void {
    this.showConfirmSendEmailDialog.set(false);
    this.emailToConfirm.set(null);
  }

  onSendReceipt(event: { email: string }): void {
    // Usar el método directo para mantener consistencia
    this.sendReceiptDirectly(event.email);
  }

  goBack(): void {
    this.router.navigate(['/ventas']);
  }

  loadBrandSettings(): void {
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        this.brandSettings.set(settings);
      },
      error: (error) => {
        console.error('Error loading brand settings:', error);
        // Usar valores por defecto
        this.brandSettings.set({
          id: '',
          logoUrl: '',
          storeName: 'Minimarket Camucha',
          primaryColor: '#4CAF50',
          secondaryColor: '#81C784',
          buttonColor: '#4CAF50',
          textColor: '#333333',
          hoverColor: '#45A049',
          ruc: '10095190559',
          address: 'Jr. Pedro Labarthe 449 – Ingeniería, San Martín de Porres, Lima, Lima, Perú',
          phone: '+51 999 999 999',
          email: '',
          yapeEnabled: false,
          plinEnabled: false,
          bankAccountVisible: false,
          deliveryType: 'none',
          createdAt: new Date().toISOString()
        });
      }
    });
  }

  toggleReceiptPreview(): void {
    this.showReceiptPreview.set(!this.showReceiptPreview());
  }
}


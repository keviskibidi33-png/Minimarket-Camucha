import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { SalesService, Sale } from '../../../core/services/sales.service';
import { ToastService } from '../../../shared/services/toast.service';
import { SendReceiptDialogComponent } from '../../../shared/components/send-receipt-dialog/send-receipt-dialog.component';
import { DocumentSettingsService, DocumentViewSettings } from '../../../core/services/document-settings.service';

@Component({
  selector: 'app-sale-detail',
  standalone: true,
  imports: [CommonModule, SendReceiptDialogComponent],
  templateUrl: './sale-detail.component.html',
  styleUrl: './sale-detail.component.css'
})
export class SaleDetailComponent implements OnInit {
  sale = signal<Sale | null>(null);
  isLoading = signal(false);
  showSendReceiptDialog = signal(false);
  documentViewSettings = signal<DocumentViewSettings | null>(null);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private salesService: SalesService,
    private toastService: ToastService,
    private documentSettingsService: DocumentSettingsService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadSale(id);
    }
    // Cargar configuración de documentos
    this.loadDocumentViewSettings();
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
    window.print();
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

  sendReceipt(): void {
    this.showSendReceiptDialog.set(true);
  }

  onSendReceipt(event: { email: string }): void {
    const sale = this.sale();
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

  goBack(): void {
    this.router.navigate(['/ventas']);
  }
}


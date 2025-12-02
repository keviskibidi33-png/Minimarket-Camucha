import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Sale, SaleDetail } from '../../../core/services/sales.service';

@Component({
  selector: 'app-receipt-print',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './receipt-print.component.html',
  styleUrl: './receipt-print.component.css'
})
export class ReceiptPrintComponent {
  @Input() sale!: Sale;
  @Input() brandSettings!: {
    logoUrl?: string;
    storeName: string;
    ruc?: string;
    address?: string;
    phone?: string;
    email?: string;
    primaryColor: string;
  };

  get documentType(): 'Boleta' | 'Factura' {
    return this.sale?.documentType || 'Boleta';
  }

  get documentNumber(): string {
    return this.sale?.documentNumber || '';
  }

  get saleDate(): string {
    return this.sale?.saleDate || '';
  }

  get customerName(): string | undefined {
    return this.sale?.customerName;
  }

  get customerDocumentType(): string | undefined {
    return this.sale?.customerDocumentType;
  }

  get customerDocumentNumber(): string | undefined {
    return this.sale?.customerDocumentNumber;
  }

  get customerAddress(): string | undefined {
    return this.sale?.customerAddress;
  }

  get customerEmail(): string | undefined {
    return this.sale?.customerEmail;
  }

  get saleDetails(): SaleDetail[] {
    return this.sale?.saleDetails || [];
  }

  get subtotal(): number {
    return this.sale?.subtotal || 0;
  }

  get discount(): number {
    return this.sale?.discount || 0;
  }

  get tax(): number {
    return this.sale?.tax || 0;
  }

  get total(): number {
    return this.sale?.total || 0;
  }

  get paymentMethod(): string {
    return this.sale?.paymentMethod || 'Efectivo';
  }

  get amountPaid(): number {
    return this.sale?.amountPaid || 0;
  }

  get change(): number {
    return this.sale?.change || 0;
  }

  get logoUrl(): string {
    return 'assets/logo.png'; // Siempre usar el logo de assets
  }

  get companyName(): string {
    return this.brandSettings?.storeName || 'Minimarket Camucha';
  }

  get companyRuc(): string | undefined {
    return this.brandSettings?.ruc;
  }

  get companyAddress(): string | undefined {
    return this.brandSettings?.address;
  }

  get companyPhone(): string | undefined {
    return this.brandSettings?.phone;
  }

  get companyEmail(): string | undefined {
    return this.brandSettings?.email;
  }

  get primaryColor(): string {
    return this.brandSettings?.primaryColor || '#4CAF50';
  }

  formatDate(date: string): string {
    if (!date) return '';
    const d = new Date(date);
    return d.toLocaleDateString('es-PE', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
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
}


import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface QRPaymentData {
  amount: number;
  phoneNumber: string;
  qrImageUrl?: string;
  reference: string;
  paymentType: 'Yape' | 'Plin';
}

@Component({
  selector: 'app-qr-payment-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './qr-payment-modal.component.html',
  styleUrl: './qr-payment-modal.component.css'
})
export class QRPaymentModalComponent {
  @Input() isOpen: boolean = false;
  @Input() paymentData: QRPaymentData | null = null;
  @Output() onConfirm = new EventEmitter<void>();
  @Output() onCancel = new EventEmitter<void>();

  // Getters para formatear el monto
  get formattedAmount(): string {
    if (!this.paymentData) return 'S/ 0.00';
    return `S/ ${this.paymentData.amount.toFixed(2)}`;
  }

  // Getter para formatear el teléfono
  get formattedPhone(): string {
    if (!this.paymentData) return '';
    // Formatear teléfono: 999888777 -> 999 888 777
    const phone = this.paymentData.phoneNumber.replace(/\D/g, '');
    if (phone.length === 9) {
      return `${phone.slice(0, 3)} ${phone.slice(3, 6)} ${phone.slice(6)}`;
    }
    return this.paymentData.phoneNumber;
  }

  copyPhoneToClipboard(): void {
    if (!this.paymentData) return;

    const phone = this.paymentData.phoneNumber.replace(/\D/g, '');
    navigator.clipboard.writeText(phone).then(() => {
      // Mostrar feedback visual (se puede mejorar con un toast)
      const button = document.querySelector('[data-cy="copy-phone-button"]') as HTMLElement;
      if (button) {
        const originalText = button.textContent;
        button.textContent = '¡Copiado!';
        button.classList.add('bg-green-500');
        setTimeout(() => {
          button.textContent = originalText;
          button.classList.remove('bg-green-500');
        }, 2000);
      }
    }).catch(err => {
      console.error('Error al copiar:', err);
    });
  }

  openQRInMobile(): void {
    if (!this.paymentData || !this.paymentData.qrImageUrl) return;

    // Abrir QR en una nueva ventana para que el cajero pueda mostrarlo en su celular
    // O generar un link que se pueda escanear desde el celular
    const qrWindow = window.open(this.paymentData.qrImageUrl, '_blank', 'width=400,height=400');
    if (!qrWindow) {
      // Si no se puede abrir ventana, intentar descargar la imagen
      const link = document.createElement('a');
      link.href = this.paymentData.qrImageUrl;
      link.download = `qr-${this.paymentData.paymentType.toLowerCase()}.png`;
      link.click();
    }
  }

  handleConfirm(): void {
    this.onConfirm.emit();
  }

  handleCancel(): void {
    this.onCancel.emit();
  }

  // Cerrar modal al hacer clic fuera
  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.handleCancel();
    }
  }
}


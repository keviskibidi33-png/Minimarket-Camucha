import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { StoreHeaderComponent } from '../../../../shared/components/store-header/store-header.component';
import { CartService } from '../../../../core/services/cart.service';

@Component({
  selector: 'app-success',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    StoreHeaderComponent
  ],
  templateUrl: './success.component.html',
  styleUrl: './success.component.css'
})
export class SuccessComponent implements OnInit {
  orderNumber = signal('');
  paymentData: any = null;
  shippingData: any = null;
  total = signal(0);
  subtotal = signal(0);
  shippingCost = signal(0);
  requiresProof = signal(false);
  orderItems: any[] = [];
  isReceiving = signal(true); // Estado de recepción
  showContent = signal(false); // Controla la aparición del contenido
  isCopied = signal(false); // Estado para el botón de copiar
  
  // Bank account info
  bankAccountNumber = '193-12345678-0-00';
  bankAccountName = 'Minimarket Camucha S.A.C.';
  
  // Digital wallet info (solo Yape)
  yapeNumber = '999 888 777'; // TODO: Mover a configuración
  yapeQR = signal<string>('');

  constructor(
    private router: Router,
    private cartService: CartService
  ) {}

  ngOnInit() {
    // Cargar datos del pedido desde localStorage
    const orderNum = localStorage.getItem('current-order-number');
    const payment = localStorage.getItem('checkout-payment');
    const shipping = localStorage.getItem('checkout-shipping');
    const totalStr = localStorage.getItem('checkout-total');
    const orderItemsStr = localStorage.getItem('checkout-items');
    
    if (!orderNum || !payment) {
      // Si no hay datos, redirigir al carrito
      this.router.navigate(['/carrito']);
      return;
    }
    
    try {
      this.orderNumber.set(orderNum);
      this.paymentData = JSON.parse(payment);
      this.shippingData = shipping ? JSON.parse(shipping) : null;
      this.total.set(totalStr ? parseFloat(totalStr) : 0);
      this.shippingCost.set(this.shippingData?.shippingCost || 0);
      this.requiresProof.set(this.paymentData?.requiresProof === true);
      
      // Cargar items del pedido
      if (orderItemsStr) {
        this.orderItems = JSON.parse(orderItemsStr);
        this.subtotal.set(this.orderItems.reduce((sum, item) => sum + (item.subtotal || 0), 0));
      }
      
      // Si requiere comprobante y es billetera digital, generar QR de Yape
      if (this.requiresProof() && this.paymentData?.paymentMethod === 'wallet') {
        this.generateYapeQR();
      }
      
      // Limpiar datos de checkout DESPUÉS de cargarlos (solo se necesitan para mostrar)
      // Esto permite que los datos persistan hasta aquí pero se limpien para el próximo pedido
      // También limpiar el flag de confirmación
      setTimeout(() => {
        localStorage.removeItem('checkout-shipping');
        localStorage.removeItem('checkout-payment');
        localStorage.removeItem('checkout-total');
        localStorage.removeItem('checkout-items');
        localStorage.removeItem('checkout-subtotal');
        localStorage.removeItem('order-confirming');
      }, 100); // Pequeño delay para asegurar que los datos se cargaron
      
      // Simular recepción del pago (3-4 segundos)
      setTimeout(() => {
        this.isReceiving.set(false);
        // Pequeño delay antes de mostrar el contenido para la animación del check
        setTimeout(() => {
          this.showContent.set(true);
        }, 300);
      }, 3500); // 3.5 segundos
    } catch (error) {
      console.error('Error loading order data:', error);
      this.router.navigate(['/carrito']);
    }
  }

  generateYapeQR() {
    // Generar QR dinámico de Yape con el monto
    const amount = this.total();
    const qrData = `yape://payment?phone=${this.yapeNumber.replace(/\s/g, '')}&amount=${amount.toFixed(2)}`;
    const qrUrl = `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(qrData)}`;
    this.yapeQR.set(qrUrl);
  }

  openWhatsApp() {
    const adminWhatsAppNumber = '51987654321'; // TODO: Mover a configuración
    
    // Mensaje natural y profesional para confirmar el pago
    const message = `Hola, confirmo el pago de mi pedido.\n\n` +
      `📦 Número de Pedido: ${this.orderNumber()}\n\n` +
      `Adjunto el comprobante de pago para su verificación.\n\n` +
      `Gracias.`;
    
    const whatsappUrl = `https://wa.me/${adminWhatsAppNumber}?text=${encodeURIComponent(message)}`;
    window.open(whatsappUrl, '_blank');
  }

  copyToClipboard(text: string) {
    navigator.clipboard.writeText(text).then(() => {
      // Cambiar estado del botón a "Copiado"
      this.isCopied.set(true);
      
      // Volver al estado normal después de 2 segundos
      setTimeout(() => {
        this.isCopied.set(false);
      }, 2000);
    }).catch(() => {
      // Si falla, mostrar un mensaje breve
      this.isCopied.set(true);
      setTimeout(() => {
        this.isCopied.set(false);
      }, 2000);
    });
  }

  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }

  getWalletName(): string {
    return 'Yape'; // Siempre Yape
  }

  isStripePayment(): boolean {
    return this.paymentData?.paymentMethod === 'card' && this.paymentData?.paymentIntentId;
  }

  // No limpiar datos aquí - se mantendrán hasta que se complete la venta o se limpie el carrito
  // Los datos se limpiarán en confirmation.component.ts cuando se confirme el pedido
}


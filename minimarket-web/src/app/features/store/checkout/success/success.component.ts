import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { StoreHeaderComponent } from '../../../../shared/components/store-header/store-header.component';
import { CartService } from '../../../../core/services/cart.service';
import { SettingsService } from '../../../../core/services/settings.service';
import { BrandSettingsService } from '../../../../core/services/brand-settings.service';
import { firstValueFrom } from 'rxjs';

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

  // Shipping settings
  deliveryDays = signal(3);
  deliveryTime = signal('18:00');
  pickupDays = signal(2);
  pickupTime = signal('16:00');

  constructor(
    private router: Router,
    private cartService: CartService,
    private settingsService: SettingsService,
    private brandSettingsService: BrandSettingsService
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

      // Cargar configuraciones de envío desde el backend
      this.loadShippingSettings();
      
      // Enviar WhatsApp automáticamente al cliente
      this.sendWhatsAppToCustomer();
      
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

  async sendWhatsAppToCustomer(): Promise<void> {
    try {
      // Obtener número de WhatsApp del negocio desde BrandSettings
      const settings = await firstValueFrom(this.brandSettingsService.get());
      const businessWhatsApp = settings?.whatsAppPhone || settings?.phone;
      if (!businessWhatsApp) {
        console.warn('No hay número de WhatsApp configurado en el admin');
        return;
      }

      // Obtener teléfono del cliente desde shippingData
      const customerPhone = this.shippingData?.phone;
      if (!customerPhone) {
        console.warn('No hay teléfono del cliente');
        return;
      }

      // Limpiar los números de teléfono (remover espacios, guiones, etc.)
      const cleanCustomerPhone = customerPhone.replace(/\s+/g, '').replace(/-/g, '').replace(/\+/g, '');
      
      // Crear mensaje de confirmación para el cliente
      const shippingMethod = this.shippingData?.shippingMethod === 'delivery' ? 'Delivery' : 'Recojo en Tienda';
      const estimatedDate = this.getEstimatedDeliveryDate();
      
      const message = `¡Hola! Tu pedido ha sido confirmado.\n\n` +
        `📦 Número de Pedido: ${this.orderNumber()}\n` +
        `💰 Total: S/ ${this.getPriceFormatted(this.total())}\n` +
        `🚚 Método: ${shippingMethod}\n` +
        `📅 Fecha Estimada: ${estimatedDate}\n\n` +
        `Te notificaremos cuando tu pedido esté listo. ¡Gracias por tu compra!`;

      // Abrir WhatsApp Web con el mensaje prellenado
      // El formato es: wa.me/[número_destino]?text=[mensaje]
      // Esto abrirá WhatsApp Web para que el admin pueda enviar el mensaje al cliente
      // Nota: Para envío automático se necesitaría WhatsApp Business API
      const whatsappUrl = `https://wa.me/${cleanCustomerPhone}?text=${encodeURIComponent(message)}`;
      
      // Abrir en nueva ventana después de un pequeño delay para que la página cargue
      setTimeout(() => {
        window.open(whatsappUrl, '_blank');
      }, 1000);
    } catch (error) {
      console.error('Error preparando WhatsApp para el cliente:', error);
    }
  }

  openWhatsApp() {
    // Obtener número de WhatsApp de BrandSettings
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        const adminWhatsAppNumber = settings?.whatsAppPhone || settings?.phone || '51987654321';
        // Limpiar el número (remover espacios, guiones, etc.)
        const cleanNumber = adminWhatsAppNumber.replace(/\s+/g, '').replace(/-/g, '').replace(/\+/g, '');
        
        // Mensaje natural y profesional para confirmar el pago
        const message = `Hola, confirmo el pago de mi pedido.\n\n` +
          `📦 Número de Pedido: ${this.orderNumber()}\n\n` +
          `Adjunto el comprobante de pago para su verificación.\n\n` +
          `Gracias.`;
        
        const whatsappUrl = `https://wa.me/${cleanNumber}?text=${encodeURIComponent(message)}`;
        window.open(whatsappUrl, '_blank');
      },
      error: (error) => {
        console.error('Error obteniendo número de WhatsApp:', error);
        // Fallback a número por defecto
        const whatsappUrl = `https://wa.me/51987654321?text=${encodeURIComponent('Hola, confirmo el pago de mi pedido ' + this.orderNumber())}`;
        window.open(whatsappUrl, '_blank');
      }
    });
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

  async loadShippingSettings(): Promise<void> {
    try {
      // Cargar configuraciones de envío desde el backend
      const deliveryDaysValue = await firstValueFrom(
        this.settingsService.getSettingValue('delivery_days', '3')
      );
      this.deliveryDays.set(parseInt(deliveryDaysValue) || 3);

      const deliveryTimeValue = await firstValueFrom(
        this.settingsService.getSettingValue('delivery_time', '18:00')
      );
      this.deliveryTime.set(deliveryTimeValue || '18:00');

      const pickupDaysValue = await firstValueFrom(
        this.settingsService.getSettingValue('pickup_days', '2')
      );
      this.pickupDays.set(parseInt(pickupDaysValue) || 2);

      const pickupTimeValue = await firstValueFrom(
        this.settingsService.getSettingValue('pickup_time', '16:00')
      );
      this.pickupTime.set(pickupTimeValue || '16:00');
    } catch (error) {
      console.error('Error loading shipping settings:', error);
      // Usar valores por defecto si hay error
    }
  }

  // Métodos para el estado del pedido
  getEstimatedDeliveryDate(): string {
    const today = new Date();
    let daysToAdd = 0;
    let time = '';
    
    if (this.shippingData?.shippingMethod === 'delivery') {
      daysToAdd = this.deliveryDays();
      time = this.deliveryTime();
    } else if (this.shippingData?.shippingMethod === 'pickup') {
      daysToAdd = this.pickupDays();
      time = this.pickupTime();
    } else {
      daysToAdd = 3;
      time = '18:00';
    }
    
    const deliveryDate = new Date(today);
    deliveryDate.setDate(today.getDate() + daysToAdd);
    
    // Formatear fecha en español
    const months = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 
                    'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];
    const day = deliveryDate.getDate();
    const month = months[deliveryDate.getMonth()];
    const year = deliveryDate.getFullYear();
    
    // Formatear hora (convertir de 24h a 12h)
    const [hours, minutes] = time.split(':');
    let hour12 = parseInt(hours);
    const ampm = hour12 >= 12 ? 'PM' : 'AM';
    
    if (hour12 === 0) {
      hour12 = 12; // 00:00 -> 12:00 AM
    } else if (hour12 > 12) {
      hour12 = hour12 - 12; // 13:00 -> 1:00 PM
    }
    // Si es 12:00, se mantiene como 12:00 PM
    
    const formattedTime = `${hour12}:${minutes} ${ampm}`;
    
    return `${day} de ${month}, ${year} a las ${formattedTime}`;
  }

  getOrderStatus(): { icon: string; message: string; color: string } {
    // Estado inicial: preparando pedido
    return {
      icon: 'local_shipping',
      message: 'Preparando tu pedido',
      color: 'text-primary'
    };
  }

  getShippingMethodName(): string {
    if (this.shippingData?.shippingMethod === 'delivery') {
      return 'Despacho a Domicilio';
    } else if (this.shippingData?.shippingMethod === 'pickup') {
      return 'Retiro en Tienda';
    }
    return 'Envío';
  }

  getNotificationMethod(): string {
    // Usar correo electrónico si está disponible, sino mencionar WhatsApp
    if (this.shippingData?.email) {
      return 'correo electrónico';
    }
    return 'WhatsApp';
  }

  // No limpiar datos aquí - se mantendrán hasta que se complete la venta o se limpie el carrito
  // Los datos se limpiarán en confirmation.component.ts cuando se confirme el pedido
}


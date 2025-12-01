import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { StoreHeaderComponent } from '../../../../shared/components/store-header/store-header.component';
import { CartService } from '../../../../core/services/cart.service';
import { SettingsService } from '../../../../core/services/settings.service';
import { BrandSettingsService } from '../../../../core/services/brand-settings.service';
import { PaymentsService } from '../../../../core/services/payments.service';
import { ToastService } from '../../../../shared/services/toast.service';
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
  
  // Digital wallet info (Yape/Plin) - se carga desde BrandSettings
  yapeNumber = signal<string>('999 888 777'); // Fallback si no hay configuración
  yapeQR = signal<string>('');

  // Shipping settings
  deliveryDays = signal(3);
  deliveryTime = signal('18:00');
  pickupDays = signal(2);
  pickupTime = signal('16:00');

  // Payment proof upload
  paymentProofFile = signal<File | null>(null);
  paymentProofPreview = signal<string | null>(null);
  paymentProofUploading = signal(false);
  operationCode = signal<string>('');
  proofUploaded = signal(false);
  showContactModal = signal(false);

  constructor(
    private router: Router,
    private cartService: CartService,
    private settingsService: SettingsService,
    private brandSettingsService: BrandSettingsService,
    private paymentsService: PaymentsService,
    private toastService: ToastService
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
      
      // Cargar configuración de Yape/Plin desde BrandSettings
      if (this.requiresProof() && this.paymentData?.paymentMethod === 'wallet') {
        this.loadYapeSettings();
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

  loadYapeSettings(): void {
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        if (settings) {
          // Cargar número de Yape/Plin desde BrandSettings (unificado)
          // Priorizar yapePhone, pero usar plinPhone como fallback
          const phoneNumber = settings.yapePhone || settings.plinPhone;
          if (phoneNumber) {
            this.yapeNumber.set(phoneNumber);
          }
          
          // Cargar QR de Yape/Plin desde BrandSettings (unificado)
          // Priorizar yapeQRUrl, pero usar plinQRUrl como fallback
          const qrUrl = settings.yapeQRUrl || settings.plinQRUrl;
          if (qrUrl) {
            this.yapeQR.set(qrUrl);
          } else {
            // Si no hay QR en BrandSettings, generar uno dinámico como fallback
            this.generateYapeQR();
          }
        } else {
          // Si no hay settings, generar QR dinámico como fallback
          this.generateYapeQR();
        }
      },
      error: (error) => {
        console.error('Error loading Yape settings:', error);
        // En caso de error, generar QR dinámico como fallback
        this.generateYapeQR();
      }
    });
  }

  generateYapeQR() {
    // Generar QR dinámico de Yape con el monto (solo si no hay QR en BrandSettings)
    const amount = this.total();
    const phoneNumber = this.yapeNumber().replace(/\s/g, '');
    const qrData = `yape://payment?phone=${phoneNumber}&amount=${amount.toFixed(2)}`;
    const qrUrl = `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(qrData)}`;
    // Solo establecer si no hay QR ya cargado desde BrandSettings
    if (!this.yapeQR()) {
      this.yapeQR.set(qrUrl);
    }
  }

  async sendWhatsAppToCustomer(): Promise<void> {
    try {
      // Obtener número de WhatsApp del negocio desde BrandSettings
      const settings = await firstValueFrom(this.brandSettingsService.get());
      const businessWhatsApp = settings?.whatsAppPhone || settings?.phone;
      if (!businessWhatsApp) {
        // No es crítico, solo un warning
        return;
      }

      // Obtener teléfono del cliente desde shippingData
      const customerPhone = this.shippingData?.phone;
      if (!customerPhone) {
        // No es crítico si no hay teléfono, solo no se envía WhatsApp automático
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
    return 'Yape/Plin'; // Unificado
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

  // Payment proof upload methods
  onPaymentProofSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validar tamaño (5MB máximo)
      if (file.size > 5 * 1024 * 1024) {
        this.toastService.error('El archivo es demasiado grande. Máximo 5MB');
        return;
      }
      
      this.paymentProofFile.set(file);
      
      // Crear preview si es imagen (para PDFs no hay preview, pero se muestra el ícono)
      if (file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = (e) => {
          this.paymentProofPreview.set(e.target?.result as string);
        };
        reader.readAsDataURL(file);
      } else if (file.type === 'application/pdf') {
        // Para PDFs, establecer un indicador especial
        this.paymentProofPreview.set('pdf');
      } else {
        this.paymentProofPreview.set('document');
      }
    }
  }

  removePaymentProof() {
    this.paymentProofFile.set(null);
    this.paymentProofPreview.set(null);
    this.operationCode.set('');
  }

  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    img.style.display = 'none';
    // Mostrar un placeholder si la imagen falla
    console.warn('Error al cargar la imagen del comprobante');
  }

  getFileTypeLabel(fileType: string): string {
    if (fileType.startsWith('image/')) {
      if (fileType.includes('jpeg') || fileType.includes('jpg')) return 'JPG';
      if (fileType.includes('png')) return 'PNG';
      if (fileType.includes('gif')) return 'GIF';
      return 'Imagen';
    }
    if (fileType === 'application/pdf') return 'PDF';
    if (fileType.includes('document') || fileType.includes('word')) return 'Documento';
    return 'Archivo';
  }

  async uploadPaymentProof() {
    if (!this.paymentProofFile()) {
      this.toastService.error('Por favor selecciona un archivo');
      return;
    }

    if (!this.orderNumber()) {
      this.toastService.error('No hay un pedido creado');
      return;
    }

    this.paymentProofUploading.set(true);

    try {
      // Convertir archivo a base64
      const fileData = await new Promise<string>((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = (e) => resolve(e.target?.result as string);
        reader.onerror = reject;
        reader.readAsDataURL(this.paymentProofFile()!);
      });

      // Preparar datos para enviar
      const proofData = {
        orderNumber: this.orderNumber(),
        email: this.shippingData?.email || '',
        phone: this.shippingData?.phone || '',
        customerName: `${this.shippingData?.firstName || ''} ${this.shippingData?.lastName || ''}`.trim(),
        total: this.total(),
        paymentMethod: this.paymentData.paymentMethod,
        walletMethod: this.paymentData.walletMethod || null,
        bankAccount: this.paymentData.bankAccountNumber || null,
        fileName: this.paymentProofFile()?.name || '',
        fileData: fileData,
        fileType: this.paymentProofFile()?.type || 'image/jpeg',
        operationCode: this.operationCode() || null
      };

      // Enviar comprobante al backend
      await firstValueFrom(
        this.paymentsService.sendPaymentProof(proofData)
      );

      this.toastService.success('Comprobante enviado exitosamente. Te notificaremos cuando sea verificado.');
      
      // Marcar como subido
      this.proofUploaded.set(true);
      
      // Limpiar formulario
      this.paymentProofFile.set(null);
      this.paymentProofPreview.set(null);
      this.operationCode.set('');
      
      // Mostrar modal de contacto
      this.showContactModal.set(true);
      
    } catch (error: any) {
      console.error('Error uploading payment proof:', error);
      this.toastService.error('Error al subir el comprobante. Por favor intenta de nuevo.');
    } finally {
      this.paymentProofUploading.set(false);
    }
  }

  closeContactModal() {
    this.showContactModal.set(false);
  }

  // No limpiar datos aquí - se mantendrán hasta que se complete la venta o se limpie el carrito
  // Los datos se limpiarán en confirmation.component.ts cuando se confirme el pedido
}


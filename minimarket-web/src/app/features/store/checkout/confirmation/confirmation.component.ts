import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../../core/services/cart.service';
import { PaymentsService } from '../../../../core/services/payments.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { CheckoutStepperComponent } from '../../../../shared/components/checkout-stepper/checkout-stepper.component';
import { StoreHeaderComponent } from '../../../../shared/components/store-header/store-header.component';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-confirmation',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule,
    FormsModule,
    CheckoutStepperComponent,
    StoreHeaderComponent
  ],
  templateUrl: './confirmation.component.html',
  styleUrl: './confirmation.component.css'
})
export class ConfirmationComponent implements OnInit {
  cartItems!: ReturnType<typeof CartService.prototype.getCartItems>;
  shippingData: any = null;
  paymentData: any = null;
  subtotal = computed(() => this.cartItems().reduce((sum: number, item: any) => sum + item.subtotal, 0));
  shippingCost = signal(0);
  total = computed(() => this.subtotal() + this.shippingCost());
  orderNumber = signal('');
  orderCreated = signal(false);
  
  // Payment proof upload
  paymentProofFile = signal<File | null>(null);
  paymentProofPreview = signal<string | null>(null);
  paymentProofUploading = signal(false);
  operationCode = signal(''); // Código de operación opcional

  constructor(
    private cartService: CartService,
    private paymentsService: PaymentsService,
    private toastService: ToastService,
    private router: Router
  ) {
    // Inicializar cartItems después del constructor
    this.cartItems = this.cartService.getCartItems();
  }

  ngOnInit() {
    // Si se está confirmando un pedido, no validar (evitar redirección durante la confirmación)
    const isConfirming = localStorage.getItem('order-confirming') === 'true';
    
    // Cargar datos de envío y pago desde localStorage (persistencia después de recargar)
    // Esto protege contra pérdida de datos por errores de red o recargas accidentales
    const shipping = this.loadShippingDataFromStorage();
    const payment = this.loadPaymentDataFromStorage();
    
    // Verificar que el carrito no esté vacío y que haya datos de checkout
    // PERO solo si NO se está confirmando un pedido (para evitar redirección durante confirmación)
    if (!isConfirming && (this.cartItems().length === 0 || !shipping || !payment)) {
      this.router.navigate(['/carrito']);
      return;
    }
    
    // Generar número de orden
    this.orderNumber.set('ORD-' + Date.now().toString().slice(-8));
  }

  // Cargar datos de envío desde localStorage con manejo de errores
  private loadShippingDataFromStorage(): boolean {
    try {
      const shipping = localStorage.getItem('checkout-shipping');
      if (shipping) {
        const shippingData = JSON.parse(shipping);
        if (shippingData && typeof shippingData === 'object') {
          this.shippingData = shippingData;
          this.shippingCost.set(shippingData.shippingCost || 0);
          return true;
        }
      }
    } catch (error) {
      console.error('Error loading shipping data from localStorage:', error);
      try {
        localStorage.removeItem('checkout-shipping');
      } catch (e) {
        console.error('Error clearing corrupted shipping data:', e);
      }
    }
    return false;
  }

  // Cargar datos de pago desde localStorage con manejo de errores
  private loadPaymentDataFromStorage(): boolean {
    try {
      const payment = localStorage.getItem('checkout-payment');
      if (payment) {
        const paymentData = JSON.parse(payment);
        if (paymentData && typeof paymentData === 'object') {
          this.paymentData = paymentData;
          return true;
        }
      }
    } catch (error) {
      console.error('Error loading payment data from localStorage:', error);
      try {
        localStorage.removeItem('checkout-payment');
      } catch (e) {
        console.error('Error clearing corrupted payment data:', e);
      }
    }
    return false;
  }

  async confirmOrder() {
    // Validar que haya datos necesarios antes de confirmar
    if (!this.shippingData || !this.paymentData || this.cartItems().length === 0) {
      this.toastService.error('Faltan datos necesarios para confirmar el pedido');
      this.router.navigate(['/carrito']);
      return;
    }

    try {
      // TODO: Enviar orden al backend y obtener número de pedido real
      // Por ahora, generar número temporal
      const orderNum = 'ORD-' + Date.now().toString().slice(-8);
      
      // Guardar TODOS los datos necesarios para la página de éxito ANTES de limpiar cualquier cosa
      const orderItems = this.cartItems();
      const orderTotal = this.total();
      const orderSubtotal = this.subtotal();
      const orderShippingCost = this.shippingCost();
      
      // Guardar todos los datos necesarios
      localStorage.setItem('current-order-number', orderNum);
      localStorage.setItem('checkout-total', orderTotal.toString());
      localStorage.setItem('checkout-subtotal', orderSubtotal.toString());
      localStorage.setItem('checkout-items', JSON.stringify(orderItems));
      // Guardar shipping y payment para la página de éxito (se limpiarán después)
      localStorage.setItem('checkout-shipping', JSON.stringify(this.shippingData));
      localStorage.setItem('checkout-payment', JSON.stringify(this.paymentData));
      
      // Marcar que el pedido está siendo confirmado (para evitar validaciones)
      localStorage.setItem('order-confirming', 'true');
      
      // Limpiar carrito DESPUÉS de guardar todos los datos
      // NO limpiar datos de checkout aquí - se necesitan para la página de éxito
      this.cartService.clearCart(false);
      
      // Navegar a página de éxito INMEDIATAMENTE después de guardar datos
      // Los datos de checkout se limpiarán en la página de éxito después de mostrarlos
      this.router.navigate(['/checkout/exito']).then(() => {
        // Limpiar el flag después de navegar
        localStorage.removeItem('order-confirming');
      }).catch((error) => {
        console.error('Navigation error:', error);
        localStorage.removeItem('order-confirming');
        this.toastService.error('Error al navegar. Por favor intenta de nuevo.');
      });
      
    } catch (error) {
      console.error('Error confirming order:', error);
      localStorage.removeItem('order-confirming');
      this.toastService.error('Error al confirmar el pedido. Por favor intenta de nuevo.');
    }
  }

  clearCheckoutData() {
    // Limpiar todos los datos de checkout después de confirmar el pedido
    localStorage.removeItem('checkout-shipping');
    localStorage.removeItem('checkout-payment');
    localStorage.removeItem('checkout-total');
    localStorage.removeItem('checkout-items');
    // Mantener current-order-number para la página de éxito
  }

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
      
      // Crear preview si es imagen
      if (file.type.startsWith('image/')) {
        const reader = new FileReader();
        reader.onload = (e) => {
          this.paymentProofPreview.set(e.target?.result as string);
        };
        reader.readAsDataURL(file);
      } else {
        this.paymentProofPreview.set(null);
      }
    }
  }

  removePaymentProof() {
    this.paymentProofFile.set(null);
    this.paymentProofPreview.set(null);
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

      // Notificar al admin por WhatsApp (abrir chat)
      await this.notifyAdminByWhatsApp(proofData);

      this.toastService.success('Comprobante enviado exitosamente. Te notificaremos cuando sea verificado.');
      
      // Limpiar formulario
      this.paymentProofFile.set(null);
      this.paymentProofPreview.set(null);
      this.operationCode.set('');
      
    } catch (error: any) {
      console.error('Error uploading payment proof:', error);
      this.toastService.error('Error al subir el comprobante. Por favor intenta de nuevo.');
    } finally {
      this.paymentProofUploading.set(false);
    }
  }

  async notifyAdminByWhatsApp(proofData: any) {
    // Número de WhatsApp del admin (configurar)
    const adminWhatsAppNumber = '51987654321'; // TODO: Mover a configuración
    
    const paymentMethodText = proofData.paymentMethod === 'bank' 
      ? 'transferencia bancaria' 
      : proofData.paymentMethod === 'wallet' 
        ? `${proofData.walletMethod} (billetera digital)`
        : 'efectivo';
    
    const message = `🚨 NUEVO COMPROBANTE DE PAGO\n\n` +
      `📦 Pedido: ${proofData.orderNumber}\n` +
      `👤 Cliente: ${proofData.customerName}\n` +
      `📧 Email: ${proofData.email}\n` +
      `📱 Teléfono: ${proofData.phone}\n` +
      `💰 Monto: S/ ${this.getPriceFormatted(proofData.total)}\n` +
      `💳 Método: ${paymentMethodText}\n` +
      `🔢 Código: ${proofData.operationCode || 'No proporcionado'}\n\n` +
      `⚠️ Revisar inmediatamente en el panel de administración.`;
    
    const whatsappUrl = `https://wa.me/${adminWhatsAppNumber}?text=${encodeURIComponent(message)}`;
    
    // Abrir WhatsApp en nueva ventana
    window.open(whatsappUrl, '_blank');
  }

  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }

  getPaymentMethodName(): string {
    if (!this.paymentData) return '';
    switch (this.paymentData.paymentMethod) {
      case 'card': return 'Tarjeta de Crédito/Débito';
      case 'cash': return 'Efectivo al Recibir';
      case 'bank': return 'Transferencia Bancaria';
      case 'wallet': 
        const walletName = this.paymentData.walletMethod === 'yape' ? 'Yape' :
                          this.paymentData.walletMethod === 'plin' ? 'Plin' :
                          this.paymentData.walletMethod === 'tunki' ? 'Tunki' : 'Billetera Digital';
        return `Billetera Digital (${walletName})`;
      default: return '';
    }
  }

  requiresPaymentProof(): boolean {
    return this.paymentData?.requiresProof === true;
  }
}


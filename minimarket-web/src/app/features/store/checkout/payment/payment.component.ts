import { Component, OnInit, signal, computed, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../../core/services/cart.service';
import { PaymentsService } from '../../../../core/services/payments.service';
import { CheckoutStepperComponent } from '../../../../shared/components/checkout-stepper/checkout-stepper.component';
import { StoreHeaderComponent } from '../../../../shared/components/store-header/store-header.component';
import { ToastService } from '../../../../shared/services/toast.service';
import { loadStripe } from '@stripe/stripe-js';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    FormsModule,
    CheckoutStepperComponent,
    StoreHeaderComponent
  ],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css'
})
export class PaymentComponent implements OnInit, OnDestroy {
  paymentMethod = signal<'card' | 'cash' | 'bank' | 'wallet'>('card');
  // Ya no necesitamos walletMethod porque solo usamos Yape
  
  // Card details
  cardNumber = signal('');
  cardName = signal('');
  expiryDate = signal('');
  cvv = signal('');
  
  // Payment proof
  paymentProofFile = signal<File | null>(null);
  paymentProofPreview = signal<string | null>(null);
  paymentProofUploading = signal(false);
  
  // Stripe
  stripe: any = null;
  elements: any = null;
  cardElement: any = null;
  stripeLoading = signal(false);
  
  // Bank account info (BCP)
  bankAccountNumber = '193-12345678-0-00'; // TODO: Mover a configuración
  bankAccountName = 'Minimarket Camucha S.A.C.'; // TODO: Mover a configuración
  
  // Digital wallet info (solo Yape)
  yapeNumber = '999 888 777'; // TODO: Mover a configuración
  yapeQR = signal<string>('');
  
  cartItems!: ReturnType<typeof CartService.prototype.getCartItems>;
  shippingData: any = null;
  subtotal = computed(() => this.cartItems().reduce((sum: number, item: any) => sum + item.subtotal, 0));
  shippingCost = signal(0);
  total = computed(() => this.subtotal() + this.shippingCost());

  constructor(
    private cartService: CartService,
    private paymentsService: PaymentsService,
    private router: Router,
    private toastService: ToastService
  ) {
    // Inicializar cartItems después del constructor
    this.cartItems = this.cartService.getCartItems();
  }

  async ngOnInit() {
    // Cargar datos de envío desde localStorage (persistencia después de recargar)
    this.loadShippingDataFromStorage();
    
    // Cargar datos de pago desde localStorage (persistencia después de recargar)
    this.loadPaymentDataFromStorage();
    
    // Verificar que el carrito no esté vacío
    if (this.cartItems().length === 0) {
      this.router.navigate(['/carrito']);
      return;
    }

    // Inicializar Stripe solo si se selecciona tarjeta
    if (this.paymentMethod() === 'card') {
      await this.initializeStripe();
    }
  }

  // Cargar datos de envío desde localStorage
  private loadShippingDataFromStorage() {
    try {
      const shipping = localStorage.getItem('checkout-shipping');
      if (shipping) {
        const shippingData = JSON.parse(shipping);
        if (shippingData && typeof shippingData === 'object') {
          this.shippingData = shippingData;
          this.shippingCost.set(shippingData.shippingCost || 0);
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
  }

  // Cargar datos de pago desde localStorage
  private loadPaymentDataFromStorage() {
    try {
      const savedPayment = localStorage.getItem('checkout-payment');
      if (savedPayment) {
        const paymentData = JSON.parse(savedPayment);
        if (paymentData && typeof paymentData === 'object') {
          this.paymentMethod.set(paymentData.paymentMethod || 'card');
          
          // Si hay datos de tarjeta guardados, cargarlos
          if (paymentData.cardNumber) {
            this.cardNumber.set(paymentData.cardNumber || '');
          }
          if (paymentData.cardName) {
            this.cardName.set(paymentData.cardName || '');
          }
          if (paymentData.expiryDate) {
            this.expiryDate.set(paymentData.expiryDate || '');
          }
          if (paymentData.cvv) {
            this.cvv.set(paymentData.cvv || '');
          }
          
          // Si es billetera digital, generar QR
          if (this.paymentMethod() === 'wallet') {
            this.generateYapeQR();
          }
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
  }

  async initializeStripe() {
    try {
      // Cargar Stripe - por ahora deshabilitado hasta tener la clave pública
      // TODO: Agregar stripePublishableKey a environment
      // const stripePublishableKey = 'pk_test_...';
      // this.stripe = await loadStripe(stripePublishableKey);
      
      // Por ahora, usar campos manuales hasta configurar Stripe
      this.stripeLoading.set(false);
    } catch (error) {
      console.error('Error initializing Stripe:', error);
      this.stripeLoading.set(false);
    }
  }

  ngOnDestroy() {
    if (this.cardElement) {
      this.cardElement.destroy();
    }
  }

  onPaymentMethodChange(method: 'card' | 'cash' | 'bank' | 'wallet') {
    this.paymentMethod.set(method);
    if (method === 'card' && !this.stripe) {
      this.initializeStripe();
    } else if (method === 'wallet') {
      this.generateYapeQR(); // Generar QR de Yape automáticamente
      this.removePaymentProof(); // Clear proof if switching methods
    } else if (method === 'bank') {
      this.removePaymentProof(); // Clear proof if switching methods
    }
    // Guardar método de pago automáticamente
    this.savePaymentData();
  }

  // Guardar datos de pago automáticamente cuando cambian (para persistencia después de recargar)
  savePaymentData() {
    try {
      const paymentData: any = {
        paymentMethod: this.paymentMethod()
      };
      
      // Guardar datos de tarjeta si están llenos
      if (this.paymentMethod() === 'card') {
        if (this.cardNumber()) paymentData.cardNumber = this.cardNumber();
        if (this.cardName()) paymentData.cardName = this.cardName();
        if (this.expiryDate()) paymentData.expiryDate = this.expiryDate();
        if (this.cvv()) paymentData.cvv = this.cvv();
      } else if (this.paymentMethod() === 'wallet') {
        paymentData.walletMethod = 'yape';
        paymentData.walletNumber = this.yapeNumber;
        paymentData.requiresProof = true;
      } else if (this.paymentMethod() === 'bank') {
        paymentData.bankAccountNumber = this.bankAccountNumber;
        paymentData.bankAccountName = this.bankAccountName;
        paymentData.requiresProof = true;
      } else {
        paymentData.requiresProof = false;
      }
      
      localStorage.setItem('checkout-payment', JSON.stringify(paymentData));
    } catch (error) {
      console.error('Error saving payment data to localStorage:', error);
      // Continuar sin guardar si hay error (puede ser por espacio insuficiente)
    }
  }

  generateYapeQR() {
    // Generar QR dinámico de Yape con el monto
    const amount = this.total();
    const qrData = `yape://payment?phone=${this.yapeNumber.replace(/\s/g, '')}&amount=${amount.toFixed(2)}`;
    const qrUrl = `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(qrData)}`;
    this.yapeQR.set(qrUrl);
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

  copyToClipboard(text: string) {
    navigator.clipboard.writeText(text).then(() => {
      this.toastService.success('Número de cuenta copiado al portapapeles');
    }).catch(() => {
      this.toastService.error('Error al copiar al portapapeles');
    });
  }


  async continueToConfirmation() {
    // Validar según el método de pago
    if (this.paymentMethod() === 'card') {
      // Validar campos de tarjeta
      if (!this.cardNumber() || !this.cardName() || !this.expiryDate() || !this.cvv()) {
        this.toastService.error('Por favor completa todos los campos de la tarjeta');
        return;
      }

      // Si Stripe está disponible, procesar con Stripe
      if (this.stripe && this.cardElement) {
        try {
          this.stripeLoading.set(true);
          const paymentIntent = await firstValueFrom(
            this.paymentsService.createPaymentIntent({
              amount: this.total(),
              currency: 'pen',
              description: `Pedido Minimarket Camucha`
            })
          );

          if (paymentIntent) {
            const { error, paymentIntent: confirmedIntent } = await this.stripe.confirmCardPayment(
              paymentIntent.clientSecret,
              {
                payment_method: {
                  card: this.cardElement,
                  billing_details: {
                    name: this.cardName()
                  }
                }
              }
            );

            if (error) {
              this.toastService.error(error.message || 'Error al procesar el pago');
              this.stripeLoading.set(false);
              return;
            }

            if (confirmedIntent && confirmedIntent.status === 'succeeded') {
              const paymentData = {
                paymentMethod: this.paymentMethod(),
                paymentIntentId: confirmedIntent.id,
                status: confirmedIntent.status,
                requiresProof: false // Stripe no requiere comprobante
              };
              localStorage.setItem('checkout-payment', JSON.stringify(paymentData));
              localStorage.setItem('checkout-total', this.total().toString());
              
              // NO limpiar carrito aquí - se limpiará cuando se confirme el pedido
              // Solo navegar a confirmación
              this.router.navigate(['/checkout/confirmacion']);
              return;
            }
          }
        } catch (error: any) {
          console.error('Error processing payment:', error);
          this.toastService.error('Error al procesar el pago. Por favor intenta de nuevo.');
        } finally {
          this.stripeLoading.set(false);
        }
      }

      // Fallback: guardar datos manuales (para procesar después)
      const paymentData = {
        paymentMethod: this.paymentMethod(),
        cardNumber: this.cardNumber().replace(/\s/g, ''), // Remover espacios
        cardName: this.cardName(),
        expiryDate: this.expiryDate(),
        cvv: this.cvv(),
        requiresProof: false // Tarjeta manual no requiere comprobante (se procesará después)
      };
      localStorage.setItem('checkout-payment', JSON.stringify(paymentData));
      localStorage.setItem('checkout-total', this.total().toString());
      this.router.navigate(['/checkout/confirmacion']);
    } else if (this.paymentMethod() === 'bank') {
      // NO requerir comprobante aquí - se subirá después de crear el pedido
      const paymentData = {
        paymentMethod: this.paymentMethod(),
        bankAccountNumber: this.bankAccountNumber,
        bankAccountName: this.bankAccountName,
        requiresProof: true // Indica que necesita comprobante
      };
      localStorage.setItem('checkout-payment', JSON.stringify(paymentData));
      this.router.navigate(['/checkout/confirmacion']);
      
    } else if (this.paymentMethod() === 'wallet') {
      // NO requerir comprobante aquí - se subirá después de crear el pedido
      const paymentData = {
        paymentMethod: this.paymentMethod(),
        walletMethod: 'yape', // Siempre Yape
        walletNumber: this.yapeNumber,
        requiresProof: true // Indica que necesita comprobante
      };
      localStorage.setItem('checkout-payment', JSON.stringify(paymentData));
      this.router.navigate(['/checkout/confirmacion']);
    } else {
      // Efectivo al recibir
      const paymentData = {
        paymentMethod: this.paymentMethod(),
        requiresProof: false
      };
      localStorage.setItem('checkout-payment', JSON.stringify(paymentData));
      localStorage.setItem('checkout-total', this.total().toString());
      this.router.navigate(['/checkout/confirmacion']);
    }
  }

  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }

  // Método removido - el envío de comprobante ahora se hace desde la página de éxito
}


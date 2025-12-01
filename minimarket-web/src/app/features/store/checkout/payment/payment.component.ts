import { Component, OnInit, signal, computed, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../../core/services/cart.service';
import { PaymentsService } from '../../../../core/services/payments.service';
import { AuthService } from '../../../../core/services/auth.service';
import { CheckoutStepperComponent } from '../../../../shared/components/checkout-stepper/checkout-stepper.component';
import { StoreHeaderComponent } from '../../../../shared/components/store-header/store-header.component';
import { ToastService } from '../../../../shared/services/toast.service';
import { BrandSettingsService } from '../../../../core/services/brand-settings.service';
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
  paymentMethod = signal<'cash' | 'bank' | 'wallet'>('cash');
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
  
  // Bank account info - se carga desde BrandSettings
  bankAccountNumber = signal<string>('');
  bankAccountName = signal<string>('');
  bankName = signal<string>('');
  bankAccountType = signal<string>('');
  bankCCI = signal<string>('');
  
  // Digital wallet info (solo Yape) - se carga desde BrandSettings
  yapeNumber = signal<string>('999 888 777'); // Fallback si no hay configuración
  yapeQR = signal<string>('');
  yapeEnabled = signal<boolean>(true); // Estado de habilitación desde BrandSettings
  
  // Bank account info - se carga desde BrandSettings
  bankAccountEnabled = signal<boolean>(true); // Estado de habilitación desde BrandSettings
  
  cartItems!: ReturnType<typeof CartService.prototype.getCartItems>;
  shippingData: any = null;
  subtotal = computed(() => this.cartItems().reduce((sum: number, item: any) => sum + item.subtotal, 0));
  shippingCost = signal(0);
  total = computed(() => this.subtotal() + this.shippingCost());

  constructor(
    private cartService: CartService,
    private paymentsService: PaymentsService,
    private router: Router,
    private toastService: ToastService,
    private brandSettingsService: BrandSettingsService,
    private authService: AuthService
  ) {
    // Inicializar cartItems después del constructor
    this.cartItems = this.cartService.getCartItems();
  }

  async ngOnInit() {
    // Verificar que el usuario esté autenticado
    if (!this.authService.isAuthenticated()) {
      this.toastService.warning('Debes iniciar sesión para realizar un pedido');
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: '/checkout/pago' } });
      return;
    }

    // Cargar BrandSettings para obtener QR y número de Yape
    this.loadBrandSettings();
    
    // Cargar datos de envío desde localStorage (persistencia después de recargar)
    this.loadShippingDataFromStorage();
    
    // Cargar datos de pago desde localStorage (persistencia después de recargar)
    this.loadPaymentDataFromStorage();
    
    // Verificar que el carrito no esté vacío
    if (this.cartItems().length === 0) {
      this.router.navigate(['/carrito']);
      return;
    }

    // Ya no se usa Stripe/tarjeta
  }
  
  loadBrandSettings(): void {
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        if (settings) {
          // Cargar estado de habilitación de Yape/Plin (unificado)
          const isYapeEnabled = settings.yapeEnabled ?? settings.plinEnabled ?? false;
          this.yapeEnabled.set(isYapeEnabled);
          
          // Si está deshabilitado y estaba seleccionado, cambiar a efectivo
          if (!isYapeEnabled && this.paymentMethod() === 'wallet') {
            this.paymentMethod.set('cash');
            this.savePaymentData();
          }
          
          // Cargar estado de habilitación de Transferencia Bancaria
          const isBankEnabled = settings.bankAccountVisible ?? false;
          this.bankAccountEnabled.set(isBankEnabled);
          
          // Si está deshabilitado y estaba seleccionado, cambiar a efectivo
          if (!isBankEnabled && this.paymentMethod() === 'bank') {
            this.paymentMethod.set('cash');
            this.savePaymentData();
          }
          
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
          } else if (isYapeEnabled) {
            // Si no hay QR en BrandSettings pero está habilitado, generar uno dinámico como fallback
            this.generateYapeQR();
          }
          
          // Cargar datos de cuenta bancaria desde BrandSettings
          this.bankName.set(settings.bankName || '');
          this.bankAccountType.set(settings.bankAccountType || '');
          this.bankAccountNumber.set(settings.bankAccountNumber || '');
          this.bankCCI.set(settings.bankCCI || '');
          // Nombre del titular: usar storeName como fallback
          this.bankAccountName.set(settings.storeName || 'Minimarket Camucha S.A.C.');
        } else {
          // Si no hay settings, deshabilitar por defecto
          this.yapeEnabled.set(false);
        }
      },
      error: (error) => {
        console.error('Error loading brand settings:', error);
        // En caso de error, deshabilitar por defecto
        this.yapeEnabled.set(false);
        this.bankAccountEnabled.set(false);
        if (this.paymentMethod() === 'wallet' || this.paymentMethod() === 'bank') {
          this.paymentMethod.set('cash');
          this.savePaymentData();
        }
      }
    });
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
          this.paymentMethod.set(paymentData.paymentMethod || 'cash');
          
          // Si es billetera digital y no hay QR cargado, generar uno dinámico
          if (this.paymentMethod() === 'wallet' && !this.yapeQR()) {
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

  onPaymentMethodChange(method: 'cash' | 'bank' | 'wallet') {
    this.paymentMethod.set(method);
    if (method === 'wallet') {
      // Si no hay QR cargado desde BrandSettings, generar uno dinámico
      if (!this.yapeQR()) {
        this.generateYapeQR();
      }
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
      
      if (this.paymentMethod() === 'wallet') {
        paymentData.walletMethod = 'yape';
        paymentData.walletNumber = this.yapeNumber;
        paymentData.requiresProof = true;
      } else if (this.paymentMethod() === 'bank') {
        paymentData.bankName = this.bankName();
        paymentData.bankAccountType = this.bankAccountType();
        paymentData.bankAccountNumber = this.bankAccountNumber();
        paymentData.bankCCI = this.bankCCI();
        paymentData.bankAccountName = this.bankAccountName();
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
    // Generar QR dinámico de Yape con el monto (solo si no hay QR en BrandSettings)
    // Este método se usa como fallback cuando no hay QR subido en el admin
    const amount = this.total();
    const phoneNumber = this.yapeNumber().replace(/\s/g, '');
    const qrData = `yape://payment?phone=${phoneNumber}&amount=${amount.toFixed(2)}`;
    const qrUrl = `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(qrData)}`;
    // Solo establecer si no hay QR ya cargado desde BrandSettings
    if (!this.yapeQR()) {
      this.yapeQR.set(qrUrl);
    }
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
    if (this.paymentMethod() === 'bank') {
      // NO requerir comprobante aquí - se subirá después de crear el pedido
      const paymentData = {
        paymentMethod: this.paymentMethod(),
        bankName: this.bankName(),
        bankAccountType: this.bankAccountType(),
        bankAccountNumber: this.bankAccountNumber(),
        bankCCI: this.bankCCI(),
        bankAccountName: this.bankAccountName(),
        requiresProof: true // Indica que necesita comprobante
      };
      localStorage.setItem('checkout-payment', JSON.stringify(paymentData));
      this.router.navigate(['/checkout/confirmacion']);
      
    } else if (this.paymentMethod() === 'wallet') {
      // NO requerir comprobante aquí - se subirá después de crear el pedido
      const paymentData = {
        paymentMethod: this.paymentMethod(),
        walletMethod: 'yape', // Siempre Yape
        walletNumber: this.yapeNumber(),
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


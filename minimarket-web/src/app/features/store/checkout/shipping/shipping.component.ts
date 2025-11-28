import { Component, OnInit, signal, computed, NgZone, effect, afterNextRender, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../../core/services/cart.service';
import { ShippingService } from '../../../../core/services/shipping.service';
import { SedesService, Sede } from '../../../../core/services/sedes.service';
import { AuthService, UserAddress } from '../../../../core/services/auth.service';
import { SettingsService } from '../../../../core/services/settings.service';
import { StoreHeaderComponent } from '../../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../../shared/components/store-footer/store-footer.component';
import { CheckoutStepperComponent } from '../../../../shared/components/checkout-stepper/checkout-stepper.component';
import { ToastService } from '../../../../shared/services/toast.service';
import { firstValueFrom, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-shipping',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    FormsModule, 
    StoreHeaderComponent, 
    StoreFooterComponent,
    CheckoutStepperComponent
  ],
  templateUrl: './shipping.component.html',
  styleUrl: './shipping.component.css'
})
export class ShippingComponent implements OnInit {
  // Contact info
  email = signal('');
  
  // Shipping address
  isDifferentRecipient = signal(false);
  firstName = signal('');
  lastName = signal('');
  recipientFirstName = signal('');
  recipientLastName = signal('');
  recipientDni = signal('');
  recipientPhone = signal('');
  address = signal('');
  city = signal('');
  region = signal('');
  district = signal('');
  
  // User addresses
  userAddresses = signal<UserAddress[]>([]);
  selectedAddress = signal<UserAddress | null>(null);
  showAddressForm = signal(false);
  showAddressSelector = signal(false);
  
  // Store locations (sedes)
  sedes = signal<Sede[]>([]);
  selectedSede = signal<Sede | null>(null);
  
  // Coordinates for distance calculation
  private storeLat = -12.0464; // Latitud de la tienda (se actualizará con la sede seleccionada)
  private storeLon = -77.0428; // Longitud de la tienda (se actualizará con la sede seleccionada)
  customerLat = signal<number | null>(null);
  customerLon = signal<number | null>(null);
  
  // Shipping method
  shippingMethod = signal<'delivery' | 'pickup'>('delivery');
  shippingCost = signal(0);
  isCalculatingShipping = signal(false);
  shippingCalculationDetails = signal('');
  
  // Configuración de envío simplificado
  fixedShippingPrice = signal(8.00);
  freeShippingThreshold = signal(20.00);
  
  cartItems!: ReturnType<typeof CartService.prototype.getCartItems>;
  subtotal = computed(() => this.cartItems().reduce((sum: number, item: any) => sum + item.subtotal, 0));
  totalWeight = computed(() => {
    // Calcular peso total estimado (puedes mejorar esto con peso real de productos)
    // Estimación: 0.5kg por producto en promedio
    return this.cartItems().length * 0.5;
  });
  total = computed(() => this.subtotal() + this.shippingCost());

  private destroyRef = inject(DestroyRef);
  private shippingEffectCleanup?: ReturnType<typeof effect>;

  constructor(
    private cartService: CartService,
    private shippingService: ShippingService,
    private sedesService: SedesService,
    private authService: AuthService,
    private settingsService: SettingsService,
    private router: Router,
    private ngZone: NgZone,
    private toastService: ToastService
  ) {
    // Inicializar cartItems después del constructor
    this.cartItems = this.cartService.getCartItems();
  }

  async ngOnInit() {
    // Verificar que el carrito no esté vacío
    if (this.cartItems().length === 0) {
      this.router.navigate(['/carrito']);
      return;
    }

    // Los datos de checkout se mantendrán hasta que:
    // 1. Se complete la venta (en confirmation.component.ts)
    // 2. Se limpie el carrito manualmente (en cart.service.ts)
    // No limpiar aquí para permitir que los datos persistan durante todo el proceso

    // Cargar sedes primero para que estén disponibles cuando se carguen los datos guardados
    this.loadSedes();
    
    // Cargar direcciones del usuario y dirección predeterminada
    await this.loadUserAddresses();
    
    // Cargar datos del perfil del usuario si está autenticado
    await this.loadUserProfile();
    
    // Cargar configuraciones de envío simplificado primero
    this.loadShippingSettings();
    
    // Cargar datos guardados desde localStorage (persistencia incluso después de recargar la página)
    // Esto protege contra pérdida de datos por errores de red o recargas accidentales
    // La sede ya se carga dentro de loadSedes(), así que solo cargamos los demás datos aquí
    // Solo cargar si no hay dirección seleccionada ya cargada
    if (!this.selectedAddress() || !this.address()) {
      this.loadShippingDataFromStorage();
    }

    // Calcular shipping inicial después de que el componente esté inicializado
    // El cálculo se hará automáticamente cuando las configuraciones se carguen
    // en loadShippingSettings(), pero también lo intentamos aquí como respaldo
    this.ngZone.run(() => {
      // Pequeño delay para asegurar que las configuraciones se carguen
      setTimeout(() => {
        // Validar que si no hay sedes y el método es pickup, cambiar a delivery
        if (this.shippingMethod() === 'pickup' && this.sedes().length === 0) {
          this.shippingMethod.set('delivery');
        }
        
        if (this.shippingMethod() === 'delivery') {
          // Siempre recalcular si hay dirección para usar el sistema simplificado actualizado
          if (this.address()) {
            this.calculateShippingCost();
          } else {
            // Si no hay dirección, mostrar precio estimado basado en el sistema simplificado
            // Solo calcular si las configuraciones ya están cargadas
            if (this.fixedShippingPrice() > 0 && this.freeShippingThreshold() > 0) {
              this.calculateEstimatedShipping();
            }
          }
        } else if (this.shippingMethod() === 'pickup' && this.sedes().length > 0) {
          // Pickup siempre es gratis, solo si hay sedes disponibles
          this.shippingCost.set(0);
          this.shippingCalculationDetails.set('Retiro en tienda - Sin costo de envío');
          this.onFieldChange();
        }
      }, 200); // Aumentar delay para asegurar que las configuraciones se carguen
    });
    
    // Efecto para recalcular precio estimado cuando cambia el subtotal
    // Solo si no hay dirección y el método es delivery
    afterNextRender(() => {
      this.shippingEffectCleanup = effect(() => {
        const subtotalValue = this.subtotal();
        const method = this.shippingMethod();
        const hasAddress = this.address();
        
        // Recalcular precio estimado si:
        // - El método es delivery
        // - No hay dirección
        // - Las configuraciones ya están cargadas (valores > 0)
        if (method === 'delivery' && !hasAddress && 
            this.fixedShippingPrice() > 0 && this.freeShippingThreshold() > 0) {
          this.calculateEstimatedShipping();
        }
      });

      // Limpiar el effect cuando el componente se destruya
      this.destroyRef.onDestroy(() => {
        this.shippingEffectCleanup?.destroy();
      });
    });
  }
  
  loadSedes(): void {
    this.sedesService.getAll(true).subscribe({
      next: (sedes) => {
        this.sedes.set(sedes);
        
        // Si no hay sedes disponibles, forzar método delivery
        if (sedes.length === 0) {
          if (this.shippingMethod() === 'pickup') {
            this.shippingMethod.set('delivery');
            this.calculateEstimatedShipping();
          }
          this.selectedSede.set(null);
          return;
        }
        
        // Intentar cargar la sede guardada desde localStorage
        try {
          const savedShipping = localStorage.getItem('checkout-shipping');
          if (savedShipping) {
            const shippingData = JSON.parse(savedShipping);
            if (shippingData?.selectedSede) {
              const savedSede = sedes.find(s => s.id === shippingData.selectedSede.id);
              if (savedSede) {
                this.selectedSede.set(savedSede);
                this.storeLat = savedSede.latitud;
                this.storeLon = savedSede.longitud;
                return;
              }
            }
          }
        } catch (error) {
          console.error('Error loading saved sede:', error);
        }
        
        // Si no hay sede guardada, seleccionar la primera por defecto
        if (sedes.length > 0 && !this.selectedSede()) {
          this.selectedSede.set(sedes[0]);
          this.storeLat = sedes[0].latitud;
          this.storeLon = sedes[0].longitud;
        }
      },
      error: (error) => {
        console.error('Error loading sedes:', error);
        this.sedes.set([]);
        // Si hay error cargando sedes, forzar método delivery
        if (this.shippingMethod() === 'pickup') {
          this.shippingMethod.set('delivery');
          this.calculateEstimatedShipping();
        }
      }
    });
  }

  private loadShippingSettings(): void {
    let priceLoaded = false;
    let thresholdLoaded = false;
    
    const tryCalculateEstimated = () => {
      if (priceLoaded && thresholdLoaded && 
          this.shippingMethod() === 'delivery' && !this.address()) {
        this.calculateEstimatedShipping();
      }
    };

    const loadSetting = (key: string, defaultValue: number, setter: (value: number) => void) => {
      this.settingsService.getByKey(key).subscribe({
        next: (setting: any) => {
          const value = setting?.value ? parseFloat(setting.value) || defaultValue : defaultValue;
          setter(value);
          if (key === 'fixed_shipping_price') {
            priceLoaded = true;
          } else {
            thresholdLoaded = true;
          }
          tryCalculateEstimated();
        },
        error: () => {
          setter(defaultValue);
          if (key === 'fixed_shipping_price') {
            priceLoaded = true;
          } else {
            thresholdLoaded = true;
          }
          tryCalculateEstimated();
        }
      });
    };

    loadSetting('fixed_shipping_price', 8.00, (value) => this.fixedShippingPrice.set(value));
    loadSetting('free_shipping_threshold', 20.00, (value) => this.freeShippingThreshold.set(value));
  }

  private calculateEstimatedShipping(): void {
    const subtotalValue = this.subtotal();
    const threshold = this.freeShippingThreshold();
    const fixedPrice = this.fixedShippingPrice();
    const isFreeShipping = subtotalValue >= threshold;
    
    this.shippingCost.set(isFreeShipping ? 0 : fixedPrice);
    this.shippingCalculationDetails.set(
      isFreeShipping
        ? `Envío gratis - Compra mínima alcanzada (S/ ${threshold.toFixed(2)})`
        : `Envío fijo para Lima: S/ ${fixedPrice.toFixed(2)} (precio estimado)`
    );
  }

  calculateShippingCost(): void {
    if (this.shippingMethod() === 'pickup') {
      this.setPickupShipping();
      return;
    }

    if (!this.address()) {
      this.calculateEstimatedShipping();
      return;
    }

    const distance = this.calculateDistance();
    this.isCalculatingShipping.set(true);
    
    this.shippingService.calculateShipping({
      subtotal: this.subtotal(),
      totalWeight: this.totalWeight(),
      distance,
      zoneName: this.city() || undefined,
      deliveryMethod: this.shippingMethod()
    }).subscribe({
      next: (response) => {
        this.shippingCost.set(response.shippingCost);
        this.shippingCalculationDetails.set(response.calculationDetails || '');
        this.isCalculatingShipping.set(false);
        this.onFieldChange();
      },
      error: () => {
        const defaultPrice = this.fixedShippingPrice() || 8.00;
        this.shippingCost.set(defaultPrice);
        this.shippingCalculationDetails.set(`Envío fijo para Lima: S/ ${defaultPrice.toFixed(2)}`);
        this.isCalculatingShipping.set(false);
        this.onFieldChange();
      }
    });
  }

  private setPickupShipping(): void {
    this.shippingCost.set(0);
    this.shippingCalculationDetails.set('Retiro en tienda - Sin costo de envío');
    this.onFieldChange();
  }

  private calculateDistance(): number {
    if (this.customerLat() && this.customerLon()) {
      return this.shippingService.calculateDistance(
        this.storeLat,
        this.storeLon,
        this.customerLat()!,
        this.customerLon()!
      );
    }
    return 5; // Distancia por defecto en km
  }

  onShippingMethodChange(method: 'delivery' | 'pickup'): void {
    // Validar que si no hay sedes, no se permita seleccionar pickup
    if (method === 'pickup' && this.sedes().length === 0) {
      this.toastService.error('No hay sedes disponibles para retiro en tienda. Por favor, selecciona envío a domicilio');
      return;
    }
    
    this.shippingMethod.set(method);
    
    if (method === 'delivery') {
      this.handleDeliveryMethodChange();
    } else {
      this.setPickupShipping();
    }
  }

  private handleDeliveryMethodChange(): void {
    if (this.selectedAddress() && !this.showAddressForm() && !this.address()) {
      this.loadAddressData(this.selectedAddress()!);
    } else if (this.address()) {
      this.calculateShippingCost();
    } else {
      this.calculateEstimatedShipping();
    }
  }

  onSedeChange(sedeId: string): void {
    const sede = this.sedes().find(s => s.id === sedeId);
    if (sede) {
      this.selectedSede.set(sede);
      this.storeLat = sede.latitud;
      this.storeLon = sede.longitud;
      // Si hay dirección y es delivery, recalcular costo
      if (this.shippingMethod() === 'delivery' && this.address()) {
        this.calculateShippingCost();
      }
      this.onFieldChange();
    }
  }

  onAddressChange() {
    // Cuando el usuario cambia la dirección manualmente, marcar que no se está usando una dirección guardada
    if (this.address() && this.selectedAddress()) {
      this.selectedAddress.set(null);
      this.showAddressForm.set(true);
    }
    
    // Cuando el usuario cambia la dirección, recalcular distancia
    // Nota: En producción, usarías un servicio de geocoding para obtener coordenadas
    // Por ahora, usamos una distancia estimada basada en la ciudad
    if (this.shippingMethod() === 'delivery' && this.address()) {
      this.calculateShippingCost();
    }
    // Guardar cambios automáticamente
    this.onFieldChange();
  }

  async loadUserAddresses(): Promise<void> {
    if (!this.authService.isAuthenticated()) {
      return;
    }

    try {
      const addresses = await firstValueFrom(
        this.authService.getAddresses().pipe(
          catchError((error: any) => {
            if (error?.status === 401) {
              return of([]);
            }
            throw error;
          })
        )
      );
      
      this.userAddresses.set(addresses);
      this.loadDefaultAddress(addresses);
    } catch (error: any) {
      if (error?.status !== 401) {
        console.error('Error loading user addresses:', error);
      }
      this.userAddresses.set([]);
    }
  }

  private loadDefaultAddress(addresses: UserAddress[]): void {
    const defaultAddr = addresses.find(addr => addr.isDefault) || addresses[0];
    if (defaultAddr && !this.address() && !this.city() && !this.region()) {
      this.selectedAddress.set(defaultAddr);
      this.loadAddressData(defaultAddr);
    }
  }
  
  loadAddressData(address: UserAddress) {
    this.selectedAddress.set(address);
    this.address.set(address.address);
    this.city.set(address.city);
    this.region.set(address.region);
    this.district.set(address.district);
    this.recipientPhone.set(address.phone);
    
    // Si es diferente destinatario, cargar datos del destinatario
    if (address.isDifferentRecipient) {
      this.isDifferentRecipient.set(true);
      this.recipientFirstName.set(address.firstName || '');
      this.recipientLastName.set(address.lastName || '');
      this.recipientDni.set(address.dni || '');
    } else {
      this.isDifferentRecipient.set(false);
    }
    
    // Calcular costo de envío con la dirección seleccionada
    if (this.shippingMethod() === 'delivery') {
      this.calculateShippingCost();
    }
    
    this.showAddressForm.set(false);
    this.showAddressSelector.set(false);
    this.onFieldChange();
  }
  
  selectAddress(address: UserAddress) {
    this.loadAddressData(address);
  }
  
  changeAddress() {
    // Si hay varias direcciones, mostrar selector; si no, mostrar formulario
    if (this.userAddresses().length > 1) {
      this.showAddressSelector.set(true);
    } else {
      this.showAddressForm.set(true);
      this.selectedAddress.set(null);
    }
  }
  
  cancelAddressChange() {
    this.showAddressForm.set(false);
    this.showAddressSelector.set(false);
    // Restaurar dirección seleccionada si existe
    const selected = this.selectedAddress();
    if (selected) {
      this.loadAddressData(selected);
    }
  }
  
  useNewAddress() {
    this.showAddressSelector.set(false);
    this.showAddressForm.set(true);
    this.selectedAddress.set(null);
    // Limpiar campos para nueva dirección
    this.address.set('');
    this.city.set('');
    this.region.set('');
    this.district.set('');
  }

  async loadUserProfile(): Promise<void> {
    if (!this.authService.isAuthenticated()) {
      return;
    }

    try {
      const profile = await firstValueFrom(
        this.authService.getProfile().pipe(
          catchError((error: any) => {
            if (error?.status === 401) {
              return of(null);
            }
            throw error;
          })
        )
      );
      
      if (profile && !this.firstName() && !this.lastName()) {
        this.firstName.set(profile.firstName || '');
        this.lastName.set(profile.lastName || '');
        this.email.set(profile.email || '');
      }
    } catch (error: any) {
      if (error?.status !== 401) {
        console.error('Error loading user profile:', error);
      }
    }
  }

  onRecipientChange() {
    const isDifferent = this.isDifferentRecipient();
    if (!isDifferent) {
      // Si no es diferente destinatario, cargar datos del perfil
      this.loadUserProfile();
      // Limpiar campos de destinatario diferente
      this.recipientFirstName.set('');
      this.recipientLastName.set('');
      this.recipientDni.set('');
      this.recipientPhone.set('');
    }
    this.onFieldChange();
  }

  continueToPayment() {
    // Validar que los datos requeridos estén completos según el método seleccionado
    if (!this.email()) {
      this.toastService.error('Por favor, ingresa tu correo electrónico');
      return;
    }
    
    // Validar formato de email
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.email())) {
      this.toastService.error('Por favor, ingresa un correo electrónico válido');
      return;
    }
    
    if (!this.firstName()) {
      this.toastService.error('Por favor, ingresa tu nombre');
      return;
    }
    
    if (!this.lastName()) {
      this.toastService.error('Por favor, ingresa tu apellido');
      return;
    }

    // Si es diferente destinatario, validar campos del destinatario
    if (this.isDifferentRecipient()) {
      if (!this.recipientFirstName()) {
        this.toastService.error('Por favor, ingresa el nombre del destinatario');
        return;
      }
      if (!this.recipientLastName()) {
        this.toastService.error('Por favor, ingresa el apellido del destinatario');
        return;
      }
      if (!this.recipientDni()) {
        this.toastService.error('Por favor, ingresa el DNI del destinatario');
        return;
      }
      if (!this.recipientPhone()) {
        this.toastService.error('Por favor, ingresa el teléfono del destinatario');
        return;
      }
    }

    if (this.shippingMethod() === 'delivery') {
      // Para delivery, validar dirección completa solo si no hay dirección seleccionada o se está usando el formulario
      if (!this.selectedAddress() || this.showAddressForm()) {
        if (!this.address()) {
          this.toastService.error('Por favor, ingresa la dirección de envío');
          return;
        }
        if (!this.city()) {
          this.toastService.error('Por favor, ingresa la ciudad');
          return;
        }
        if (!this.region()) {
          this.toastService.error('Por favor, ingresa la región o departamento');
          return;
        }
      } else if (this.selectedAddress() && !this.address()) {
        // Si hay dirección seleccionada pero no se cargó, cargarla ahora
        this.loadAddressData(this.selectedAddress()!);
      }
    } else if (this.shippingMethod() === 'pickup') {
      // Validar que haya sedes disponibles
      if (this.sedes().length === 0) {
        this.toastService.error('No hay sedes disponibles para retiro en tienda. Por favor, selecciona envío a domicilio');
        this.shippingMethod.set('delivery');
        this.calculateEstimatedShipping();
        return;
      }
      
      // Validar que se haya seleccionado una sede
      if (!this.selectedSede()) {
        this.toastService.error('Por favor, selecciona una sede para retirar tu pedido');
        return;
      }
    }

    const shippingData = this.buildShippingData();
    localStorage.setItem('checkout-shipping', JSON.stringify(shippingData));
    
    // Navegar al siguiente paso
    this.router.navigate(['/checkout/pago']);
  }

  // Cargar datos de envío desde localStorage (para persistencia después de recargar)
  private loadShippingDataFromStorage() {
    try {
      const savedShipping = localStorage.getItem('checkout-shipping');
      if (savedShipping) {
        const shippingData = JSON.parse(savedShipping);
        // Validar que los datos sean válidos antes de cargar
        if (shippingData && typeof shippingData === 'object') {
          // Solo cargar email y nombre si no están ya cargados
          if (!this.email()) {
            this.email.set(shippingData.email || '');
          }
          if (!this.firstName()) {
            this.firstName.set(shippingData.firstName || '');
          }
          if (!this.lastName()) {
            this.lastName.set(shippingData.lastName || '');
          }
          
          // Solo cargar dirección si no hay dirección seleccionada ya cargada
          if (!this.selectedAddress() || !this.address()) {
            this.isDifferentRecipient.set(shippingData.isDifferentRecipient || false);
            this.recipientFirstName.set(shippingData.recipientFirstName || '');
            this.recipientLastName.set(shippingData.recipientLastName || '');
            this.recipientDni.set(shippingData.recipientDni || '');
            this.recipientPhone.set(shippingData.recipientPhone || '');
            this.address.set(shippingData.address || '');
            this.city.set(shippingData.city || '');
            this.region.set(shippingData.region || '');
            this.district.set(shippingData.district || '');
          }
          
          this.shippingMethod.set(shippingData.shippingMethod || 'delivery');
          
          // NO cargar shippingCost ni shippingCalculationDetails desde localStorage
          // Siempre recalcular para usar el sistema simplificado actualizado
          // Esto evita usar cálculos antiguos guardados
          this.shippingCost.set(0);
          this.shippingCalculationDetails.set('');
          
          // La sede ya se carga en loadSedes(), no necesitamos cargarla aquí de nuevo
        }
      }
    } catch (error) {
      console.error('Error loading saved shipping data from localStorage:', error);
      // Si hay error, limpiar datos corruptos
      try {
        localStorage.removeItem('checkout-shipping');
      } catch (e) {
        console.error('Error clearing corrupted shipping data:', e);
      }
    }
  }

  onFieldChange(): void {
    try {
      const shippingData = this.buildShippingData();
      localStorage.setItem('checkout-shipping', JSON.stringify(shippingData));
    } catch (error) {
      console.error('Error saving shipping data to localStorage:', error);
    }
  }

  private buildShippingData() {
    return {
      email: this.email(),
      firstName: this.firstName(),
      lastName: this.lastName(),
      isDifferentRecipient: this.isDifferentRecipient(),
      recipientFirstName: this.recipientFirstName(),
      recipientLastName: this.recipientLastName(),
      recipientDni: this.recipientDni(),
      recipientPhone: this.recipientPhone(),
      address: this.address(),
      city: this.city(),
      region: this.region(),
      district: this.district(),
      shippingMethod: this.shippingMethod() as 'delivery' | 'pickup',
      shippingCost: this.shippingCost(),
      shippingCalculationDetails: this.shippingCalculationDetails(),
      selectedSede: this.selectedSede() ? this.buildSedeData(this.selectedSede()!) : null
    };
  }

  private buildSedeData(sede: Sede) {
    return {
      id: sede.id,
      nombre: sede.nombre,
      direccion: sede.direccion,
      ciudad: sede.ciudad,
      telefono: sede.telefono
    };
  }

  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }

  getDeliveryShippingPrice(): { price: number; isFree: boolean } {
    const isSelected = this.shippingMethod() === 'delivery';
    const hasAddress = !!this.address();
    const subtotalValue = this.subtotal();
    const threshold = this.freeShippingThreshold();
    const fixedPrice = this.fixedShippingPrice();
    const isFreeShipping = subtotalValue >= threshold;

    if (!isSelected) {
      return {
        price: isFreeShipping ? 0 : fixedPrice,
        isFree: isFreeShipping
      };
    }

    if (!hasAddress && fixedPrice > 0) {
      return {
        price: isFreeShipping ? 0 : fixedPrice,
        isFree: isFreeShipping
      };
    }

    if (this.shippingCost() === 0 && isFreeShipping) {
      return { price: 0, isFree: true };
    }

    if (this.shippingCost() > 0) {
      return { price: this.shippingCost(), isFree: false };
    }

    return {
      price: isFreeShipping ? 0 : fixedPrice,
      isFree: isFreeShipping
    };
  }
}


import { Component, OnInit, signal, computed, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../../core/services/cart.service';
import { ShippingService } from '../../../../core/services/shipping.service';
import { SedesService, Sede } from '../../../../core/services/sedes.service';
import { AuthService, UserAddress } from '../../../../core/services/auth.service';
import { StoreHeaderComponent } from '../../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../../shared/components/store-footer/store-footer.component';
import { CheckoutStepperComponent } from '../../../../shared/components/checkout-stepper/checkout-stepper.component';
import { ToastService } from '../../../../shared/services/toast.service';
import { firstValueFrom } from 'rxjs';

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
  
  cartItems!: ReturnType<typeof CartService.prototype.getCartItems>;
  subtotal = computed(() => this.cartItems().reduce((sum: number, item: any) => sum + item.subtotal, 0));
  totalWeight = computed(() => {
    // Calcular peso total estimado (puedes mejorar esto con peso real de productos)
    // Estimación: 0.5kg por producto en promedio
    return this.cartItems().length * 0.5;
  });
  total = computed(() => this.subtotal() + this.shippingCost());

  constructor(
    private cartService: CartService,
    private shippingService: ShippingService,
    private sedesService: SedesService,
    private authService: AuthService,
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
    
    // Cargar datos guardados desde localStorage (persistencia incluso después de recargar la página)
    // Esto protege contra pérdida de datos por errores de red o recargas accidentales
    // La sede ya se carga dentro de loadSedes(), así que solo cargamos los demás datos aquí
    // Solo cargar si no hay dirección seleccionada ya cargada
    if (!this.selectedAddress() || !this.address()) {
      this.loadShippingDataFromStorage();
    }

    // Calcular shipping inicial después de que el componente esté inicializado
    // Usar NgZone.run para asegurar que se ejecute dentro de la zona de Angular
    this.ngZone.run(() => {
      if (this.shippingMethod() === 'delivery') {
        // Si hay datos guardados con costo válido (> 0), usarlos y mostrarlos
        // Si el costo es 0 pero hay dirección, recalcular
        if (this.shippingCost() === 0 && this.address()) {
          // Recalcular solo si no hay costo guardado
          this.calculateShippingCost();
        } else if (this.shippingCost() === 0 && !this.address()) {
          // Si no hay dirección, mantener en 0 pero sin detalles
          this.shippingCalculationDetails.set('');
        }
        // Si shippingCost() > 0, ya está cargado desde localStorage y se mostrará automáticamente
      } else {
        // Pickup siempre es gratis
        this.shippingCost.set(0);
        this.shippingCalculationDetails.set('Retiro en tienda - Sin costo de envío');
        this.onFieldChange(); // Guardar el cambio
      }
    });
  }
  
  loadSedes(): void {
    // Cargar sedes activas para mostrar en pickup
    this.sedesService.getAll(true).subscribe({
      next: (sedes) => {
        this.sedes.set(sedes);
        
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
                return; // Ya tenemos la sede guardada, no seleccionar la primera
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
      }
    });
  }

  calculateShippingCost() {
    if (this.shippingMethod() === 'pickup') {
      this.shippingCost.set(0);
      this.shippingCalculationDetails.set('Retiro en tienda - Sin costo de envío');
      this.onFieldChange(); // Guardar el cambio
      return;
    }

    // Si no hay dirección, no calcular
    if (!this.address()) {
      this.shippingCost.set(0);
      this.shippingCalculationDetails.set('');
      return;
    }

    // Calcular distancia (si tenemos coordenadas del cliente)
    let distance = 5; // Distancia por defecto en km (si no hay coordenadas)
    
    if (this.customerLat() && this.customerLon()) {
      distance = this.shippingService.calculateDistance(
        this.storeLat,
        this.storeLon,
        this.customerLat()!,
        this.customerLon()!
      );
    }

    this.isCalculatingShipping.set(true);
    
    this.shippingService.calculateShipping({
      subtotal: this.subtotal(),
      totalWeight: this.totalWeight(),
      distance: distance,
      zoneName: this.city() || undefined,
      deliveryMethod: this.shippingMethod()
    }).subscribe({
      next: (response) => {
        this.shippingCost.set(response.shippingCost);
        this.shippingCalculationDetails.set(response.calculationDetails || '');
        this.isCalculatingShipping.set(false);
        // Guardar cambios automáticamente después de calcular
        this.onFieldChange();
      },
      error: (error) => {
        console.error('Error calculating shipping:', error);
        // Usar tarifa por defecto en caso de error
        this.shippingCost.set(3.50);
        this.shippingCalculationDetails.set('Tarifa por defecto aplicada');
        this.isCalculatingShipping.set(false);
        // Guardar cambios automáticamente después de calcular
        this.onFieldChange();
      }
    });
  }

  onShippingMethodChange(method: 'delivery' | 'pickup') {
    this.shippingMethod.set(method);
    if (method === 'delivery') {
      // Si hay dirección seleccionada y no se está usando el formulario, cargarla
      if (this.selectedAddress() && !this.showAddressForm() && !this.address()) {
        this.loadAddressData(this.selectedAddress()!);
      } else if (this.shippingCost() > 0 && this.address()) {
        // Ya hay costo guardado, solo guardar el cambio de método
        this.onFieldChange();
      } else if (this.address()) {
        // Recalcular el costo si hay dirección
        this.calculateShippingCost();
      }
    } else {
      this.shippingCost.set(0);
      this.shippingCalculationDetails.set('Retiro en tienda - Sin costo de envío');
      this.onFieldChange(); // Guardar cambios automáticamente
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

  async loadUserAddresses() {
    if (this.authService.isAuthenticated()) {
      try {
        const addresses = await firstValueFrom(this.authService.getAddresses());
        this.userAddresses.set(addresses);
        
        // Buscar dirección predeterminada o la primera disponible
        const defaultAddr = addresses.find(addr => addr.isDefault) || addresses[0];
        if (defaultAddr) {
          this.selectedAddress.set(defaultAddr);
          // Cargar datos de la dirección seleccionada si no hay datos guardados
          if (!this.address() && !this.city() && !this.region()) {
            this.loadAddressData(defaultAddr);
          }
        }
      } catch (error) {
        console.error('Error loading user addresses:', error);
        this.userAddresses.set([]);
      }
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

  async loadUserProfile() {
    if (this.authService.isAuthenticated()) {
      try {
        const profile = await firstValueFrom(this.authService.getProfile());
        if (profile && !this.firstName() && !this.lastName()) {
          // Solo cargar si no hay datos guardados
          this.firstName.set(profile.firstName || '');
          this.lastName.set(profile.lastName || '');
          this.email.set(profile.email || '');
        }
      } catch (error) {
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
      // Para pickup, validar que se haya seleccionado una sede
      if (!this.selectedSede()) {
        this.toastService.error('Por favor, selecciona una sede para retirar tu pedido');
        return;
      }
    }

    // Guardar datos en localStorage o servicio
    const shippingData = {
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
      selectedSede: this.selectedSede() ? {
        id: this.selectedSede()!.id,
        nombre: this.selectedSede()!.nombre,
        direccion: this.selectedSede()!.direccion,
        ciudad: this.selectedSede()!.ciudad,
        telefono: this.selectedSede()!.telefono
      } : null
    };
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
          this.shippingCost.set(shippingData.shippingCost || 0);
          this.shippingCalculationDetails.set(shippingData.shippingCalculationDetails || '');
          
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

  // Guardar datos automáticamente cuando cambian (para persistencia mientras navega y después de recargar)
  onFieldChange() {
    try {
      const shippingData = {
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
        selectedSede: this.selectedSede() ? {
          id: this.selectedSede()!.id,
          nombre: this.selectedSede()!.nombre,
          direccion: this.selectedSede()!.direccion,
          ciudad: this.selectedSede()!.ciudad,
          telefono: this.selectedSede()!.telefono
        } : null
      };
      localStorage.setItem('checkout-shipping', JSON.stringify(shippingData));
    } catch (error) {
      console.error('Error saving shipping data to localStorage:', error);
      // Continuar sin guardar si hay error (puede ser por espacio insuficiente)
    }
  }

  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }
}


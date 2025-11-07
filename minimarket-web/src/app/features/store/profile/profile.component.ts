import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService, PaymentMethod, UserProfile, UserAddress } from '../../../core/services/auth.service';
import { OrdersService, WebOrder } from '../../../core/services/orders.service';
import { PaymentMethodSettingsService, PaymentMethodSetting } from '../../../core/services/payment-method-settings.service';
import { StoreHeaderComponent } from '../../../shared/components/store-header/store-header.component';
import { OrderStatusTrackerComponent } from '../../../shared/components/order-status-tracker/order-status-tracker.component';
import { ToastService } from '../../../shared/services/toast.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterModule, StoreHeaderComponent, ReactiveFormsModule, OrderStatusTrackerComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  currentUser = computed(() => this.authService.currentUser());
  orders = signal<WebOrder[]>([]);
  isLoading = signal(false);
  activeSection = signal<'dashboard' | 'orders' | 'personal' | 'addresses' | 'payment'>('dashboard');
  recentOrder = signal<WebOrder | null>(null);
  selectedOrder = signal<WebOrder | null>(null);
  isLoadingOrderDetails = signal(false);
  showOrderDetails = signal(false);
  
  // Datos personales
  profileForm: FormGroup;
  isSavingProfile = signal(false);
  userProfile = signal<UserProfile | null>(null);
  isLoadingProfile = signal(false);
  
  // Métodos de pago
  paymentMethods = signal<PaymentMethod[]>([]);
  paymentMethodSettings = signal<PaymentMethodSetting[]>([]);
  isLoadingPaymentMethods = signal(false);
  isLoadingPaymentMethodSettings = signal(false);
  isAddingPaymentMethod = signal(false);
  isEditingPaymentMethod = signal<string | null>(null);
  paymentMethodForm: FormGroup;
  showPaymentMethodForm = signal(false);

  // Direcciones de envío
  addresses = signal<UserAddress[]>([]);
  isLoadingAddresses = signal(false);
  isAddingAddress = signal(false);
  isEditingAddress = signal<string | null>(null);
  addressForm: FormGroup;
  showAddressForm = signal(false);

  constructor(
    private authService: AuthService,
    private ordersService: OrdersService,
    private paymentMethodSettingsService: PaymentMethodSettingsService,
    private router: Router,
    private fb: FormBuilder,
    private toastService: ToastService
  ) {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      dni: [{value: '', disabled: true}, [Validators.required, Validators.pattern(/^\d{8}$/)]],
      email: [{value: '', disabled: true}, [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^\+?[0-9\s\-\(\)]+$/), Validators.maxLength(20)]]
    });
    
    this.paymentMethodForm = this.fb.group({
      cardHolderName: ['', [Validators.required, Validators.maxLength(100)]],
      cardNumber: ['', [Validators.required, Validators.pattern(/^\d{13,19}$/)]],
      expiryMonth: [new Date().getMonth() + 1, [Validators.required, Validators.min(1), Validators.max(12)]],
      expiryYear: [new Date().getFullYear(), [Validators.required, Validators.min(new Date().getFullYear())]],
      isDefault: [false]
    });

    this.addressForm = this.fb.group({
      label: ['', [Validators.required, Validators.maxLength(50)]],
      isDifferentRecipient: [false],
      fullName: ['', [Validators.maxLength(200)]],
      firstName: ['', [Validators.maxLength(100)]],
      lastName: ['', [Validators.maxLength(100)]],
      dni: ['', [Validators.pattern(/^\d{8}$/)]],
      phone: ['', [Validators.pattern(/^\+?[0-9\s\-\(\)]+$/), Validators.maxLength(20)]],
      address: ['', [Validators.required, Validators.maxLength(500)]],
      reference: ['', [Validators.maxLength(500)]],
      district: ['', [Validators.required, Validators.maxLength(100)]],
      city: ['', [Validators.required, Validators.maxLength(100)]],
      region: ['', [Validators.required, Validators.maxLength(100)]],
      postalCode: ['', [Validators.maxLength(20)]],
      latitude: [null],
      longitude: [null],
      isDefault: [false]
    });
  }

  ngOnInit() {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.loadUserOrders();
    this.loadUserProfile();
    this.loadAddresses();
  }

  async loadUserOrders() {
    this.isLoading.set(true);
    try {
      // El endpoint obtiene automáticamente el email del usuario autenticado
      const orders = await firstValueFrom(this.ordersService.getUserOrders());
      this.orders.set(orders);
      
      // Si hay pedidos, obtener el más reciente
      if (orders.length > 0) {
        this.recentOrder.set(orders[0]);
      }
    } catch (error) {
      console.error('Error loading orders:', error);
      // Si el endpoint no existe aún o hay error, dejar la lista vacía
      this.orders.set([]);
    } finally {
      this.isLoading.set(false);
    }
  }

  setActiveSection(section: 'dashboard' | 'orders' | 'personal' | 'addresses' | 'payment') {
    this.activeSection.set(section);
  }

  getButtonClasses(section: 'dashboard' | 'orders' | 'personal' | 'addresses' | 'payment'): string {
    const baseClasses = 'flex items-center gap-3 px-3 py-2.5 rounded-lg hover:bg-primary/10 transition-colors';
    const activeClasses = 'bg-primary/20 text-primary';
    return this.activeSection() === section 
      ? `${baseClasses} ${activeClasses}` 
      : baseClasses;
  }

  shouldShowStatusTracker(status: string): boolean {
    // Mostrar el tracker para estados en curso (confirmado, preparando, en camino, listo para retiro)
    // También incluir variaciones que el admin pueda usar
    const normalizedStatus = status?.toLowerCase().trim() || '';
    const activeStates = [
      'confirmed', 'preparing', 'shipped', 'ready_for_pickup',
      'in_progress', 'processing', 'on_way', 'out_for_delivery',
      'ready', 'available'
    ];
    
    // No mostrar para estados finales o cancelados
    const finalStates = ['delivered', 'cancelled', 'completed', 'finished', 'anulado'];
    if (finalStates.some(s => normalizedStatus.includes(s))) {
      return false;
    }
    
    return activeStates.some(s => normalizedStatus.includes(s)) || 
           (normalizedStatus && !finalStates.some(s => normalizedStatus.includes(s)));
  }

  getOrderStatusClass(status: string): string {
    const statusMap: { [key: string]: string } = {
      'pending': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300',
      'confirmed': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
      'preparing': 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300',
      'shipped': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
      'delivered': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
      'ready_for_pickup': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
      'cancelled': 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300'
    };
    return statusMap[status] || 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300';
  }

  getOrderStatusText(status: string): string {
    const statusMap: { [key: string]: string } = {
      'pending': 'Pendiente',
      'confirmed': 'Confirmado',
      'preparing': 'Preparando',
      'shipped': 'En Camino',
      'delivered': 'Entregado',
      'ready_for_pickup': 'Listo para Retiro',
      'cancelled': 'Cancelado'
    };
    return statusMap[status] || status;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-PE', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  formatPrice(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  // Gestión de perfil
  async loadUserProfile() {
    this.isLoadingProfile.set(true);
    try {
      const profile = await firstValueFrom(this.authService.getProfile());
      this.userProfile.set(profile);
      
      // Cargar datos en el formulario
      this.profileForm.patchValue({
        firstName: profile.firstName || '',
        lastName: profile.lastName || '',
        dni: profile.dni || '',
        email: profile.email || '',
        phone: profile.phone || ''
      });
    } catch (error) {
      console.error('Error loading user profile:', error);
      // Si no hay perfil, dejar los campos vacíos
      this.userProfile.set(null);
    } finally {
      this.isLoadingProfile.set(false);
    }
  }

  async saveProfile() {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isSavingProfile.set(true);
    try {
      const profileData = {
        firstName: this.profileForm.get('firstName')?.value,
        lastName: this.profileForm.get('lastName')?.value,
        phone: this.profileForm.get('phone')?.value
        // DNI y email no se envían porque están bloqueados
      };

      await firstValueFrom(this.authService.updateProfile(profileData));
      this.toastService.success('Perfil actualizado exitosamente');
      
      // Recargar el perfil actualizado
      await this.loadUserProfile();
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Error al actualizar el perfil');
    } finally {
      this.isSavingProfile.set(false);
    }
  }

  // Gestión de métodos de pago
  async loadPaymentMethods() {
    this.isLoadingPaymentMethods.set(true);
    try {
      const methods = await firstValueFrom(this.authService.getPaymentMethods());
      this.paymentMethods.set(methods);
    } catch (error) {
      console.error('Error loading payment methods:', error);
      this.paymentMethods.set([]);
    } finally {
      this.isLoadingPaymentMethods.set(false);
    }
  }

  async loadPaymentMethodSettings() {
    this.isLoadingPaymentMethodSettings.set(true);
    try {
      const settings = await firstValueFrom(this.paymentMethodSettingsService.getAll(true)); // Solo habilitados
      this.paymentMethodSettings.set(settings);
    } catch (error) {
      console.error('Error loading payment method settings:', error);
      this.paymentMethodSettings.set([]);
    } finally {
      this.isLoadingPaymentMethodSettings.set(false);
    }
  }

  showAddPaymentMethodForm() {
    this.paymentMethodForm.reset({
      expiryMonth: new Date().getMonth() + 1,
      expiryYear: new Date().getFullYear(),
      isDefault: this.paymentMethods().length === 0
    });
    this.showPaymentMethodForm.set(true);
    this.isEditingPaymentMethod.set(null);
  }

  cancelPaymentMethodForm() {
    this.showPaymentMethodForm.set(false);
    this.isEditingPaymentMethod.set(null);
    this.paymentMethodForm.reset();
    this.paymentMethodForm.get('cardNumber')?.enable(); // Habilitar el campo al cancelar
  }

  async savePaymentMethod() {
    if (this.paymentMethodForm.invalid) {
      this.paymentMethodForm.markAllAsTouched();
      return;
    }

    this.isAddingPaymentMethod.set(true);
    try {
      const formValue = this.paymentMethodForm.value;
      const cardNumber = formValue.cardNumber.replace(/\s/g, '').replace(/-/g, '');
      
      if (this.isEditingPaymentMethod()) {
        // Actualizar método existente
        await firstValueFrom(this.authService.updatePaymentMethod(
          this.isEditingPaymentMethod()!,
          {
            cardHolderName: formValue.cardHolderName,
            expiryMonth: formValue.expiryMonth,
            expiryYear: formValue.expiryYear,
            isDefault: formValue.isDefault
          }
        ));
        this.toastService.success('Método de pago actualizado exitosamente');
      } else {
        // Agregar nuevo método
        await firstValueFrom(this.authService.addPaymentMethod({
          cardHolderName: formValue.cardHolderName,
          cardNumber: cardNumber,
          expiryMonth: formValue.expiryMonth,
          expiryYear: formValue.expiryYear,
          isDefault: formValue.isDefault
        }));
        this.toastService.success('Método de pago agregado exitosamente');
      }
      
      await this.loadPaymentMethods();
      this.cancelPaymentMethodForm();
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Error al guardar el método de pago');
    } finally {
      this.isAddingPaymentMethod.set(false);
    }
  }

  editPaymentMethod(method: PaymentMethod) {
    this.paymentMethodForm.patchValue({
      cardHolderName: method.cardHolderName,
      expiryMonth: method.expiryMonth,
      expiryYear: method.expiryYear,
      isDefault: method.isDefault
    });
    // No podemos editar el número de tarjeta por seguridad
    this.paymentMethodForm.get('cardNumber')?.disable();
    this.isEditingPaymentMethod.set(method.id);
    this.showPaymentMethodForm.set(true);
  }

  async deletePaymentMethod(id: string) {
    if (!confirm('¿Estás seguro de que deseas eliminar este método de pago?')) {
      return;
    }

    try {
      await firstValueFrom(this.authService.deletePaymentMethod(id));
      this.toastService.success('Método de pago eliminado exitosamente');
      await this.loadPaymentMethods();
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Error al eliminar el método de pago');
    }
  }

  formatCardNumber(masked: string): string {
    return masked;
  }

  formatExpiryDate(month: number, year: number): string {
    return `${String(month).padStart(2, '0')}/${year}`;
  }

  getYears(): number[] {
    const currentYear = new Date().getFullYear();
    const years: number[] = [];
    for (let i = 0; i < 20; i++) {
      years.push(currentYear + i);
    }
    return years;
  }

  formatMonth(month: number): string {
    return String(month).padStart(2, '0');
  }

  async viewOrderDetails(orderId: string) {
    console.log('viewOrderDetails called with orderId:', orderId);
    
    // Prevenir múltiples llamadas
    if (this.isLoadingOrderDetails()) {
      console.log('Already loading order details, skipping...');
      return;
    }

    this.isLoadingOrderDetails.set(true);
    this.showOrderDetails.set(true);
    
    try {
      console.log('Fetching order details for:', orderId);
      const order = await firstValueFrom(this.ordersService.getOrderById(orderId));
      console.log('Order details received:', order);
      
      if (!order) {
        throw new Error('No se recibieron datos del pedido');
      }

      this.selectedOrder.set(order);
      console.log('Selected order set:', this.selectedOrder());
      
      // Cambiar a la sección de pedidos
      this.setActiveSection('orders');
      
      // Forzar detección de cambios
      setTimeout(() => {
        const ordersSection = document.querySelector('[data-section="orders"]');
        if (ordersSection) {
          ordersSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
        console.log('showOrderDetails signal:', this.showOrderDetails());
        console.log('selectedOrder signal:', this.selectedOrder());
      }, 100);
    } catch (error: any) {
      console.error('Error loading order details:', error);
      console.error('Error details:', {
        status: error.status,
        statusText: error.statusText,
        error: error.error,
        message: error.message
      });
      
      let errorMessage = 'Error al cargar los detalles del pedido';
      
      if (error.message) {
        errorMessage = error.message;
      } else if (error.error?.message) {
        errorMessage = error.error.message;
      } else if (error.status === 404) {
        errorMessage = 'Pedido no encontrado';
      } else if (error.status === 403) {
        errorMessage = 'No tienes permiso para ver este pedido';
      } else if (error.status === 401) {
        errorMessage = 'No estás autenticado. Por favor, inicia sesión.';
      }
      
      this.toastService.error(errorMessage);
      this.showOrderDetails.set(false);
      this.selectedOrder.set(null);
    } finally {
      this.isLoadingOrderDetails.set(false);
    }
  }

  closeOrderDetails() {
    this.showOrderDetails.set(false);
    this.selectedOrder.set(null);
  }

  // Gestión de direcciones de envío
  async loadAddresses() {
    this.isLoadingAddresses.set(true);
    try {
      const addresses = await firstValueFrom(this.authService.getAddresses());
      this.addresses.set(addresses);
    } catch (error) {
      console.error('Error loading addresses:', error);
      this.addresses.set([]);
    } finally {
      this.isLoadingAddresses.set(false);
    }
  }

  showAddAddressForm() {
    const profile = this.userProfile();
    this.addressForm.reset({
      isDifferentRecipient: false,
      fullName: profile ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim() : '',
      firstName: '',
      lastName: '',
      dni: '',
      phone: profile?.phone || '',
      isDefault: this.addresses().length === 0
    });
    this.updateAddressValidators();
    this.showAddressForm.set(true);
    this.isEditingAddress.set(null);
  }

  onRecipientChange() {
    this.updateAddressValidators();
    const isDifferent = this.addressForm.get('isDifferentRecipient')?.value;
    if (!isDifferent) {
      // Si no es diferente destinatario, usar datos del perfil
      const profile = this.userProfile();
      this.addressForm.patchValue({
        fullName: profile ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim() : '',
        phone: profile?.phone || ''
      });
      // Limpiar campos de destinatario diferente
      this.addressForm.patchValue({
        firstName: '',
        lastName: '',
        dni: ''
      });
    }
  }

  updateAddressValidators() {
    const isDifferent = this.addressForm.get('isDifferentRecipient')?.value;
    const fullNameControl = this.addressForm.get('fullName');
    const phoneControl = this.addressForm.get('phone');
    const firstNameControl = this.addressForm.get('firstName');
    const lastNameControl = this.addressForm.get('lastName');
    const dniControl = this.addressForm.get('dni');

    if (isDifferent) {
      // Si es diferente destinatario, requerir firstName, lastName, dni, phone
      fullNameControl?.clearValidators();
      phoneControl?.setValidators([Validators.required, Validators.pattern(/^\+?[0-9\s\-\(\)]+$/), Validators.maxLength(20)]);
      firstNameControl?.setValidators([Validators.required, Validators.maxLength(100)]);
      lastNameControl?.setValidators([Validators.required, Validators.maxLength(100)]);
      dniControl?.setValidators([Validators.required, Validators.pattern(/^\d{8}$/)]);
    } else {
      // Si no es diferente destinatario, requerir fullName y phone
      fullNameControl?.setValidators([Validators.required, Validators.maxLength(200)]);
      phoneControl?.setValidators([Validators.required, Validators.pattern(/^\+?[0-9\s\-\(\)]+$/), Validators.maxLength(20)]);
      firstNameControl?.clearValidators();
      lastNameControl?.clearValidators();
      dniControl?.clearValidators();
    }

    fullNameControl?.updateValueAndValidity();
    phoneControl?.updateValueAndValidity();
    firstNameControl?.updateValueAndValidity();
    lastNameControl?.updateValueAndValidity();
    dniControl?.updateValueAndValidity();
  }

  cancelAddressForm() {
    this.showAddressForm.set(false);
    this.isEditingAddress.set(null);
    this.addressForm.reset();
  }

  async saveAddress() {
    if (this.addressForm.invalid) {
      this.addressForm.markAllAsTouched();
      return;
    }

    this.isAddingAddress.set(true);
    try {
      const formValue = this.addressForm.value;
      const isDifferent = formValue.isDifferentRecipient;
      
      const addressData: any = {
        label: formValue.label,
        isDifferentRecipient: isDifferent,
        fullName: isDifferent ? `${formValue.firstName || ''} ${formValue.lastName || ''}`.trim() : formValue.fullName,
        firstName: isDifferent ? formValue.firstName : undefined,
        lastName: isDifferent ? formValue.lastName : undefined,
        dni: isDifferent ? formValue.dni : undefined,
        phone: formValue.phone,
        address: formValue.address,
        reference: formValue.reference || undefined,
        district: formValue.district,
        city: formValue.city,
        region: formValue.region,
        postalCode: formValue.postalCode || undefined,
        latitude: formValue.latitude || undefined,
        longitude: formValue.longitude || undefined,
        isDefault: formValue.isDefault
      };
      
      if (this.isEditingAddress()) {
        // Actualizar dirección existente
        await firstValueFrom(this.authService.updateAddress(
          this.isEditingAddress()!,
          addressData
        ));
        this.toastService.success('Dirección actualizada exitosamente');
      } else {
        // Agregar nueva dirección
        await firstValueFrom(this.authService.addAddress(addressData));
        this.toastService.success('Dirección agregada exitosamente');
      }
      
      await this.loadAddresses();
      this.cancelAddressForm();
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Error al guardar la dirección');
    } finally {
      this.isAddingAddress.set(false);
    }
  }

  editAddress(address: UserAddress) {
    this.addressForm.patchValue({
      label: address.label,
      isDifferentRecipient: address.isDifferentRecipient,
      fullName: address.fullName,
      firstName: address.firstName || '',
      lastName: address.lastName || '',
      dni: address.dni || '',
      phone: address.phone,
      address: address.address,
      reference: address.reference || '',
      district: address.district,
      city: address.city,
      region: address.region,
      postalCode: address.postalCode || '',
      latitude: address.latitude || null,
      longitude: address.longitude || null,
      isDefault: address.isDefault
    });
    this.updateAddressValidators();
    this.isEditingAddress.set(address.id);
    this.showAddressForm.set(true);
  }

  async deleteAddress(id: string) {
    if (!confirm('¿Estás seguro de que deseas eliminar esta dirección?')) {
      return;
    }

    try {
      await firstValueFrom(this.authService.deleteAddress(id));
      this.toastService.success('Dirección eliminada exitosamente');
      await this.loadAddresses();
    } catch (error: any) {
      this.toastService.error(error.error?.message || 'Error al eliminar la dirección');
    }
  }
}


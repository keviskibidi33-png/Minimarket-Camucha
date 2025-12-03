import { Component, OnInit, signal, computed, effect, DestroyRef, inject, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { SetupStatusService } from '../../../core/services/setup-status.service';
import { filter } from 'rxjs/operators';
import { SettingsNavbarComponent } from '../../../shared/components/settings-navbar/settings-navbar.component';
import { SettingsService, SystemSettings, UpdateSystemSettings } from '../../../core/services/settings.service';
import { ShippingService, ShippingRate, CreateShippingRate, UpdateShippingRate } from '../../../core/services/shipping.service';
import { BrandSettingsService, BrandSettings, UpdateBrandSettings } from '../../../core/services/brand-settings.service';
import { FilesService } from '../../../core/services/files.service';
import { ToastService } from '../../../shared/services/toast.service';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SettingsNavbarComponent, ConfirmDialogComponent],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent implements OnInit {
  settings = signal<SystemSettings[]>([]);
  isLoading = signal(false);
  activeTab = signal<'cart' | 'shipping' | 'shipping-rates' | 'payment-methods'>('cart');
  
  private readonly destroyRef = inject(DestroyRef);
  private tabPersistenceEffect?: ReturnType<typeof effect>;
  
  // Configuraciones del carrito
  applyIgvToCart = signal(false);
  
  // Configuraciones de envío
  deliveryDays = signal(3);
  deliveryTime = signal('18:00');
  pickupDays = signal(2);
  pickupTime = signal('16:00');
  
  // Configuración de IGV
  igvRate = signal(0.18); // 18% IGV
  
  // Configuraciones de tarifas de envío simplificadas
  fixedShippingPrice = signal(8.00);
  freeShippingThreshold = signal(20.00);
  

  // Configuraciones de métodos de pago y comunicación
  phone = signal('');
  whatsAppPhone = signal('');
  email = signal('');
  yapePhone = signal('');
  plinPhone = signal('');
  yapeQRUrl = signal('');
  plinQRUrl = signal('');
  yapeEnabled = signal(false);
  plinEnabled = signal(false);
  bankName = signal('');
  bankAccountType = signal('');
  bankAccountNumber = signal('');
  bankCCI = signal('');
  bankAccountVisible = signal(false);
  isUploadingYapeQR = signal(false);
  isUploadingPlinQR = signal(false);

  constructor(
    private settingsService: SettingsService,
    private shippingService: ShippingService,
    private brandSettingsService: BrandSettingsService,
    private filesService: FilesService,
    private toastService: ToastService,
    private router: Router,
    private route: ActivatedRoute,
    private setupStatusService: SetupStatusService
  ) {}

  ngOnInit(): void {
    // Leer query param para activar el tab correspondiente
    this.route.queryParams.subscribe(params => {
      const tabParam = params['tab'];
      if (tabParam) {
        const validTabs: Array<'cart' | 'shipping' | 'shipping-rates' | 'payment-methods'> = 
          ['cart', 'shipping', 'shipping-rates', 'payment-methods'];
        if (validTabs.includes(tabParam as any)) {
          this.activeTab.set(tabParam as any);
        }
      } else {
        // Si no hay query param, cargar desde localStorage
        this.initializeActiveTab();
      }
    });
    
    // Observar cambios de ruta para actualizar el tab activo
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.updateTabFromRoute();
      });

    // Actualizar tab según la ruta actual
    this.updateTabFromRoute();

    this.loadSettings();
    this.loadPaymentMethodsSettings();

    // Observar cambios en activeTab para persistir
    afterNextRender(() => {
      this.tabPersistenceEffect = effect(() => {
        const tab = this.activeTab();
        localStorage.setItem('settings_active_tab', tab);
      });
    });

    // Limpiar el effect cuando el componente se destruya (fuera del callback de afterNextRender)
    this.destroyRef.onDestroy(() => {
      this.tabPersistenceEffect?.destroy();
    });
  }

  private initializeActiveTab(): void {
    // Primero intentar detectar desde la ruta
    const url = this.router.url;
    if (url.includes('/configuraciones/permisos')) {
      // Permisos es una ruta separada, no necesita persistencia de tab
      return;
    }

    // Para las otras pestañas, cargar desde localStorage
    const savedTab = localStorage.getItem('settings_active_tab');
        const validTabs: Array<'cart' | 'shipping' | 'shipping-rates' | 'email-templates' | 'categories' | 'payment-methods'> = 
          ['cart', 'shipping', 'shipping-rates', 'email-templates', 'categories', 'payment-methods'];
    
    if (savedTab && validTabs.includes(savedTab as any)) {
      this.activeTab.set(savedTab as any);
    }
  }

  private updateTabFromRoute(): void {
    const url = this.router.url;
    // Si estamos en una ruta específica (permisos), no hacer nada
    // porque es una ruta separada
    if (url.includes('/configuraciones/permisos')) {
      return;
    }
    
    // Si estamos en /admin/configuraciones, restaurar el último tab guardado
    if (url === '/admin/configuraciones' || url.endsWith('/configuraciones')) {
      const savedTab = localStorage.getItem('settings_active_tab');
      const validTabs: Array<'cart' | 'shipping' | 'shipping-rates' | 'categories' | 'payment-methods'> = 
        ['cart', 'shipping', 'shipping-rates', 'categories', 'payment-methods'];
      
      if (savedTab && validTabs.includes(savedTab as any)) {
        this.activeTab.set(savedTab as any);
      }
    }
  }

  loadSettings(): void {
    this.isLoading.set(true);
    this.settingsService.getAll().subscribe({
      next: (settings) => {
        this.settings.set(settings);
        // Cargar configuración de IGV
        const igvSetting = settings.find(s => s.key === 'apply_igv_to_cart');
        if (igvSetting) {
          const igvEnabled = igvSetting.value === 'true' || igvSetting.value === '1';
          this.applyIgvToCart.set(igvEnabled);
        } else {
          this.applyIgvToCart.set(false);
        }
        
        const igvRateSetting = settings.find(s => s.key === 'igv_rate');
        if (igvRateSetting) {
          const rate = parseFloat(igvRateSetting.value) || 0.18;
          this.igvRate.set(rate);
        } else {
          this.igvRate.set(0.18);
        }
        
        // Cargar configuraciones de envío
        const deliveryDaysSetting = settings.find(s => s.key === 'delivery_days');
        if (deliveryDaysSetting) {
          this.deliveryDays.set(parseInt(deliveryDaysSetting.value) || 3);
        }
        
        const deliveryTimeSetting = settings.find(s => s.key === 'delivery_time');
        if (deliveryTimeSetting) {
          this.deliveryTime.set(deliveryTimeSetting.value || '18:00');
        }
        
        const pickupDaysSetting = settings.find(s => s.key === 'pickup_days');
        if (pickupDaysSetting) {
          this.pickupDays.set(parseInt(pickupDaysSetting.value) || 2);
        }
        
        const pickupTimeSetting = settings.find(s => s.key === 'pickup_time');
        if (pickupTimeSetting) {
          this.pickupTime.set(pickupTimeSetting.value || '16:00');
        }
        
        // Cargar configuraciones de tarifas de envío simplificadas
        const fixedPriceSetting = settings.find(s => s.key === 'fixed_shipping_price');
        if (fixedPriceSetting) {
          const price = parseFloat(fixedPriceSetting.value) || 8.00;
          this.fixedShippingPrice.set(price);
        }
        
        const freeShippingSetting = settings.find(s => s.key === 'free_shipping_threshold');
        if (freeShippingSetting) {
          const threshold = parseFloat(freeShippingSetting.value) || 20.00;
          this.freeShippingThreshold.set(threshold);
        }
        
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading settings:', error);
        this.toastService.error('Error al cargar configuraciones');
        this.isLoading.set(false);
      }
    });
  }


  saveCartSetting(): void {
    const currentIgvState = this.applyIgvToCart();
    const currentIgvRate = this.igvRate();

    const settingsToSave = [
      {
        key: 'apply_igv_to_cart',
        value: currentIgvState ? 'true' : 'false',
        description: 'Aplicar IGV al carrito de compras',
        isActive: true
      },
      {
        key: 'igv_rate',
        value: currentIgvRate.toString() || '0.18',
        description: 'Tasa de IGV (ej: 0.18 para 18%)',
        isActive: true
      }
    ];

    // Validar que los valores no estén vacíos
    const invalidSettings = settingsToSave.filter(s => !s.value || s.value.trim() === '');
    if (invalidSettings.length > 0) {
      this.toastService.error('Error: No se pueden guardar valores vacíos');
      return;
    }

    let savedCount = 0;
    let errorCount = 0;
    const totalSettings = settingsToSave.length;
    const responses: { [key: string]: string } = {};

    settingsToSave.forEach(setting => {
      this.settingsService.update(setting.key, setting).subscribe({
        next: (response) => {
          responses[setting.key] = response.value;
          
          // Actualizar el estado local con el valor de la respuesta
          if (setting.key === 'apply_igv_to_cart') {
            const enabled = response.value === 'true' || response.value === '1';
            this.applyIgvToCart.set(enabled);
          } else if (setting.key === 'igv_rate') {
            const rate = parseFloat(response.value) || 0.18;
            this.igvRate.set(rate);
          }
          
          savedCount++;
          
          if (savedCount === totalSettings) {
            // Validar que todas las respuestas sean válidas
            const allValid = settingsToSave.every(s => {
              const responseValue = responses[s.key];
              if (!responseValue || responseValue.trim() === '') return false;
              
              if (s.key === 'apply_igv_to_cart') {
                return responseValue === 'true' || responseValue === '1' || responseValue === 'false' || responseValue === '0';
              } else if (s.key === 'igv_rate') {
                return !isNaN(parseFloat(responseValue));
              }
              return true;
            });
            
            if (allValid) {
              this.toastService.success('Configuraciones del carrito guardadas correctamente');
            } else {
              this.toastService.warning('Configuración guardada pero algunos valores son inválidos. Recargando...');
              setTimeout(() => this.loadSettings(), 500);
            }
          }
        },
        error: () => {
          errorCount++;
          this.toastService.error(`Error al guardar ${setting.key}`);
          
          if (errorCount === totalSettings || savedCount + errorCount === totalSettings) {
            setTimeout(() => this.loadSettings(), 500);
          }
        }
      });
    });
  }


  saveShippingSettings(): void {
    const settingsToSave: UpdateSystemSettings[] = [
      {
        key: 'delivery_days',
        value: this.deliveryDays().toString(),
        description: 'Días estimados para entrega a domicilio',
        isActive: true
      },
      {
        key: 'delivery_time',
        value: this.deliveryTime(),
        description: 'Hora estimada de entrega a domicilio (formato HH:mm)',
        isActive: true
      },
      {
        key: 'pickup_days',
        value: this.pickupDays().toString(),
        description: 'Días estimados para retiro en tienda',
        isActive: true
      },
      {
        key: 'pickup_time',
        value: this.pickupTime(),
        description: 'Hora estimada para retiro en tienda (formato HH:mm)',
        isActive: true
      }
    ];

    let savedCount = 0;
    const totalSettings = settingsToSave.length;

    settingsToSave.forEach(setting => {
      this.settingsService.update(setting.key, setting).subscribe({
        next: () => {
          savedCount++;
          if (savedCount === totalSettings) {
            this.toastService.success('Configuraciones de envío guardadas correctamente');
            this.loadSettings();
          }
        },
        error: (error) => {
          console.error(`Error saving setting ${setting.key}:`, error);
          this.toastService.error(`Error al guardar ${setting.key}`);
        }
      });
    });
  }

  setTab(tab: 'cart' | 'shipping' | 'shipping-rates' | 'payment-methods'): void {
    this.activeTab.set(tab);
    // Actualizar la URL con el query param para mantener consistencia
    this.router.navigate(['/admin/configuraciones'], { queryParams: { tab } });
  }
  

  saveShippingRatesSettings(): void {
    const settingsToSave = [
      {
        key: 'fixed_shipping_price',
        value: this.fixedShippingPrice().toString(),
        description: 'Precio fijo de envío para Lima',
        isActive: true
      },
      {
        key: 'free_shipping_threshold',
        value: this.freeShippingThreshold().toString(),
        description: 'Monto mínimo para envío gratis',
        isActive: true
      }
    ];

    let savedCount = 0;
    let errorCount = 0;
    const totalSettings = settingsToSave.length;

    settingsToSave.forEach(setting => {
      this.settingsService.update(setting.key, setting).subscribe({
        next: () => {
          savedCount++;
          if (savedCount === totalSettings) {
            this.toastService.success('Configuraciones de tarifas de envío guardadas correctamente');
            setTimeout(() => {
              this.loadSettings();
            }, 300);
          }
        },
        error: () => {
          errorCount++;
          this.toastService.error(`Error al guardar ${setting.key}`);
        }
      });
    });
  }


  // Helper methods for template
  parseInt = parseInt;
  parseFloat = parseFloat;

  loadPaymentMethodsSettings(): void {
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        if (settings) {
          this.phone.set(settings.phone || '');
          this.whatsAppPhone.set(settings.whatsAppPhone || '');
          this.email.set(settings.email || '');
          // Cargar Yape (unificado con Plin)
          this.yapePhone.set(settings.yapePhone || settings.plinPhone || '');
          this.yapeQRUrl.set(settings.yapeQRUrl || settings.plinQRUrl || '');
          this.yapeEnabled.set(settings.yapeEnabled ?? settings.plinEnabled ?? false);
          // Plin se sincroniza automáticamente con Yape al guardar
          this.bankName.set(settings.bankName || '');
          this.bankAccountType.set(settings.bankAccountType || '');
          this.bankAccountNumber.set(settings.bankAccountNumber || '');
          this.bankCCI.set(settings.bankCCI || '');
          this.bankAccountVisible.set(settings.bankAccountVisible ?? false);
        }
      },
      error: (error) => {
        console.error('Error loading payment methods settings:', error);
      }
    });
  }

  savePaymentMethodsSettings(): void {
    // Obtener BrandSettings actual para mantener otros campos
    this.brandSettingsService.get().subscribe({
      next: (currentSettings) => {
        const updateData: UpdateBrandSettings = {
          // Mantener campos de marca visual (no se editan aquí)
          logoUrl: currentSettings?.logoUrl || '',
          logoEmoji: currentSettings?.logoEmoji,
          storeName: currentSettings?.storeName || 'Minimarket Camucha',
          faviconUrl: currentSettings?.faviconUrl,
          primaryColor: currentSettings?.primaryColor || '#4CAF50',
          secondaryColor: currentSettings?.secondaryColor || '#0d7ff2',
          buttonColor: currentSettings?.buttonColor || '#4CAF50',
          textColor: currentSettings?.textColor || '#333333',
          hoverColor: currentSettings?.hoverColor || '#45a049',
          description: currentSettings?.description,
          slogan: currentSettings?.slogan,
          // Campos de comunicación y métodos de pago (editables)
          phone: this.phone().trim() || '',
          whatsAppPhone: this.whatsAppPhone().trim() || '',
          email: this.email().trim() || '',
          yapePhone: this.yapePhone().trim() || '',
          plinPhone: this.yapePhone().trim() || '', // Unificado: usar el mismo número de Yape
          yapeQRUrl: this.yapeQRUrl().trim() || '',
          plinQRUrl: this.yapeQRUrl().trim() || '', // Unificado: usar el mismo QR de Yape
          yapeEnabled: this.yapeEnabled(),
          plinEnabled: this.yapeEnabled(), // Unificado: mismo estado que Yape
          bankName: this.bankName().trim() || '',
          bankAccountType: this.bankAccountType().trim() || '',
          bankAccountNumber: this.bankAccountNumber().trim() || '',
          bankCCI: this.bankCCI().trim() || '',
          bankAccountVisible: this.bankAccountVisible(),
          // Mantener otros campos
          ruc: currentSettings?.ruc || '',
          address: currentSettings?.address || '',
          deliveryType: currentSettings?.deliveryType || 'Ambos',
          deliveryCost: currentSettings?.deliveryCost,
          deliveryZones: currentSettings?.deliveryZones || '',
          homeTitle: currentSettings?.homeTitle || '',
          homeSubtitle: currentSettings?.homeSubtitle || '',
          homeDescription: currentSettings?.homeDescription || '',
          homeBannerImageUrl: currentSettings?.homeBannerImageUrl || ''
        };

        this.brandSettingsService.update(updateData).subscribe({
          next: () => {
            this.toastService.success('Configuración de métodos de pago guardada correctamente');
            this.loadPaymentMethodsSettings();
          },
          error: (error) => {
            console.error('Error saving payment methods settings:', error);
            this.toastService.error('Error al guardar la configuración de métodos de pago');
          }
        });
      },
      error: (error) => {
        console.error('Error getting current brand settings:', error);
        this.toastService.error('Error al cargar la configuración actual');
      }
    });
  }

  onYapeQRSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    
    if (!file) {
      return;
    }
    
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      this.toastService.error('Tipo de archivo no permitido. Solo se permiten imágenes (JPG, PNG, WEBP)');
      return;
    }
    
    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      this.toastService.error('El archivo excede el tamaño máximo de 5MB');
      return;
    }
    
    this.isUploadingYapeQR.set(true);
    
    this.filesService.uploadFile(file, 'payment-qr').subscribe({
      next: (response) => {
        console.log('QR de Yape subido exitosamente. URL:', response.url);
        if (response.url) {
          this.yapeQRUrl.set(response.url);
          this.isUploadingYapeQR.set(false);
          this.toastService.success('QR de Yape subido exitosamente');
        } else {
          console.error('La respuesta no contiene URL válida:', response);
          this.isUploadingYapeQR.set(false);
          this.toastService.error('Error: No se recibió una URL válida del servidor');
        }
        input.value = '';
      },
      error: (error) => {
        console.error('Error uploading Yape QR:', error);
        this.isUploadingYapeQR.set(false);
        let errorMessage = 'Error al subir el QR de Yape';
        if (error?.error?.error) {
          errorMessage = error.error.error;
        } else if (error?.error?.message) {
          errorMessage = error.error.message;
        } else if (error?.message) {
          errorMessage = error.message;
        }
        this.toastService.error(errorMessage);
        input.value = '';
      }
    });
  }

  onPlinQRSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    
    if (!file) {
      return;
    }
    
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      this.toastService.error('Tipo de archivo no permitido. Solo se permiten imágenes (JPG, PNG, WEBP)');
      return;
    }
    
    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      this.toastService.error('El archivo excede el tamaño máximo de 5MB');
      return;
    }
    
    this.isUploadingPlinQR.set(true);
    
    this.filesService.uploadFile(file, 'payment-qr').subscribe({
      next: (response) => {
        console.log('QR de Plin subido exitosamente. URL:', response.url);
        if (response.url) {
          this.plinQRUrl.set(response.url);
          this.isUploadingPlinQR.set(false);
          this.toastService.success('QR de Plin subido exitosamente');
        } else {
          console.error('La respuesta no contiene URL válida:', response);
          this.isUploadingPlinQR.set(false);
          this.toastService.error('Error: No se recibió una URL válida del servidor');
        }
        input.value = '';
      },
      error: (error) => {
        console.error('Error uploading Plin QR:', error);
        this.isUploadingPlinQR.set(false);
        let errorMessage = 'Error al subir el QR de Plin';
        if (error?.error?.error) {
          errorMessage = error.error.error;
        } else if (error?.error?.message) {
          errorMessage = error.error.message;
        } else if (error?.message) {
          errorMessage = error.message;
        }
        this.toastService.error(errorMessage);
        input.value = '';
      }
    });
  }

  onImageError(event: Event, type: 'yape' | 'plin' = 'yape'): void {
    const img = event.target as HTMLImageElement | null;
    if (img) {
      console.error(`Error al cargar imagen de ${type}. URL:`, img.src);
      // Ocultar la imagen si falla
      img.style.display = 'none';
      this.toastService.error(`Error al cargar la imagen de ${type === 'yape' ? 'Yape' : 'Plin'}. Verifique la URL.`);
    }
  }

  onImageLoad(event: Event): void {
    const img = event.target as HTMLImageElement | null;
    if (img) {
      console.log('Imagen cargada exitosamente:', img.src);
    }
  }
}


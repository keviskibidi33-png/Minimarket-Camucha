import { Component, OnInit, signal, computed, effect, DestroyRef, inject, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { SetupStatusService } from '../../../core/services/setup-status.service';
import { filter } from 'rxjs/operators';
import { SettingsNavbarComponent } from '../../../shared/components/settings-navbar/settings-navbar.component';
import { SettingsService, SystemSettings, UpdateSystemSettings } from '../../../core/services/settings.service';
import { ShippingService, ShippingRate, CreateShippingRate, UpdateShippingRate } from '../../../core/services/shipping.service';
import { EmailTemplatesService, UpdateEmailTemplateSettings } from '../../../core/services/email-templates.service';
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
  activeTab = signal<'cart' | 'shipping' | 'shipping-rates' | 'email-templates'>('cart');
  
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
  
  // Configuraciones de templates de email
  emailLogoUrl = signal('');
  emailPromotionImageUrl = signal('');

  constructor(
    private settingsService: SettingsService,
    private shippingService: ShippingService,
    private emailTemplatesService: EmailTemplatesService,
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
        const validTabs: Array<'cart' | 'shipping' | 'shipping-rates' | 'email-templates'> = 
          ['cart', 'shipping', 'shipping-rates', 'email-templates'];
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
    this.loadEmailTemplateSettings();

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
    if (url.includes('/configuraciones/marca')) {
      // Marca y Permisos son rutas separadas, no necesitan persistencia de tab
      return;
    }
    if (url.includes('/configuraciones/permisos')) {
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
    // Si estamos en una ruta específica (marca o permisos), no hacer nada
    // porque esas son rutas separadas
    if (url.includes('/configuraciones/marca') || url.includes('/configuraciones/permisos')) {
      return;
    }
    
    // Si estamos en /admin/configuraciones, restaurar el último tab guardado
    if (url === '/admin/configuraciones' || url.endsWith('/configuraciones')) {
      const savedTab = localStorage.getItem('settings_active_tab');
      const validTabs: Array<'cart' | 'shipping' | 'shipping-rates' | 'email-templates' | 'categories' | 'payment-methods'> = 
        ['cart', 'shipping', 'shipping-rates', 'email-templates', 'categories', 'payment-methods'];
      
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

  setTab(tab: 'cart' | 'shipping' | 'shipping-rates' | 'email-templates'): void {
    this.activeTab.set(tab);
    // Actualizar la URL con el query param para mantener consistencia
    this.router.navigate(['/admin/configuraciones'], { queryParams: { tab } });
  }
  
  // Email Templates Management
  loadEmailTemplateSettings(): void {
    this.emailTemplatesService.getTemplate('order_confirmation').subscribe({
      next: (template) => {
        this.emailLogoUrl.set(template.logoUrl);
        this.emailPromotionImageUrl.set(template.promotionImageUrl);
      },
      error: (error: any) => {
        // Solo loguear si no es un 404 (endpoint no existe aún)
        if (error.status !== 404) {
          console.error('Error loading email template settings:', error);
        }
        // Establecer valores por defecto si el endpoint no existe
        this.emailLogoUrl.set('');
        this.emailPromotionImageUrl.set('');
      }
    });
  }

  saveEmailTemplateSettings(): void {
    const settings: UpdateEmailTemplateSettings = {
      logoUrl: this.emailLogoUrl(),
      promotionImageUrl: this.emailPromotionImageUrl()
    };

    this.emailTemplatesService.updateSettings(settings).subscribe({
      next: () => {
        this.toastService.success('Configuración de templates de email guardada correctamente');
      },
      error: (error) => {
        console.error('Error saving email template settings:', error);
        this.toastService.error('Error al guardar configuración de templates de email');
      }
    });
  }

  testConfirmationEmail(): void {
    const testEmail = prompt('Ingresa tu correo electrónico para recibir el email de prueba:');
    if (!testEmail) return;

    this.emailTemplatesService.sendTestConfirmationEmail({
      email: testEmail,
      customerName: 'Cliente de Prueba',
      orderNumber: 'TEST-001',
      total: 150.00,
      shippingMethod: 'delivery',
      estimatedDelivery: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString()
    }).subscribe({
      next: (result: any) => {
        if (result.sent) {
          this.toastService.success('Email de prueba enviado exitosamente. Revisa tu bandeja de entrada.');
        } else {
          this.toastService.error('Error al enviar el email de prueba');
        }
      },
      error: (error: any) => {
        console.error('Error sending test email:', error);
        this.toastService.error('Error al enviar el email de prueba');
      }
    });
  }

  testStatusUpdateEmail(): void {
    const testEmail = prompt('Ingresa tu correo electrónico para recibir el email de prueba:');
    if (!testEmail) return;

    this.emailTemplatesService.sendTestStatusUpdateEmail({
      email: testEmail,
      customerName: 'Cliente de Prueba',
      orderNumber: 'TEST-001',
      status: 'preparing'
    }).subscribe({
      next: (result: any) => {
        if (result.sent) {
          this.toastService.success('Email de prueba enviado exitosamente. Revisa tu bandeja de entrada.');
        } else {
          this.toastService.error('Error al enviar el email de prueba');
        }
      },
      error: (error: any) => {
        console.error('Error sending test email:', error);
        this.toastService.error('Error al enviar el email de prueba');
      }
    });
  }

  // Test email
  testEmailAddress = signal('');
  testTemplateType = signal<'order_confirmation' | 'order_status_update'>('order_confirmation');
  isSendingTestEmail = signal(false);

  sendTestEmail(): void {
    if (!this.testEmailAddress().trim()) {
      this.toastService.error('Por favor ingresa un correo electrónico');
      return;
    }

    this.isSendingTestEmail.set(true);
    this.emailTemplatesService.sendTestEmail({
      email: this.testEmailAddress(),
      templateType: this.testTemplateType()
    }).subscribe({
      next: (result) => {
        this.isSendingTestEmail.set(false);
        if (result.sent) {
          this.toastService.success('Correo de prueba enviado correctamente. Revisa tu bandeja de entrada.');
        } else {
          this.toastService.error('Error al enviar el correo de prueba');
        }
      },
      error: (error) => {
        this.isSendingTestEmail.set(false);
        console.error('Error sending test email:', error);
        this.toastService.error('Error al enviar el correo de prueba. Verifica la configuración de email.');
      }
    });
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

  showResetWarning = signal(false);

  goToFullSetup(): void {
    // Mostrar advertencia antes de ir al setup
    this.showResetWarning.set(true);
  }

  onResetWarningConfirmed(): void {
    this.showResetWarning.set(false);
    // Marcar setup como incompleto para forzar el guard
    this.setupStatusService.markSetupIncomplete();
    // Navegar con parámetro reset=true para limpiar datos guardados incompletos
    this.router.navigate(['/auth/admin-setup'], { queryParams: { reset: 'true' } });
  }

  onResetWarningCancelled(): void {
    this.showResetWarning.set(false);
  }
}


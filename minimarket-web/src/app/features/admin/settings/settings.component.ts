import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SettingsService, SystemSettings, UpdateSystemSettings } from '../../../core/services/settings.service';
import { CategoriesService } from '../../../core/services/categories.service';
import { ShippingService, ShippingRate, CreateShippingRate, UpdateShippingRate } from '../../../core/services/shipping.service';
import { EmailTemplatesService, UpdateEmailTemplateSettings } from '../../../core/services/email-templates.service';
import { PaymentMethodSettingsService, PaymentMethodSetting, UpdatePaymentMethodSetting } from '../../../core/services/payment-method-settings.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent implements OnInit {
  settings = signal<SystemSettings[]>([]);
  categories = signal<any[]>([]);
  isLoading = signal(false);
  activeTab = signal<'cart' | 'shipping' | 'shipping-rates' | 'email-templates' | 'banners' | 'categories' | 'payment-methods'>('cart');
  
  // Configuraciones del carrito
  applyIgvToCart = signal(false);
  
  // Configuraciones de envío
  deliveryDays = signal(3);
  deliveryTime = signal('18:00');
  pickupDays = signal(2);
  pickupTime = signal('16:00');
  
  // Configuración de IGV
  igvRate = signal(0.18); // 18% IGV
  
  // Configuraciones de categorías
  selectedCategory = signal<string | null>(null);
  categoryImageUrl = signal('');
  
  // Configuraciones de tarifas de envío
  shippingRates = signal<ShippingRate[]>([]);
  showShippingRateForm = signal(false);
  editingShippingRate = signal<ShippingRate | null>(null);
  
  // Form data para tarifas
  zoneName = signal('');
  basePrice = signal(0);
  pricePerKm = signal(0);
  pricePerKg = signal(0);
  minDistance = signal(0);
  maxDistance = signal(0);
  minWeight = signal(0);
  maxWeight = signal(0);
  freeShippingThreshold = signal(0);
  isActiveRate = signal(true);
  
  // Configuraciones de templates de email
  emailLogoUrl = signal('');
  emailPromotionImageUrl = signal('');
  
  // Configuraciones de métodos de pago
  paymentMethodSettings = signal<PaymentMethodSetting[]>([]);
  isLoadingPaymentMethods = signal(false);
  
  // Computed para obtener el nombre de la categoría seleccionada
  selectedCategoryName = computed(() => {
    const categoryId = this.selectedCategory();
    if (!categoryId) return '';
    const category = this.categories().find(c => c.id === categoryId);
    return category?.name || '';
  });

  constructor(
    private settingsService: SettingsService,
    private categoriesService: CategoriesService,
    private shippingService: ShippingService,
    private emailTemplatesService: EmailTemplatesService,
    private paymentMethodSettingsService: PaymentMethodSettingsService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadSettings();
    this.loadCategories();
    this.loadShippingRates();
    this.loadEmailTemplateSettings();
    this.loadPaymentMethodSettings();
  }

  loadSettings(): void {
    this.isLoading.set(true);
    this.settingsService.getAll().subscribe({
      next: (settings) => {
        this.settings.set(settings);
        // Cargar configuración de IGV
        const igvSetting = settings.find(s => s.key === 'apply_igv_to_cart');
        if (igvSetting) {
          this.applyIgvToCart.set(igvSetting.value === 'true' || igvSetting.value === '1');
        }
        
        const igvRateSetting = settings.find(s => s.key === 'igv_rate');
        if (igvRateSetting) {
          this.igvRate.set(parseFloat(igvRateSetting.value) || 0.18);
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
        
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading settings:', error);
        this.toastService.error('Error al cargar configuraciones');
        this.isLoading.set(false);
      }
    });
  }

  loadCategories(): void {
    this.categoriesService.getAll().subscribe({
      next: (categories) => {
        this.categories.set(categories);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  saveCartSetting(): void {
    const settingsToSave = [
      {
        key: 'apply_igv_to_cart',
        value: this.applyIgvToCart() ? 'true' : 'false',
        description: 'Aplicar IGV al carrito de compras',
        isActive: true
      },
      {
        key: 'igv_rate',
        value: this.igvRate().toString(),
        description: 'Tasa de IGV (ej: 0.18 para 18%)',
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
            this.toastService.success('Configuraciones del carrito guardadas correctamente');
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

  onCategorySelected(categoryId: string): void {
    this.selectedCategory.set(categoryId);
    const category = this.categories().find(c => c.id === categoryId);
    if (category) {
      this.categoryImageUrl.set(category.imageUrl || '');
    }
  }

  saveCategoryImage(): void {
    const categoryId = this.selectedCategory();
    if (!categoryId) {
      this.toastService.error('Selecciona una categoría');
      return;
    }

    const category = this.categories().find(c => c.id === categoryId);
    if (!category) {
      return;
    }

    const updateDto = {
      name: category.name,
      description: category.description,
      imageUrl: this.categoryImageUrl(),
      isActive: category.isActive
    };

    this.categoriesService.update(categoryId, updateDto).subscribe({
      next: () => {
        this.toastService.success('Imagen de categoría guardada correctamente');
        this.loadCategories();
      },
      error: (error) => {
        console.error('Error saving category image:', error);
        this.toastService.error('Error al guardar imagen de categoría');
      }
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

  setTab(tab: 'cart' | 'shipping' | 'shipping-rates' | 'email-templates' | 'banners' | 'categories' | 'payment-methods'): void {
    this.activeTab.set(tab);
  }
  
  // Email Templates Management
  loadEmailTemplateSettings(): void {
    this.emailTemplatesService.getTemplate('order_confirmation').subscribe({
      next: (template) => {
        this.emailLogoUrl.set(template.logoUrl);
        this.emailPromotionImageUrl.set(template.promotionImageUrl);
      },
      error: (error) => {
        console.error('Error loading email template settings:', error);
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

  // Shipping Rates Management
  loadShippingRates(): void {
    this.shippingService.getAllRates().subscribe({
      next: (rates) => {
        this.shippingRates.set(rates);
      },
      error: (error) => {
        console.error('Error loading shipping rates:', error);
        this.toastService.error('Error al cargar tarifas de envío');
      }
    });
  }

  openShippingRateForm(rate?: ShippingRate): void {
    if (rate) {
      this.editingShippingRate.set(rate);
      this.zoneName.set(rate.zoneName);
      this.basePrice.set(rate.basePrice);
      this.pricePerKm.set(rate.pricePerKm);
      this.pricePerKg.set(rate.pricePerKg);
      this.minDistance.set(rate.minDistance);
      this.maxDistance.set(rate.maxDistance);
      this.minWeight.set(rate.minWeight);
      this.maxWeight.set(rate.maxWeight);
      this.freeShippingThreshold.set(rate.freeShippingThreshold);
      this.isActiveRate.set(rate.isActive);
    } else {
      this.resetShippingRateForm();
    }
    this.showShippingRateForm.set(true);
  }

  closeShippingRateForm(): void {
    this.showShippingRateForm.set(false);
    this.editingShippingRate.set(null);
    this.resetShippingRateForm();
  }

  resetShippingRateForm(): void {
    this.zoneName.set('');
    this.basePrice.set(0);
    this.pricePerKm.set(0);
    this.pricePerKg.set(0);
    this.minDistance.set(0);
    this.maxDistance.set(0);
    this.minWeight.set(0);
    this.maxWeight.set(0);
    this.freeShippingThreshold.set(0);
    this.isActiveRate.set(true);
  }

  saveShippingRate(): void {
    if (!this.zoneName().trim()) {
      this.toastService.error('El nombre de la zona es requerido');
      return;
    }

    const rateData: CreateShippingRate | UpdateShippingRate = {
      zoneName: this.zoneName(),
      basePrice: this.basePrice(),
      pricePerKm: this.pricePerKm(),
      pricePerKg: this.pricePerKg(),
      minDistance: this.minDistance(),
      maxDistance: this.maxDistance(),
      minWeight: this.minWeight(),
      maxWeight: this.maxWeight(),
      freeShippingThreshold: this.freeShippingThreshold(),
      isActive: this.isActiveRate()
    };

    const rate = this.editingShippingRate();
    if (rate) {
      this.shippingService.updateRate(rate.id, rateData).subscribe({
        next: () => {
          this.toastService.success('Tarifa de envío actualizada correctamente');
          this.loadShippingRates();
          this.closeShippingRateForm();
        },
        error: (error) => {
          console.error('Error updating shipping rate:', error);
          this.toastService.error('Error al actualizar tarifa de envío');
        }
      });
    } else {
      this.shippingService.createRate(rateData).subscribe({
        next: () => {
          this.toastService.success('Tarifa de envío creada correctamente');
          this.loadShippingRates();
          this.closeShippingRateForm();
        },
        error: (error) => {
          console.error('Error creating shipping rate:', error);
          this.toastService.error('Error al crear tarifa de envío');
        }
      });
    }
  }

  deleteShippingRate(rate: ShippingRate): void {
    if (confirm(`¿Está seguro de eliminar la tarifa "${rate.zoneName}"?`)) {
      this.shippingService.deleteRate(rate.id).subscribe({
        next: () => {
          this.toastService.success('Tarifa de envío eliminada correctamente');
          this.loadShippingRates();
        },
        error: (error) => {
          console.error('Error deleting shipping rate:', error);
          this.toastService.error('Error al eliminar tarifa de envío');
        }
      });
    }
  }

  // Métodos de pago
  loadPaymentMethodSettings(): void {
    this.isLoadingPaymentMethods.set(true);
    this.paymentMethodSettingsService.getAll().subscribe({
      next: (settings) => {
        this.paymentMethodSettings.set(settings);
        this.isLoadingPaymentMethods.set(false);
      },
      error: (error) => {
        console.error('Error loading payment method settings:', error);
        this.toastService.error('Error al cargar métodos de pago');
        this.isLoadingPaymentMethods.set(false);
      }
    });
  }

  togglePaymentMethod(setting: PaymentMethodSetting): void {
    const update: UpdatePaymentMethodSetting = {
      isEnabled: !setting.isEnabled,
      displayOrder: setting.displayOrder,
      description: setting.description
    };

    this.paymentMethodSettingsService.update(setting.id, update).subscribe({
      next: (updated) => {
        const settings = this.paymentMethodSettings();
        const index = settings.findIndex(s => s.id === setting.id);
        if (index !== -1) {
          settings[index] = updated;
          this.paymentMethodSettings.set([...settings]);
        }
        this.toastService.success(`Método de pago ${updated.isEnabled ? 'habilitado' : 'deshabilitado'} correctamente`);
      },
      error: (error) => {
        console.error('Error updating payment method setting:', error);
        this.toastService.error('Error al actualizar método de pago');
      }
    });
  }

  // Helper methods for template
  parseInt = parseInt;
  parseFloat = parseFloat;
}


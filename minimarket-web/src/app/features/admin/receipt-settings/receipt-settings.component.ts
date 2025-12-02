import { Component, OnInit, signal, inject, afterNextRender } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BrandSettingsService } from '../../../core/services/brand-settings.service';
import { ToastService } from '../../../shared/services/toast.service';
import { DocumentSettingsService } from '../../../core/services/document-settings.service';

@Component({
  selector: 'app-receipt-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './receipt-settings.component.html',
  styleUrl: './receipt-settings.component.css'
})
export class ReceiptSettingsComponent implements OnInit {
  private brandSettingsService = inject(BrandSettingsService);
  private toastService = inject(ToastService);
  private documentSettingsService = inject(DocumentSettingsService);

  isLoading = signal(false);
  isSaving = signal(false);
  isGeneratingPreview = signal(false);

  // Form data - Solo campos necesarios para boletas/facturas
  storeName = signal('');
  ruc = signal('');
  address = signal('');
  phone = signal('');

  // Valores actuales de BrandSettings para mantener los colores
  private currentBrandSettings: any = null;

  constructor() {
    afterNextRender(() => {
      this.loadSettings();
    });
  }

  ngOnInit(): void {}

  loadSettings(): void {
    this.isLoading.set(true);
    console.log('🔄 Iniciando carga de configuración desde el backend...');
    
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        console.log('📥 Respuesta del backend:', settings);
        
        if (settings) {
          // Guardar valores completos para mantener los colores al actualizar
          this.currentBrandSettings = settings;
          
          // Cargar valores en los signals - usar valores exactos de la BD, no fallback a ''
          const storeNameValue = settings.storeName || '';
          const rucValue = settings.ruc || '';
          const addressValue = settings.address || '';
          const phoneValue = settings.phone || '';
          
          this.storeName.set(storeNameValue);
          this.ruc.set(rucValue);
          this.address.set(addressValue);
          this.phone.set(phoneValue);
          
          // Log detallado para debugging
          console.log('✅ Configuración cargada desde la base de datos:', {
            storeName: storeNameValue,
            ruc: rucValue,
            address: addressValue,
            phone: phoneValue,
            'storeName (raw)': settings.storeName,
            'ruc (raw)': settings.ruc,
            'address (raw)': settings.address,
            'phone (raw)': settings.phone
          });
          
          // Verificar si hay datos guardados
          const hasData = storeNameValue.trim().length > 0 || 
                          rucValue.trim().length > 0 || 
                          addressValue.trim().length > 0 || 
                          phoneValue.trim().length > 0;
          
          if (hasData) {
            console.log('✅ Se encontraron datos guardados en la base de datos');
          } else {
            console.warn('⚠️ Los datos en la base de datos están vacíos. Necesitas guardar la configuración primero.');
            this.toastService.info('No hay datos guardados. Por favor, completa y guarda la configuración.');
          }
        } else {
          console.warn('⚠️ No se encontraron configuraciones en la base de datos (settings es null)');
          this.toastService.info('No hay configuración guardada. Por favor, completa y guarda los datos.');
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('❌ Error loading settings:', error);
        console.error('Error details:', JSON.stringify(error, null, 2));
        this.toastService.error('Error al cargar la configuración desde el servidor');
        this.isLoading.set(false);
      }
    });
  }


  onStoreNameChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
    console.log('📝 storeName cambiado a:', value);
    this.storeName.set(value);
  }

  onRucChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
    console.log('📝 ruc cambiado a:', value);
    this.ruc.set(value);
  }

  onAddressChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
    console.log('📝 address cambiado a:', value);
    this.address.set(value);
  }

  onPhoneChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
    console.log('📝 phone cambiado a:', value);
    this.phone.set(value);
  }


  validateRuc(): boolean {
    const rucValue = this.ruc().trim();
    if (rucValue && !/^\d{11}$/.test(rucValue)) {
      this.toastService.error('El RUC debe tener exactamente 11 dígitos numéricos');
      return false;
    }
    return true;
  }

  saveSettings(): void {
    // IMPORTANTE: Capturar valores ANTES de cualquier otra operación
    const storeNameValue = this.storeName().trim();
    const rucValue = this.ruc().trim();
    const addressValue = this.address().trim();
    const phoneValue = this.phone().trim();

    // Validar después de capturar
    if (!storeNameValue) {
      this.toastService.error('El nombre comercial es requerido');
      return;
    }

    if (rucValue && !/^\d{11}$/.test(rucValue)) {
      this.toastService.error('El RUC debe tener exactamente 11 dígitos numéricos');
      return;
    }

    this.isSaving.set(true);

    // Log detallado ANTES de enviar
    console.log('💾 === INICIANDO GUARDADO ===');
    console.log('💾 Valores capturados del formulario (después de trim):', {
      storeName: storeNameValue,
      ruc: rucValue,
      address: addressValue,
      phone: phoneValue
    });
    console.log('💾 Valores RAW de los signals (antes de trim):', {
      'storeName (raw)': this.storeName(),
      'ruc (raw)': this.ruc(),
      'address (raw)': this.address(),
      'phone (raw)': this.phone()
    });
    
    // Validar que al menos storeName tenga valor
    if (!storeNameValue) {
      console.error('❌ ERROR: storeName está vacío después de capturar!');
      this.toastService.error('El nombre comercial es requerido');
      this.isSaving.set(false);
      return;
    }

    // Usar currentBrandSettings que ya está cargado (no hacer get() para evitar timing issues)
    const currentSettings = this.currentBrandSettings;
    
    console.log('💾 BrandSettings actual (desde memoria):', {
      storeName: currentSettings?.storeName,
      ruc: currentSettings?.ruc,
      address: currentSettings?.address,
      phone: currentSettings?.phone
    });

    // Construir updateData con TODOS los campos requeridos
    // IMPORTANTE: Usar los valores capturados del formulario, NO los de currentSettings
    const updateData: any = {
      // Campos editables en este módulo - USAR VALORES DEL FORMULARIO
      storeName: storeNameValue, // SIEMPRE usar el valor del formulario (requerido)
      // Para campos opcionales, usar undefined si están vacíos (no null ni string vacío)
      // El backend convertirá undefined a null correctamente
      ruc: rucValue || undefined,
      address: addressValue || undefined,
      phone: phoneValue || undefined,
      logoUrl: 'assets/logo.png', // Siempre usar el logo de assets
      // Mantener TODOS los demás campos de BrandSettings
      logoEmoji: currentSettings?.logoEmoji || '',
      faviconUrl: currentSettings?.faviconUrl || '',
      primaryColor: currentSettings?.primaryColor || '#4A90E2',
      secondaryColor: currentSettings?.secondaryColor || '#0d7ff2',
      buttonColor: currentSettings?.buttonColor || '#4A90E2',
      textColor: currentSettings?.textColor || '#000000',
      hoverColor: currentSettings?.hoverColor || '#4A90E2',
      description: currentSettings?.description || '',
      slogan: currentSettings?.slogan || '',
      whatsAppPhone: currentSettings?.whatsAppPhone || '',
      email: currentSettings?.email || '',
      yapePhone: currentSettings?.yapePhone || '',
      plinPhone: currentSettings?.plinPhone || '',
      yapeQRUrl: currentSettings?.yapeQRUrl || '',
      plinQRUrl: currentSettings?.plinQRUrl || '',
      yapeEnabled: currentSettings?.yapeEnabled ?? false,
      plinEnabled: currentSettings?.plinEnabled ?? false,
      bankName: currentSettings?.bankName || '',
      bankAccountType: currentSettings?.bankAccountType || '',
      bankAccountNumber: currentSettings?.bankAccountNumber || '',
      bankCCI: currentSettings?.bankCCI || '',
      bankAccountVisible: currentSettings?.bankAccountVisible ?? false,
      deliveryType: currentSettings?.deliveryType || 'Ambos',
      deliveryCost: currentSettings?.deliveryCost,
      deliveryZones: currentSettings?.deliveryZones || '',
      homeTitle: currentSettings?.homeTitle || '',
      homeSubtitle: currentSettings?.homeSubtitle || '',
      homeDescription: currentSettings?.homeDescription || '',
      homeBannerImageUrl: currentSettings?.homeBannerImageUrl || ''
    };

    // Log del objeto que se enviará al backend
    console.log('💾 === OBJETO A ENVIAR AL BACKEND ===');
    console.log('💾 updateData:', JSON.stringify(updateData, null, 2));
    console.log('💾 Valores clave a enviar:', {
      storeName: updateData.storeName,
      ruc: updateData.ruc,
      address: updateData.address,
      phone: updateData.phone
    });

    this.brandSettingsService.update(updateData).subscribe({
      next: (updatedSettings) => {
        console.log('💾 Respuesta del backend después de guardar:', updatedSettings);
        
        // Actualizar currentBrandSettings con los valores guardados
        this.currentBrandSettings = updatedSettings;
        
        // Actualizar DIRECTAMENTE los signals con los valores que acabamos de guardar
        // Usar los valores del backend (updatedSettings) que son los que realmente se guardaron
        const savedStoreName = updatedSettings.storeName || storeNameValue;
        const savedRuc = updatedSettings.ruc || rucValue;
        const savedAddress = updatedSettings.address || addressValue;
        const savedPhone = updatedSettings.phone || phoneValue;
        
        this.storeName.set(savedStoreName);
        this.ruc.set(savedRuc);
        this.address.set(savedAddress);
        this.phone.set(savedPhone);
        
        console.log('✅ Configuración guardada y actualizada localmente:', {
          storeName: savedStoreName,
          ruc: savedRuc,
          address: savedAddress,
          phone: savedPhone,
          'storeName (raw from backend)': updatedSettings.storeName,
          'ruc (raw from backend)': updatedSettings.ruc,
          'address (raw from backend)': updatedSettings.address,
          'phone (raw from backend)': updatedSettings.phone
        });
        
        // Verificar que los datos se guardaron correctamente
        const hasSavedData = savedStoreName.trim().length > 0 || 
                             savedRuc.trim().length > 0 || 
                             savedAddress.trim().length > 0 || 
                             savedPhone.trim().length > 0;
        
        if (hasSavedData) {
          this.toastService.success('✅ Configuración guardada correctamente. Los datos se usarán en todas las boletas y facturas del sistema.');
        } else {
          this.toastService.warning('⚠️ Configuración guardada, pero algunos campos están vacíos. Las boletas usarán valores por defecto.');
        }
        
        this.isSaving.set(false);
      },
      error: (error: any) => {
        console.error('❌ Error saving settings:', error);
        console.error('Error details:', JSON.stringify(error, null, 2));
        const errorMessage = error?.error?.message || error?.message || 'Error al guardar configuración';
        this.toastService.error(errorMessage);
        this.isSaving.set(false);
      }
    });
  }

  generatePreviewPdf(): void {
    if (!this.storeName().trim()) {
      this.toastService.error('Debe guardar la configuración primero');
      return;
    }

    this.isGeneratingPreview.set(true);

    // Usar el endpoint de preview PDF que genera un PDF real con datos dummy
    const settings = {
      companyName: this.storeName().trim(),
      companyRuc: this.ruc().trim() || '',
      companyAddress: this.address().trim() || '',
      companyPhone: this.phone().trim() || '',
      companyEmail: '',
      logoUrl: 'assets/logo.png' // Siempre usar el logo de assets
    };

    // Llamar al endpoint que genera el PDF de prueba
    this.documentSettingsService.getPreviewPdf('Boleta', settings).subscribe({
      next: (blob: Blob) => {
        // Crear URL del blob y descargar
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `preview-boleta-${Date.now()}.pdf`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        
        this.isGeneratingPreview.set(false);
        this.toastService.success('PDF de prueba generado exitosamente');
      },
      error: (error: any) => {
        console.error('Error generating preview PDF:', error);
        this.isGeneratingPreview.set(false);
        const errorMessage = error?.error?.message || error?.message || 'Error al generar el PDF de prueba';
        this.toastService.error(errorMessage);
      }
    });
  }
}


import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SalesService } from '../../../core/services/sales.service';
import { ToastService } from '../../../shared/services/toast.service';
import { DocumentSettingsService, DocumentViewSettings } from '../../../core/services/document-settings.service';

@Component({
  selector: 'app-document-template',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './document-template.component.html',
  styleUrl: './document-template.component.css'
})
export class DocumentTemplateComponent implements OnInit {
  selectedDocumentType = signal<'Boleta' | 'Factura'>('Boleta');
  previewSaleId = signal<string | null>(null);
  isLoading = signal(false);

  // Configuración de personalización
  showLogo = signal(true);
  logoUrl = signal<string>('');
  companyName = signal<string>('Minimarket Camucha');
  companyAddress = signal<string>('');
  companyPhone = signal<string>('');
  companyEmail = signal<string>('');
  companyRuc = signal<string>('');

  // Colores
  primaryColor = signal<string>('#3B82F6');
  secondaryColor = signal<string>('#1E40AF');
  textColor = signal<string>('#111827');

  // Fuentes
  fontFamily = signal<string>('Arial');
  fontSize = signal<number>(12);

  // Configuración de visualización
  defaultViewMode = signal<'preview' | 'direct'>('preview');
  directPrint = signal<boolean>(false);
  boletaTemplateActive = signal<boolean>(true);
  facturaTemplateActive = signal<boolean>(true);

  constructor(
    private salesService: SalesService,
    private toastService: ToastService,
    private documentSettingsService: DocumentSettingsService
  ) {}

  ngOnInit(): void {
    // Cargar configuración guardada si existe
    this.loadSavedSettings();
    // Cargar configuración de visualización
    this.loadViewSettings();
  }

  loadViewSettings(): void {
    this.documentSettingsService.getViewSettings().subscribe({
      next: (settings) => {
        this.defaultViewMode.set(settings.defaultViewMode);
        this.directPrint.set(settings.directPrint);
        this.boletaTemplateActive.set(settings.boletaTemplateActive);
        this.facturaTemplateActive.set(settings.facturaTemplateActive);
      },
      error: (error) => {
        console.error('Error loading view settings:', error);
        // Usar valores por defecto si hay error (incluyendo 404)
        this.defaultViewMode.set('preview');
        this.directPrint.set(false);
        this.boletaTemplateActive.set(true);
        this.facturaTemplateActive.set(true);
      }
    });
  }

  loadSavedSettings(): void {
    // TODO: Cargar desde backend o localStorage
    const saved = localStorage.getItem('documentTemplateSettings');
    if (saved) {
      try {
        const settings = JSON.parse(saved);
        this.showLogo.set(settings.showLogo ?? true);
        this.logoUrl.set(settings.logoUrl ?? '');
        this.companyName.set(settings.companyName ?? 'Minimarket Camucha');
        this.companyAddress.set(settings.companyAddress ?? '');
        this.companyPhone.set(settings.companyPhone ?? '');
        this.companyEmail.set(settings.companyEmail ?? '');
        this.companyRuc.set(settings.companyRuc ?? '');
        this.primaryColor.set(settings.primaryColor ?? '#3B82F6');
        this.secondaryColor.set(settings.secondaryColor ?? '#1E40AF');
        this.textColor.set(settings.textColor ?? '#111827');
        this.fontFamily.set(settings.fontFamily ?? 'Arial');
        this.fontSize.set(settings.fontSize ?? 12);
      } catch (e) {
        console.error('Error loading saved settings:', e);
      }
    }
  }

  saveSettings(): void {
    // Guardar configuración de personalización visual
    const templateSettings = {
      showLogo: this.showLogo(),
      logoUrl: this.logoUrl(),
      companyName: this.companyName(),
      companyAddress: this.companyAddress(),
      companyPhone: this.companyPhone(),
      companyEmail: this.companyEmail(),
      companyRuc: this.companyRuc(),
      primaryColor: this.primaryColor(),
      secondaryColor: this.secondaryColor(),
      textColor: this.textColor(),
      fontFamily: this.fontFamily(),
      fontSize: this.fontSize()
    };

    localStorage.setItem('documentTemplateSettings', JSON.stringify(templateSettings));

    // Guardar configuración de visualización en backend
    const viewSettings: DocumentViewSettings = {
      defaultViewMode: this.defaultViewMode(),
      directPrint: this.directPrint(),
      boletaTemplateActive: this.boletaTemplateActive(),
      facturaTemplateActive: this.facturaTemplateActive()
    };

    this.documentSettingsService.updateViewSettings(viewSettings).subscribe({
      next: () => {
        this.toastService.success('Configuración guardada exitosamente');
      },
      error: (error) => {
        console.error('Error saving view settings:', error);
        this.toastService.error('Error al guardar la configuración de visualización');
      }
    });
  }

  previewDocument(): void {
    // Obtener una venta reciente para preview
    // Por ahora, abrir en nueva pestaña con un ID de ejemplo
    // TODO: Implementar endpoint de preview en backend
    this.toastService.info('Funcionalidad de preview próximamente');
  }

  downloadTemplate(): void {
    // Descargar plantilla personalizada
    // TODO: Implementar generación de plantilla personalizada
    this.toastService.info('Funcionalidad de descarga próximamente');
  }

  resetToDefault(): void {
    if (confirm('¿Está seguro de restaurar la configuración por defecto?')) {
      this.showLogo.set(true);
      this.logoUrl.set('');
      this.companyName.set('Minimarket Camucha');
      this.companyAddress.set('');
      this.companyPhone.set('');
      this.companyEmail.set('');
      this.companyRuc.set('');
      this.primaryColor.set('#3B82F6');
      this.secondaryColor.set('#1E40AF');
      this.textColor.set('#111827');
      this.fontFamily.set('Arial');
      this.fontSize.set(12);
      this.saveSettings();
    }
  }
}


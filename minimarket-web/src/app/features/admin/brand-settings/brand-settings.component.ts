import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { BrandSettingsService, UpdateBrandSettings } from '../../../core/services/brand-settings.service';
import { ToastService } from '../../../shared/services/toast.service';
import { FilesService } from '../../../core/services/files.service';
import { environment } from '../../../../environments/environment';
import { SettingsNavbarComponent } from '../../../shared/components/settings-navbar/settings-navbar.component';

@Component({
  selector: 'app-brand-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, SettingsNavbarComponent],
  templateUrl: './brand-settings.component.html',
  styleUrl: './brand-settings.component.css'
})
export class BrandSettingsComponent implements OnInit {
  isLoading = signal(false);
  isSaving = signal(false);
  
  // Form data
  logoUrl = signal('');
  storeName = signal('');
  faviconUrl = signal('');
  primaryColor = signal('#4CAF50');
  secondaryColor = signal('#0d7ff2');
  buttonColor = signal('#4CAF50');
  textColor = signal('#333333');
  hoverColor = signal('#45a049');
  description = signal('');
  slogan = signal('');
  phone = signal('');
  email = signal('');
  address = signal('');
  ruc = signal('');
  
  // Métodos de pago
  yapePhone = signal('');
  plinPhone = signal('');
  yapeQRUrl = signal('');
  plinQRUrl = signal('');
  yapeEnabled = signal(false);
  plinEnabled = signal(false);
  
  // Cuenta bancaria
  bankName = signal('');
  bankAccountType = signal('');
  bankAccountNumber = signal('');
  bankCCI = signal('');
  bankAccountVisible = signal(false);
  
  // Opciones de envío
  deliveryType = signal('Ambos');
  deliveryCost = signal<number | null>(null);
  deliveryZones = signal('');

  // Preview
  previewLogoUrl = signal('');
  previewFaviconUrl = signal('');
  previewYapeQRUrl = signal('');
  previewPlinQRUrl = signal('');

  constructor(
    private brandSettingsService: BrandSettingsService,
    private toastService: ToastService,
    private filesService: FilesService
  ) {}

  ngOnInit(): void {
    this.loadSettings();
  }

  loadSettings(): void {
    this.isLoading.set(true);
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        if (settings) {
          this.logoUrl.set(settings.logoUrl);
          this.storeName.set(settings.storeName);
          this.faviconUrl.set(settings.faviconUrl || '');
          this.primaryColor.set(settings.primaryColor);
          this.secondaryColor.set(settings.secondaryColor);
          this.buttonColor.set(settings.buttonColor);
          this.textColor.set(settings.textColor);
          this.hoverColor.set(settings.hoverColor);
          this.description.set(settings.description || '');
          this.slogan.set(settings.slogan || '');
          this.phone.set(settings.phone || '');
          this.email.set(settings.email || '');
          this.address.set(settings.address || '');
          this.ruc.set(settings.ruc || '');
          
          // Métodos de pago
          this.yapePhone.set(settings.yapePhone || '');
          this.plinPhone.set(settings.plinPhone || '');
          this.yapeQRUrl.set(settings.yapeQRUrl || '');
          this.plinQRUrl.set(settings.plinQRUrl || '');
          this.yapeEnabled.set(settings.yapeEnabled ?? false);
          this.plinEnabled.set(settings.plinEnabled ?? false);
          
          // Cuenta bancaria
          this.bankName.set(settings.bankName || '');
          this.bankAccountType.set(settings.bankAccountType || '');
          this.bankAccountNumber.set(settings.bankAccountNumber || '');
          this.bankCCI.set(settings.bankCCI || '');
          this.bankAccountVisible.set(settings.bankAccountVisible ?? false);
          
          // Opciones de envío
          this.deliveryType.set(settings.deliveryType || 'Ambos');
          this.deliveryCost.set(settings.deliveryCost ?? null);
          this.deliveryZones.set(settings.deliveryZones || '');

          this.previewLogoUrl.set(settings.logoUrl);
          this.previewFaviconUrl.set(settings.faviconUrl || '');
          this.previewYapeQRUrl.set(settings.yapeQRUrl || '');
          this.previewPlinQRUrl.set(settings.plinQRUrl || '');
        }
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading brand settings:', error);
        this.toastService.error('Error al cargar configuración de marca');
        this.isLoading.set(false);
      }
    });
  }

  onLogoUpload(event: any): void {
    const file = event.target.files[0];
    if (!file) return;

    this.isLoading.set(true);
    this.filesService.uploadFile(file, 'brand').subscribe({
      next: (response) => {
        this.logoUrl.set(response.url);
        this.previewLogoUrl.set(response.url);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error uploading logo:', error);
        this.toastService.error('Error al subir logo');
        this.isLoading.set(false);
      }
    });
  }

  onFaviconUpload(event: any): void {
    const file = event.target.files[0];
    if (!file) return;

    // Validar tamaño (16x16 o 32x32)
    const img = new Image();
    img.onload = () => {
      if ((img.width === 16 && img.height === 16) || (img.width === 32 && img.height === 32)) {
        this.isLoading.set(true);
        this.filesService.uploadFile(file, 'brand').subscribe({
          next: (response) => {
            this.faviconUrl.set(response.url);
            this.previewFaviconUrl.set(response.url);
            this.isLoading.set(false);
          },
          error: (error) => {
            console.error('Error uploading favicon:', error);
            this.toastService.error('Error al subir favicon');
            this.isLoading.set(false);
          }
        });
      } else {
        this.toastService.error('El favicon debe ser de 16x16 o 32x32 píxeles');
      }
    };
    img.src = URL.createObjectURL(file);
  }

  saveSettings(): void {
    if (!this.storeName().trim()) {
      this.toastService.error('El nombre de la tienda es requerido');
      return;
    }

    this.isSaving.set(true);

    const updateData: UpdateBrandSettings = {
      logoUrl: this.logoUrl(),
      storeName: this.storeName(),
      faviconUrl: this.faviconUrl() || undefined,
      primaryColor: this.primaryColor(),
      secondaryColor: this.secondaryColor(),
      buttonColor: this.buttonColor(),
      textColor: this.textColor(),
      hoverColor: this.hoverColor(),
      description: this.description() || undefined,
      slogan: this.slogan() || undefined,
      phone: this.phone() || undefined,
      email: this.email() || undefined,
      address: this.address() || undefined,
      ruc: this.ruc() || undefined,
      // Métodos de pago
      yapePhone: this.yapePhone() || undefined,
      plinPhone: this.plinPhone() || undefined,
      yapeQRUrl: this.yapeQRUrl() || undefined,
      plinQRUrl: this.plinQRUrl() || undefined,
      yapeEnabled: this.yapeEnabled(),
      plinEnabled: this.plinEnabled(),
      // Cuenta bancaria
      bankName: this.bankName() || undefined,
      bankAccountType: this.bankAccountType() || undefined,
      bankAccountNumber: this.bankAccountNumber() || undefined,
      bankCCI: this.bankCCI() || undefined,
      bankAccountVisible: this.bankAccountVisible(),
      // Opciones de envío
      deliveryType: this.deliveryType(),
      deliveryCost: this.deliveryCost() ?? undefined,
      deliveryZones: this.deliveryZones() || undefined
    };

    this.brandSettingsService.update(updateData).subscribe({
      next: () => {
        this.toastService.success('Configuración de marca guardada correctamente');
        this.isSaving.set(false);
      },
      error: (error) => {
        console.error('Error saving brand settings:', error);
        this.toastService.error('Error al guardar configuración');
        this.isSaving.set(false);
      }
    });
  }

  updatePreview(): void {
    // Actualizar preview cuando cambian los colores
    this.previewLogoUrl.set(this.logoUrl());
    this.previewFaviconUrl.set(this.faviconUrl());
  }

  onYapeQRUpload(event: any): void {
    const file = event.target.files[0];
    if (!file) return;

    this.isLoading.set(true);
    this.filesService.uploadFile(file, 'payment-qr').subscribe({
      next: (response) => {
        this.yapeQRUrl.set(response.url);
        this.previewYapeQRUrl.set(response.url);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error uploading Yape QR:', error);
        this.toastService.error('Error al subir QR de Yape');
        this.isLoading.set(false);
      }
    });
  }

  onPlinQRUpload(event: any): void {
    const file = event.target.files[0];
    if (!file) return;

    this.isLoading.set(true);
    this.filesService.uploadFile(file, 'payment-qr').subscribe({
      next: (response) => {
        this.plinQRUrl.set(response.url);
        this.previewPlinQRUrl.set(response.url);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error uploading Plin QR:', error);
        this.toastService.error('Error al subir QR de Plin');
        this.isLoading.set(false);
      }
    });
  }
}


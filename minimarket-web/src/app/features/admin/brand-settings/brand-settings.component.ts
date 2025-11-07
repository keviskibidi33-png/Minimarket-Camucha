import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { BrandSettingsService, UpdateBrandSettings } from '../../../core/services/brand-settings.service';
import { ToastService } from '../../../shared/services/toast.service';
import { FilesService } from '../../../core/services/files.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-brand-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
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

  // Preview
  previewLogoUrl = signal('');
  previewFaviconUrl = signal('');

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

          this.previewLogoUrl.set(settings.logoUrl);
          this.previewFaviconUrl.set(settings.faviconUrl || '');
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
      ruc: this.ruc() || undefined
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
}


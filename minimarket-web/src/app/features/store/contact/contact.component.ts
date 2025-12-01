import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { StoreHeaderComponent } from '../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../shared/components/store-footer/store-footer.component';
import { ToastService } from '../../../shared/services/toast.service';
import { BrandSettingsService, BrandSettings } from '../../../core/services/brand-settings.service';
import { ContactService } from '../../../core/services/contact.service';

@Component({
  selector: 'app-store-contact',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, StoreHeaderComponent, StoreFooterComponent],
  templateUrl: './contact.component.html',
  styleUrl: './contact.component.css'
})
export class StoreContactComponent implements OnInit {
  name = signal('');
  email = signal('');
  phone = signal('');
  subject = signal('');
  message = signal('');
  isSubmitting = signal(false);
  
  // Datos de contacto desde BrandSettings
  brandSettings = signal<BrandSettings | null>(null);
  contactAddress = signal('');
  contactEmail = signal('');
  contactPhone = signal('');

  constructor(
    private toastService: ToastService,
    private brandSettingsService: BrandSettingsService,
    private contactService: ContactService
  ) {}
  
  ngOnInit(): void {
    // Cargar datos de BrandSettings
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        if (settings) {
          this.brandSettings.set(settings);
          if (settings.address) {
            this.contactAddress.set(settings.address);
          }
          // Usar email de BrandSettings o el correo por defecto
          this.contactEmail.set(settings.email || 'minimarket.camucha@gmail.com');
          if (settings.phone) {
            this.contactPhone.set(settings.phone);
          }
        } else {
          // Si no hay settings, usar valores por defecto
          this.contactEmail.set('minimarket.camucha@gmail.com');
        }
      },
      error: (error) => {
        console.error('Error loading brand settings:', error);
        // En caso de error, usar valores por defecto
        this.contactEmail.set('minimarket.camucha@gmail.com');
      }
    });
  }

  onSubmit(): void {
    if (!this.name() || !this.email() || !this.message()) {
      this.toastService.error('Por favor completa todos los campos requeridos');
      return;
    }

    // Validar email
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.email())) {
      this.toastService.error('Por favor ingresa un email válido');
      return;
    }

    this.isSubmitting.set(true);

    // Enviar email de contacto
    this.contactService.sendContactEmail({
      name: this.name(),
      email: this.email(),
      phone: this.phone() || undefined,
      subject: this.subject() || undefined,
      message: this.message()
    }).subscribe({
      next: () => {
        this.toastService.success('¡Mensaje enviado! Nos pondremos en contacto contigo pronto.');
        this.resetForm();
        this.isSubmitting.set(false);
      },
      error: (error: any) => {
        console.error('Error sending contact email:', error);
        this.toastService.error('Error al enviar el mensaje. Por favor intenta nuevamente.');
        this.isSubmitting.set(false);
      }
    });
  }

  resetForm(): void {
    this.name.set('');
    this.email.set('');
    this.phone.set('');
    this.subject.set('');
    this.message.set('');
  }
}


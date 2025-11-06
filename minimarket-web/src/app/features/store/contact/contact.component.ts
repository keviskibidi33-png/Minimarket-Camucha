import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { StoreHeaderComponent } from '../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../shared/components/store-footer/store-footer.component';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-store-contact',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, StoreHeaderComponent, StoreFooterComponent],
  templateUrl: './contact.component.html',
  styleUrl: './contact.component.css'
})
export class StoreContactComponent {
  name = signal('');
  email = signal('');
  phone = signal('');
  subject = signal('');
  message = signal('');
  isSubmitting = signal(false);

  constructor(private toastService: ToastService) {}

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

    // Simular envío (aquí podrías integrar con un servicio de email o API)
    setTimeout(() => {
      this.toastService.success('¡Mensaje enviado! Nos pondremos en contacto contigo pronto.');
      this.resetForm();
      this.isSubmitting.set(false);
    }, 1500);
  }

  resetForm(): void {
    this.name.set('');
    this.email.set('');
    this.phone.set('');
    this.subject.set('');
    this.message.set('');
  }
}


import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BrandSettingsService, BrandSettings } from '../../../core/services/brand-settings.service';

@Component({
  selector: 'app-store-footer',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './store-footer.component.html',
  styleUrl: './store-footer.component.css'
})
export class StoreFooterComponent implements OnInit {
  storeName = signal('Minimarket Camucha');
  address = signal('');
  email = signal('');
  phone = signal('');
  currentYear = new Date().getFullYear();
  brandSettings = signal<BrandSettings | null>(null);

  constructor(private brandSettingsService: BrandSettingsService) {}

  ngOnInit(): void {
    // Cargar datos de BrandSettings
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        if (settings) {
          this.brandSettings.set(settings);
          if (settings.storeName) {
            this.storeName.set(settings.storeName);
          }
          if (settings.address) {
            this.address.set(settings.address);
          }
          // Usar email de BrandSettings o el correo por defecto
          this.email.set(settings.email || 'minimarket.camucha@gmail.com');
          if (settings.phone) {
            this.phone.set(settings.phone);
          }
        } else {
          // Si no hay settings, usar valores por defecto
          this.email.set('minimarket.camucha@gmail.com');
        }
      },
      error: (error) => {
        console.error('Error loading brand settings:', error);
        // En caso de error, usar valores por defecto
        this.email.set('minimarket.camucha@gmail.com');
      }
    });
  }
}


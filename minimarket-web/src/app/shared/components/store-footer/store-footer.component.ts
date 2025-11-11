import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BrandSettingsService } from '../../../core/services/brand-settings.service';

@Component({
  selector: 'app-store-footer',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './store-footer.component.html',
  styleUrl: './store-footer.component.css'
})
export class StoreFooterComponent implements OnInit {
  storeName = signal('Minimarket Camucha');
  currentYear = new Date().getFullYear();

  constructor(private brandSettingsService: BrandSettingsService) {}

  ngOnInit(): void {
    // Cargar nombre de la tienda
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        if (settings?.storeName) {
          this.storeName.set(settings.storeName);
        }
      },
      error: (error) => {
        console.error('Error loading brand settings:', error);
        // Mantener el valor por defecto si hay error
      }
    });
  }
}


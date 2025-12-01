import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BrandSettingsService } from '../../../core/services/brand-settings.service';
import { BrandSettings } from '../../../core/services/brand-settings.service';

@Component({
  selector: 'app-terms',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './terms.component.html',
  styleUrl: './terms.component.css'
})
export class TermsComponent implements OnInit {
  brandSettings = signal<BrandSettings | null>(null);

  constructor(private brandSettingsService: BrandSettingsService) {}

  ngOnInit(): void {
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        this.brandSettings.set(settings);
      }
    });
  }

  get companyName(): string {
    return this.brandSettings()?.storeName || 'Minimarket Camucha';
  }

  get companyRuc(): string {
    return this.brandSettings()?.ruc || '10095190559';
  }

  get companyAddress(): string {
    return this.brandSettings()?.address || 'Jr. Pedro Labarthe 449 – Ingeniería, San Martín de Porres, Lima, Lima, Perú';
  }
}


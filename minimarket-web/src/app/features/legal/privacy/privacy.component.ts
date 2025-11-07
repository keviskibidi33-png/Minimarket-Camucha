import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BrandSettingsService } from '../../../core/services/brand-settings.service';
import { BrandSettings } from '../../../core/services/brand-settings.service';

@Component({
  selector: 'app-privacy',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './privacy.component.html',
  styleUrl: './privacy.component.css'
})
export class PrivacyComponent implements OnInit {
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
    return this.brandSettings()?.ruc || '20123456789';
  }

  get companyAddress(): string {
    return this.brandSettings()?.address || 'Av. Principal 123, Lima, Perú';
  }
}


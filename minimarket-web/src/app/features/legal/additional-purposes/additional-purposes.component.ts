import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BrandSettingsService } from '../../../core/services/brand-settings.service';
import { BrandSettings } from '../../../core/services/brand-settings.service';

@Component({
  selector: 'app-additional-purposes',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './additional-purposes.component.html',
  styleUrl: './additional-purposes.component.css'
})
export class AdditionalPurposesComponent implements OnInit {
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

  get companyAddress(): string {
    return this.brandSettings()?.address || 'Av. Principal 123, Lima, Perú';
  }
}


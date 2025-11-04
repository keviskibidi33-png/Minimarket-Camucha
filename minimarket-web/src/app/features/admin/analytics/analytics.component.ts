import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnalyticsService, AnalyticsDashboard } from '../../../core/services/analytics.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './analytics.component.html',
  styleUrl: './analytics.component.css'
})
export class AnalyticsComponent implements OnInit {
  dashboard = signal<AnalyticsDashboard | null>(null);
  isLoading = signal(false);
  startDate = signal('');
  endDate = signal('');

  constructor(
    private analyticsService: AnalyticsService,
    private toastService: ToastService
  ) {
    // Por defecto: últimos 30 días
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - 30);
    
    this.startDate.set(startDate.toISOString().split('T')[0]);
    this.endDate.set(endDate.toISOString().split('T')[0]);
  }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading.set(true);
    
    const start = this.startDate() ? new Date(this.startDate()) : undefined;
    const end = this.endDate() ? new Date(this.endDate()) : undefined;

    this.analyticsService.getDashboard(start, end).subscribe({
      next: (dashboard) => {
        this.dashboard.set(dashboard);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading analytics:', error);
        this.toastService.error('Error al cargar analytics');
        this.isLoading.set(false);
      }
    });
  }

  applyFilters(): void {
    this.loadDashboard();
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }
}


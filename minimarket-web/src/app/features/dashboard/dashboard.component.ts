import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService, DashboardStats } from '../../core/services/dashboard.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  stats = signal<DashboardStats | null>(null);
  isLoading = signal(true);

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.isLoading.set(true);
    this.dashboardService.getStats().subscribe({
      next: (stats) => {
        this.stats.set(stats);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading dashboard stats:', error);
        this.isLoading.set(false);
      }
    });
  }

  formatCurrency(amount: number): string {
    return `S/ ${amount.toFixed(2)}`;
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString('es-PE', {
      weekday: 'short',
      day: 'numeric',
      month: 'short'
    });
  }

  getMaxDailySale(): number {
    const stats = this.stats();
    if (!stats || stats.dailySales.length === 0) return 1;
    return Math.max(...stats.dailySales.map(d => d.total), 1);
  }

  getPercentage(total: number): number {
    const max = this.getMaxDailySale();
    return max > 0 ? (total / max) * 100 : 0;
  }
}

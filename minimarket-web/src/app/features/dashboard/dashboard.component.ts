import { Component, OnInit, signal, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService, DashboardStats } from '../../core/services/dashboard.service';
import { SalesService, Sale } from '../../core/services/sales.service';
import { RouterModule } from '@angular/router';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, Chart, registerables } from 'chart.js';

// Registrar todos los componentes de Chart.js
Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, BaseChartDirective],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;
  
  stats = signal<DashboardStats | null>(null);
  isLoading = signal(true);
  recentSales = signal<Sale[]>([]);
  isLoadingRecentSales = signal(false);

  constructor(
    private dashboardService: DashboardService,
    private salesService: SalesService
  ) {}

  ngOnInit(): void {
    this.loadStats();
    this.loadRecentSales();
  }

  loadStats(): void {
    this.isLoading.set(true);
    this.dashboardService.getStats().subscribe({
      next: (stats) => {
        this.stats.set(stats);
        this.isLoading.set(false);
        // Actualizar gráficos después de cargar datos
        setTimeout(() => {
          this.updateChartData();
          this.updateCharts();
        }, 100);
      },
      error: (error) => {
        console.error('Error loading dashboard stats:', error);
        this.isLoading.set(false);
      }
    });
  }

  loadRecentSales(): void {
    this.isLoadingRecentSales.set(true);
    const today = new Date();
    const sevenDaysAgo = new Date();
    sevenDaysAgo.setDate(today.getDate() - 7);
    
    this.salesService.getAll({
      startDate: sevenDaysAgo.toISOString(),
      endDate: today.toISOString(),
      page: 1,
      pageSize: 10
    }).subscribe({
      next: (pagedResult) => {
        this.recentSales.set(pagedResult.items || []);
        this.isLoadingRecentSales.set(false);
      },
      error: (error) => {
        console.error('Error loading recent sales:', error);
        this.recentSales.set([]);
        this.isLoadingRecentSales.set(false);
      }
    });
  }

  updateCharts(): void {
    if (this.chart) {
      this.chart.update();
    }
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

  // Configuración del gráfico de barras para ventas diarias
  public barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false
      },
      tooltip: {
        callbacks: {
          label: (context) => {
            const value = context.parsed.y;
            return `S/ ${value !== null && value !== undefined ? value.toFixed(2) : '0.00'}`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value) => {
            return `S/ ${Number(value).toFixed(0)}`;
          }
        }
      }
    }
  };

  public barChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [{
      data: [],
      backgroundColor: 'rgba(59, 130, 246, 0.5)',
      borderColor: 'rgba(59, 130, 246, 1)',
      borderWidth: 1
    }]
  };

  // Configuración del gráfico de líneas para tendencias
  public lineChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: true,
        position: 'top'
      },
      tooltip: {
        callbacks: {
          label: (context) => {
            const value = context.parsed.y;
            return `${context.dataset.label}: S/ ${value !== null && value !== undefined ? value.toFixed(2) : '0.00'}`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value) => {
            return `S/ ${Number(value).toFixed(0)}`;
          }
        }
      }
    }
  };

  public lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [{
      label: 'Ventas',
      data: [],
      borderColor: 'rgba(34, 197, 94, 1)',
      backgroundColor: 'rgba(34, 197, 94, 0.1)',
      tension: 0.4,
      fill: true
    }]
  };

  // Actualizar datos de gráficos cuando cambian las estadísticas
  updateChartData(): void {
    const stats = this.stats();
    if (!stats) return;

    // Actualizar gráfico de barras
    this.barChartData = {
      labels: stats.dailySales.map(d => this.formatDate(d.date)),
      datasets: [{
        data: stats.dailySales.map(d => d.total),
        backgroundColor: 'rgba(59, 130, 246, 0.5)',
        borderColor: 'rgba(59, 130, 246, 1)',
        borderWidth: 1
      }]
    };

    // Actualizar gráfico de líneas
    this.lineChartData = {
      labels: stats.dailySales.map(d => this.formatDate(d.date)),
      datasets: [{
        label: 'Ventas Diarias',
        data: stats.dailySales.map(d => d.total),
        borderColor: 'rgba(34, 197, 94, 1)',
        backgroundColor: 'rgba(34, 197, 94, 0.1)',
        tension: 0.4,
        fill: true
      }]
    };
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Pagado':
        return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300';
      case 'Anulado':
        return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300';
      case 'Pendiente':
        return 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300';
    }
  }
}

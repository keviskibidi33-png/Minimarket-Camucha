import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, Chart, registerables } from 'chart.js';
import { AnalyticsService, AnalyticsDashboard } from '../../../core/services/analytics.service';
import { ToastService } from '../../../shared/services/toast.service';

// Registrar todos los componentes de Chart.js
Chart.register(...registerables);

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule, BaseChartDirective],
  templateUrl: './analytics.component.html',
  styleUrl: './analytics.component.css'
})
export class AnalyticsComponent implements OnInit {
  dashboard = signal<AnalyticsDashboard | null>(null);
  isLoading = signal(false);
  startDate = signal('');
  endDate = signal('');

  // Configuración de gráfica de línea (Vistas y Ventas a lo largo del tiempo)
  lineChartData = computed<ChartConfiguration<'line'>['data']>(() => {
    const data = this.dashboard();
    if (!data || !data.dailyStats || data.dailyStats.length === 0) {
      return {
        labels: [],
        datasets: []
      };
    }

    const labels = data.dailyStats.map(stat => this.formatDateShort(stat.date));
    
    return {
      labels,
      datasets: [
        {
          label: 'Vistas de Páginas',
          data: data.dailyStats.map(stat => stat.pageViews),
          borderColor: 'rgb(59, 130, 246)',
          backgroundColor: 'rgba(59, 130, 246, 0.15)',
          borderWidth: 3,
          tension: 0.5,
          fill: true,
          pointRadius: 5,
          pointHoverRadius: 7,
          pointBackgroundColor: 'rgb(59, 130, 246)',
          pointBorderColor: '#fff',
          pointBorderWidth: 2
        },
        {
          label: 'Vistas de Productos',
          data: data.dailyStats.map(stat => stat.productViews),
          borderColor: 'rgb(16, 185, 129)',
          backgroundColor: 'rgba(16, 185, 129, 0.15)',
          borderWidth: 3,
          tension: 0.5,
          fill: true,
          pointRadius: 5,
          pointHoverRadius: 7,
          pointBackgroundColor: 'rgb(16, 185, 129)',
          pointBorderColor: '#fff',
          pointBorderWidth: 2
        },
        {
          label: 'Ventas',
          data: data.dailyStats.map(stat => stat.sales),
          borderColor: 'rgb(245, 101, 101)',
          backgroundColor: 'rgba(245, 101, 101, 0.15)',
          borderWidth: 3,
          tension: 0.5,
          fill: true,
          pointRadius: 5,
          pointHoverRadius: 7,
          pointBackgroundColor: 'rgb(245, 101, 101)',
          pointBorderColor: '#fff',
          pointBorderWidth: 2,
          yAxisID: 'y1'
        }
      ]
    };
  });

  lineChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    animation: {
      duration: 1000,
      easing: 'easeInOutQuart'
    },
    plugins: {
      legend: {
        display: true,
        position: 'top',
        labels: {
          usePointStyle: true,
          padding: 15,
          font: {
            size: 12,
            weight: 'normal'
          }
        }
      },
      title: {
        display: true,
        text: 'Tendencias de Vistas y Ventas',
        font: {
          size: 16,
          weight: 'bold'
        },
        padding: {
          bottom: 20
        }
      },
      tooltip: {
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        padding: 12,
        titleFont: {
          size: 14,
          weight: 'bold'
        },
        bodyFont: {
          size: 13
        },
        borderColor: 'rgba(255, 255, 255, 0.1)',
        borderWidth: 1,
        cornerRadius: 8,
        displayColors: true,
        callbacks: {
          label: (context) => {
            return `${context.dataset.label}: ${context.parsed.y !== null && context.parsed.y !== undefined ? context.parsed.y.toLocaleString('es-PE') : '0'}`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        position: 'left',
        title: {
          display: true,
          text: 'Vistas'
        }
      },
      y1: {
        beginAtZero: true,
        position: 'right',
        title: {
          display: true,
          text: 'Ventas'
        },
        grid: {
          drawOnChartArea: false
        }
      }
    }
  };

  lineChartType = 'line' as const;

  // Configuración de gráfica de barras (Top Páginas)
  topPagesChartData = computed<ChartConfiguration<'bar'>['data']>(() => {
    const data = this.dashboard();
    if (!data || !data.topPages || data.topPages.length === 0) {
      return {
        labels: [],
        datasets: []
      };
    }

    return {
      labels: data.topPages.map(page => page.pageSlug),
      datasets: [
        {
          label: 'Vistas',
          data: data.topPages.map(page => page.viewCount),
          backgroundColor: [
            'rgba(59, 130, 246, 0.9)',
            'rgba(37, 99, 235, 0.9)',
            'rgba(29, 78, 216, 0.9)',
            'rgba(30, 64, 175, 0.9)',
            'rgba(30, 58, 138, 0.9)'
          ],
          borderColor: 'rgb(59, 130, 246)',
          borderWidth: 2,
          borderRadius: 6
        }
      ]
    };
  });

  topPagesChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    animation: {
      duration: 1000,
      easing: 'easeInOutQuart'
    },
    plugins: {
      legend: {
        display: false
      },
      title: {
        display: true,
        text: 'Páginas Más Visitadas',
        font: {
          size: 16,
          weight: 'bold'
        },
        padding: {
          bottom: 20
        }
      },
      tooltip: {
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        padding: 12,
        titleFont: {
          size: 14,
          weight: 'bold'
        },
        bodyFont: {
          size: 13
        },
        borderColor: 'rgba(255, 255, 255, 0.1)',
        borderWidth: 1,
        cornerRadius: 8,
        callbacks: {
          label: (context) => {
            const value = context.parsed.y;
            return `${value !== null && value !== undefined ? value.toLocaleString('es-PE') : '0'} vistas`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value) => {
            return Number(value).toLocaleString('es-PE');
          }
        }
      }
    }
  };

  topPagesChartType = 'bar' as const;

  // Configuración de gráfica de barras (Top Productos)
  topProductsChartData = computed<ChartConfiguration<'bar'>['data']>(() => {
    const data = this.dashboard();
    if (!data || !data.topProducts || data.topProducts.length === 0) {
      return {
        labels: [],
        datasets: []
      };
    }

    return {
      labels: data.topProducts.map(product => product.productName.length > 20 
        ? product.productName.substring(0, 20) + '...' 
        : product.productName),
      datasets: [
        {
          label: 'Vistas',
          data: data.topProducts.map(product => product.viewCount),
          backgroundColor: [
            'rgba(16, 185, 129, 0.9)',
            'rgba(5, 150, 105, 0.9)',
            'rgba(4, 120, 87, 0.9)',
            'rgba(6, 95, 70, 0.9)',
            'rgba(6, 78, 59, 0.9)'
          ],
          borderColor: 'rgb(16, 185, 129)',
          borderWidth: 2,
          borderRadius: 6
        }
      ]
    };
  });

  topProductsChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    animation: {
      duration: 1000,
      easing: 'easeInOutQuart'
    },
    plugins: {
      legend: {
        display: false
      },
      title: {
        display: true,
        text: 'Productos Más Visitados',
        font: {
          size: 16,
          weight: 'bold'
        },
        padding: {
          bottom: 20
        }
      },
      tooltip: {
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        padding: 12,
        titleFont: {
          size: 14,
          weight: 'bold'
        },
        bodyFont: {
          size: 13
        },
        borderColor: 'rgba(255, 255, 255, 0.1)',
        borderWidth: 1,
        cornerRadius: 8,
        callbacks: {
          label: (context) => {
            const value = context.parsed.y;
            return `${value !== null && value !== undefined ? value.toLocaleString('es-PE') : '0'} vistas`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value) => {
            return Number(value).toLocaleString('es-PE');
          }
        }
      }
    }
  };

  topProductsChartType = 'bar' as const;

  // Configuración de gráfica de barras comparativa (KPIs)
  kpisChartData = computed<ChartConfiguration<'bar'>['data']>(() => {
    const data = this.dashboard();
    if (!data) {
      return {
        labels: [],
        datasets: []
      };
    }

    return {
      labels: ['Vistas Páginas', 'Vistas Productos', 'Ventas'],
      datasets: [
        {
          label: 'Métricas',
          data: [
            data.totalPageViews,
            data.totalProductViews,
            data.totalSales
          ],
          backgroundColor: [
            'rgba(59, 130, 246, 0.8)',
            'rgba(16, 185, 129, 0.8)',
            'rgba(245, 101, 101, 0.8)'
          ],
          borderColor: [
            'rgb(59, 130, 246)',
            'rgb(16, 185, 129)',
            'rgb(245, 101, 101)'
          ],
          borderWidth: 1
        }
      ]
    };
  });

  kpisChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    animation: {
      duration: 1000,
      easing: 'easeInOutQuart'
    },
    plugins: {
      legend: {
        display: false
      },
      title: {
        display: true,
        text: 'Resumen de Métricas',
        font: {
          size: 16,
          weight: 'bold'
        },
        padding: {
          bottom: 20
        }
      },
      tooltip: {
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        padding: 12,
        titleFont: {
          size: 14,
          weight: 'bold'
        },
        bodyFont: {
          size: 13
        },
        borderColor: 'rgba(255, 255, 255, 0.1)',
        borderWidth: 1,
        cornerRadius: 8,
        callbacks: {
          label: (context) => {
            const value = context.parsed.y;
            return `${context.dataset.label || 'Métrica'}: ${value !== null && value !== undefined ? value.toLocaleString('es-PE') : '0'}`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value) => {
            return Number(value).toLocaleString('es-PE');
          }
        }
      }
    }
  };

  kpisChartType = 'bar' as const;

  // Configuración de gráfica de ingresos (línea)
  revenueChartData = computed<ChartConfiguration<'line'>['data']>(() => {
    const data = this.dashboard();
    if (!data || !data.dailyStats || data.dailyStats.length === 0) {
      return {
        labels: [],
        datasets: []
      };
    }

    const labels = data.dailyStats.map(stat => this.formatDateShort(stat.date));
    
    return {
      labels,
      datasets: [
        {
          label: 'Ingresos (S/)',
          data: data.dailyStats.map(stat => stat.revenue),
          borderColor: 'rgb(168, 85, 247)',
          backgroundColor: 'rgba(168, 85, 247, 0.2)',
          borderWidth: 3,
          tension: 0.5,
          fill: true,
          pointRadius: 5,
          pointHoverRadius: 7,
          pointBackgroundColor: 'rgb(168, 85, 247)',
          pointBorderColor: '#fff',
          pointBorderWidth: 2
        }
      ]
    };
  });

  revenueChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    animation: {
      duration: 1000,
      easing: 'easeInOutQuart'
    },
    plugins: {
      legend: {
        display: true,
        position: 'top',
        labels: {
          usePointStyle: true,
          padding: 15,
          font: {
            size: 12,
            weight: 'normal'
          }
        }
      },
      title: {
        display: true,
        text: 'Ingresos Diarios',
        font: {
          size: 16,
          weight: 'bold'
        },
        padding: {
          bottom: 20
        }
      },
      tooltip: {
        backgroundColor: 'rgba(0, 0, 0, 0.8)',
        padding: 12,
        titleFont: {
          size: 14,
          weight: 'bold'
        },
        bodyFont: {
          size: 13
        },
        borderColor: 'rgba(255, 255, 255, 0.1)',
        borderWidth: 1,
        cornerRadius: 8,
        callbacks: {
          label: (context) => {
            const value = context.parsed.y;
            return `Ingresos: S/ ${value !== null && value !== undefined ? value.toFixed(2) : '0.00'}`;
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        title: {
          display: true,
          text: 'Ingresos (S/)',
          font: {
            size: 12,
            weight: 'bold'
          }
        },
        ticks: {
          callback: (value) => {
            return `S/ ${Number(value).toFixed(0)}`;
          }
        }
      }
    }
  };

  revenueChartType = 'line' as const;

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

  setDateRange(range: '1d' | '3d' | '1w' | '1m'): void {
    const endDate = new Date();
    const startDate = new Date();
    
    switch (range) {
      case '1d':
        startDate.setDate(startDate.getDate() - 1);
        break;
      case '3d':
        startDate.setDate(startDate.getDate() - 3);
        break;
      case '1w':
        startDate.setDate(startDate.getDate() - 7);
        break;
      case '1m':
        startDate.setMonth(startDate.getMonth() - 1);
        break;
    }
    
    this.startDate.set(startDate.toISOString().split('T')[0]);
    this.endDate.set(endDate.toISOString().split('T')[0]);
    this.loadDashboard();
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('es-PE', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  formatDateShort(dateString: string): string {
    return new Date(dateString).toLocaleDateString('es-PE', {
      month: 'short',
      day: 'numeric'
    });
  }
}


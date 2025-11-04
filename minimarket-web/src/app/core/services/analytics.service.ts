import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AnalyticsDashboard {
  totalPageViews: number;
  totalProductViews: number;
  totalSales: number;
  totalRevenue: number;
  topPages: PageViewStats[];
  topProducts: ProductViewStats[];
  dailyStats: DailyStats[];
}

export interface PageViewStats {
  pageSlug: string;
  viewCount: number;
}

export interface ProductViewStats {
  productId: string;
  productName: string;
  viewCount: number;
}

export interface DailyStats {
  date: string;
  pageViews: number;
  productViews: number;
  sales: number;
  revenue: number;
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private readonly apiUrl = `${environment.apiUrl}/analytics`;

  constructor(private http: HttpClient) {}

  getDashboard(startDate?: Date, endDate?: Date): Observable<AnalyticsDashboard> {
    let params = new HttpParams();
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }
    return this.http.get<AnalyticsDashboard>(`${this.apiUrl}/dashboard`, { params });
  }

  trackPageView(pageSlug: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/track/page-view`, { pageSlug });
  }

  trackProductView(productId: string): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/track/product-view`, { productId });
  }
}


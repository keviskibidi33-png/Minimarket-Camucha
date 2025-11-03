import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DashboardStats {
  todaySales: number;
  monthSales: number;
  totalProducts: number;
  lowStockProducts: number;
  totalCustomers: number;
  todaySalesCount: number;
  monthSalesCount: number;
  topProducts: TopProduct[];
  dailySales: DailySale[];
}

export interface TopProduct {
  productId: string;
  productName: string;
  quantitySold: number;
  totalRevenue: number;
}

export interface DailySale {
  date: string;
  total: number;
  count: number;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/stats`);
  }
}


import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CashClosureSummary {
  totalPaid: number;
  totalCount: number;
  byPaymentMethod: Array<{
    method: string;
    total: number;
    count: number;
  }>;
}

export interface GenerateCashClosureRequest {
  startDate: string;
  endDate: string;
}

export interface CashClosureHistory {
  closureDate: string;
  salesStartDate: string;
  salesEndDate: string;
  totalSales: number;
  totalAmount: number;
  byPaymentMethod: Array<{
    method: string;
    total: number;
    count: number;
  }>;
}

@Injectable({
  providedIn: 'root'
})
export class CashClosureService {
  private readonly apiUrl = `${environment.apiUrl}/cash-closure`;

  constructor(private http: HttpClient) {}

  getSummary(startDate: Date, endDate: Date): Observable<CashClosureSummary> {
    let params = new HttpParams();
    params = params.set('startDate', startDate.toISOString());
    params = params.set('endDate', endDate.toISOString());
    
    return this.http.get<CashClosureSummary>(`${this.apiUrl}/summary`, { params });
  }

  generatePdf(request: GenerateCashClosureRequest): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/generate`, request, {
      responseType: 'blob'
    });
  }

  getHistory(startDate?: Date, endDate?: Date): Observable<CashClosureHistory[]> {
    let params = new HttpParams();
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }
    
    return this.http.get<CashClosureHistory[]>(`${this.apiUrl}/history`, { params });
  }

  downloadPdf(closureDate: Date): Observable<Blob> {
    let params = new HttpParams();
    params = params.set('closureDate', closureDate.toISOString());
    
    return this.http.get(`${this.apiUrl}/download-pdf`, { 
      params,
      responseType: 'blob'
    });
  }
}


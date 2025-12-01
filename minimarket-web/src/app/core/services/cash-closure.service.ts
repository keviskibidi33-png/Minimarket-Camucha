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
}


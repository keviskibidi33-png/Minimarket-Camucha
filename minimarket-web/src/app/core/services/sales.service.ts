import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Sale {
  id: string;
  documentNumber: string;
  documentType: 'Boleta' | 'Factura';
  saleDate: string;
  customerId?: string;
  customerName?: string;
  customerDocumentType?: string;
  customerDocumentNumber?: string;
  customerAddress?: string;
  customerEmail?: string;
  subtotal: number;
  tax: number;
  discount: number;
  total: number;
  paymentMethod: 'Efectivo' | 'Tarjeta' | 'YapePlin' | 'Transferencia';
  amountPaid: number;
  change: number;
  status: 'Pendiente' | 'Pagado' | 'Anulado';
  cancellationReason?: string;
  saleDetails: SaleDetail[];
}

export interface SaleDetail {
  id: string;
  productId: string;
  productName: string;
  productCode: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface CreateSaleDto {
  documentType: 'Boleta' | 'Factura';
  customerId?: string;
  paymentMethod: 'Efectivo' | 'Tarjeta' | 'YapePlin' | 'Transferencia';
  amountPaid: number;
  discount: number;
  saleDetails: CreateSaleDetailDto[];
}

export interface CreateSaleDetailDto {
  productId: string;
  quantity: number;
  unitPrice: number;
}

export interface CartItem {
  productId: string;
  productCode: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

@Injectable({
  providedIn: 'root'
})
export class SalesService {
  private readonly apiUrl = `${environment.apiUrl}/sales`;

  constructor(private http: HttpClient) {}

  create(sale: CreateSaleDto): Observable<Sale> {
    // El backend espera { Sale: {...} } con mayúscula
    return this.http.post<Sale>(this.apiUrl, { Sale: sale });
  }

  getAll(params?: any): Observable<import('./products.service').PagedResult<Sale>> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        if (params[key] !== undefined && params[key] !== null) {
          httpParams = httpParams.set(key, params[key].toString());
        }
      });
    }
    return this.http.get<import('./products.service').PagedResult<Sale>>(this.apiUrl, { params: httpParams });
  }

  getById(id: string): Observable<Sale> {
    return this.http.get<Sale>(`${this.apiUrl}/${id}`);
  }

  cancel(saleId: string, reason: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${saleId}/cancel`, { reason });
  }

  sendReceipt(saleId: string, email: string, documentType: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(
      `${this.apiUrl}/${saleId}/send-receipt`,
      { email, documentType }
    );
  }

  getPdfUrl(saleId: string, documentType: string = 'Boleta'): string {
    return `${this.apiUrl}/${saleId}/pdf?documentType=${documentType}`;
  }
}


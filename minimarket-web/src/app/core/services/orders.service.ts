import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CreateWebOrder {
  orderNumber: string;
  customerEmail: string;
  customerName: string;
  customerPhone?: string;
  shippingMethod: string;
  shippingAddress?: string;
  shippingCity?: string;
  shippingRegion?: string;
  selectedSedeId?: string;
  paymentMethod: string;
  walletMethod?: string;
  requiresPaymentProof: boolean;
  subtotal: number;
  shippingCost: number;
  total: number;
  items: OrderItem[];
}

export interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface WebOrder {
  id: string;
  orderNumber: string;
  customerEmail: string;
  customerName: string;
  customerPhone?: string;
  shippingMethod: string;
  shippingAddress?: string;
  shippingCity?: string;
  shippingRegion?: string;
  selectedSedeId?: string;
  paymentMethod: string;
  walletMethod?: string;
  requiresPaymentProof: boolean;
  status: string;
  subtotal: number;
  shippingCost: number;
  total: number;
  createdAt: string;
  updatedAt?: string;
  items: OrderItem[];
}

@Injectable({
  providedIn: 'root'
})
export class OrdersService {
  private readonly apiUrl = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  createOrder(order: CreateWebOrder): Observable<WebOrder> {
    return this.http.post<WebOrder>(this.apiUrl, order);
  }

  getOrderById(id: string): Observable<WebOrder> {
    return this.http.get<WebOrder>(`${this.apiUrl}/${id}`);
  }
}


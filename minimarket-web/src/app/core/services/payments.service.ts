import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CreatePaymentIntent {
  amount: number;
  currency?: string;
  description?: string;
  metadata?: { [key: string]: string };
}

export interface PaymentIntentResponse {
  clientSecret: string;
  paymentIntentId: string;
}

export interface ConfirmPayment {
  paymentIntentId: string;
  saleId: string;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentsService {
  private readonly apiUrl = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  createPaymentIntent(intent: CreatePaymentIntent): Observable<PaymentIntentResponse> {
    return this.http.post<PaymentIntentResponse>(`${this.apiUrl}/create-intent`, intent);
  }

  confirmPayment(payment: ConfirmPayment): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/confirm`, payment);
  }
}


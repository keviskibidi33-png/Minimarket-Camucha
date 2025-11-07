import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PaymentMethodSetting {
  id: string;
  paymentMethodId: number;
  name: string;
  isEnabled: boolean;
  requiresCardDetails: boolean;
  description?: string;
  displayOrder: number;
}

export interface UpdatePaymentMethodSetting {
  isEnabled: boolean;
  displayOrder: number;
  description?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentMethodSettingsService {
  private readonly apiUrl = `${environment.apiUrl}/payment-method-settings`;

  constructor(private http: HttpClient) {}

  getAll(enabledOnly: boolean = false): Observable<PaymentMethodSetting[]> {
    return this.http.get<PaymentMethodSetting[]>(this.apiUrl, {
      params: { enabledOnly: enabledOnly.toString() }
    });
  }

  update(id: string, setting: UpdatePaymentMethodSetting): Observable<PaymentMethodSetting> {
    return this.http.put<PaymentMethodSetting>(`${this.apiUrl}/${id}`, setting);
  }
}


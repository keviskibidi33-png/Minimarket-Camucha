import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface EmailTemplate {
  templateType: string;
  subject: string;
  body: string;
  logoUrl: string;
  promotionImageUrl: string;
}

export interface UpdateEmailTemplateSettings {
  logoUrl: string;
  promotionImageUrl: string;
}

export interface TestEmailRequest {
  email: string;
  templateType: 'order_confirmation' | 'order_status_update';
  customerName?: string;
  orderNumber?: string;
  total?: number;
  shippingMethod?: string;
  status?: string;
  trackingUrl?: string;
}

export interface TestEmailResponse {
  message: string;
  sent: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class EmailTemplatesService {
  private readonly apiUrl = `${environment.apiUrl}/email-templates`;

  constructor(private http: HttpClient) {}

  getTemplate(templateType: string): Observable<EmailTemplate> {
    return this.http.get<EmailTemplate>(`${this.apiUrl}/${templateType}`);
  }

  updateSettings(settings: UpdateEmailTemplateSettings): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}/settings`, settings);
  }

  sendTestEmail(request: TestEmailRequest): Observable<TestEmailResponse> {
    return this.http.post<TestEmailResponse>(`${this.apiUrl}/test`, request);
  }

  sendTestConfirmationEmail(request: {
    email: string;
    customerName?: string;
    orderNumber?: string;
    total?: number;
    shippingMethod?: string;
    estimatedDelivery?: string;
  }): Observable<TestEmailResponse> {
    return this.http.post<TestEmailResponse>(`${this.apiUrl}/test/confirmation`, request);
  }

  sendTestStatusUpdateEmail(request: {
    email: string;
    customerName?: string;
    orderNumber?: string;
    status?: string;
    trackingUrl?: string;
  }): Observable<TestEmailResponse> {
    return this.http.post<TestEmailResponse>(`${this.apiUrl}/test/status-update`, request);
  }
}


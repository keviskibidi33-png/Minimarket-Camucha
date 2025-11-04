import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ShippingCalculationRequest {
  subtotal: number;
  totalWeight: number; // Peso total en kg
  distance: number; // Distancia en km
  zoneName?: string; // Zona de envío (opcional)
  deliveryMethod: 'delivery' | 'pickup'; // 'delivery' o 'pickup'
}

export interface ShippingCalculationResponse {
  shippingCost: number;
  zoneName?: string;
  isFreeShipping: boolean;
  freeShippingReason?: string;
  calculationDetails: string;
}

@Injectable({
  providedIn: 'root'
})
export class ShippingService {
  private readonly apiUrl = `${environment.apiUrl}/shipping`;

  constructor(private http: HttpClient) {}

  calculateShipping(request: ShippingCalculationRequest): Observable<ShippingCalculationResponse> {
    return this.http.post<ShippingCalculationResponse>(`${this.apiUrl}/calculate`, request);
  }

  // Calcular distancia entre dos coordenadas (Haversine formula)
  calculateDistance(lat1: number, lon1: number, lat2: number, lon2: number): number {
    const R = 6371; // Radio de la Tierra en km
    const dLat = this.toRad(lat2 - lat1);
    const dLon = this.toRad(lon2 - lon1);
    const a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
              Math.cos(this.toRad(lat1)) * Math.cos(this.toRad(lat2)) *
              Math.sin(dLon / 2) * Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return R * c;
  }

  private toRad(degrees: number): number {
    return degrees * (Math.PI / 180);
  }
}


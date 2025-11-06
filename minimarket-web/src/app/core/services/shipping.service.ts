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

export interface ShippingRate {
  id: string;
  zoneName: string;
  basePrice: number;
  pricePerKm: number;
  pricePerKg: number;
  minDistance: number;
  maxDistance: number;
  minWeight: number;
  maxWeight: number;
  freeShippingThreshold: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateShippingRate {
  zoneName: string;
  basePrice: number;
  pricePerKm: number;
  pricePerKg: number;
  minDistance: number;
  maxDistance: number;
  minWeight: number;
  maxWeight: number;
  freeShippingThreshold: number;
  isActive: boolean;
}

export interface UpdateShippingRate {
  zoneName: string;
  basePrice: number;
  pricePerKm: number;
  pricePerKg: number;
  minDistance: number;
  maxDistance: number;
  minWeight: number;
  maxWeight: number;
  freeShippingThreshold: number;
  isActive: boolean;
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

  // Shipping Rates Management
  getAllRates(onlyActive?: boolean): Observable<ShippingRate[]> {
    let params = new HttpParams();
    if (onlyActive !== undefined) {
      params = params.set('onlyActive', onlyActive.toString());
    }
    return this.http.get<ShippingRate[]>(`${this.apiUrl}/rates`, { params });
  }

  getRateById(id: string): Observable<ShippingRate> {
    return this.http.get<ShippingRate>(`${this.apiUrl}/rates/${id}`);
  }

  createRate(rate: CreateShippingRate): Observable<ShippingRate> {
    return this.http.post<ShippingRate>(`${this.apiUrl}/rates`, rate);
  }

  updateRate(id: string, rate: UpdateShippingRate): Observable<ShippingRate> {
    return this.http.put<ShippingRate>(`${this.apiUrl}/rates/${id}`, rate);
  }

  deleteRate(id: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/rates/${id}`);
  }
}


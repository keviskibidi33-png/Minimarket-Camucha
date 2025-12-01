import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
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
  trackingUrl?: string;
  estimatedDelivery?: string;
  paymentProofUrl?: string;
  createdAt: string;
  updatedAt?: string;
  items: OrderItem[];
}

export interface GetAllOrdersParams {
  status?: string;
  searchTerm?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
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
    // El interceptor de autenticación ya agrega los headers necesarios (Accept y Authorization)
    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      map((response: any) => {
        console.log('HTTP Response received:', response);
        console.log('HTTP Response type:', typeof response);
        console.log('HTTP Response is null?', response === null);
        console.log('HTTP Response is undefined?', response === undefined);
        
        // Si la respuesta es null o undefined, lanzar un error inmediatamente
        if (response === null || response === undefined) {
          console.error('Response is null or undefined!');
          throw new Error('El servidor devolvió una respuesta vacía. El pedido puede no existir o no pertenecer al usuario.');
        }
        
        // Convertir el ID de Guid a string si es necesario
        const orderId = response.id || response.Id || id;
        const orderIdString = typeof orderId === 'string' ? orderId : orderId.toString();
        
        const mappedOrder: WebOrder = {
          id: orderIdString,
          orderNumber: response.orderNumber || response.OrderNumber || '',
          customerEmail: response.customerEmail || response.CustomerEmail || '',
          customerName: response.customerName || response.CustomerName || '',
          customerPhone: response.customerPhone || response.CustomerPhone,
          shippingMethod: response.shippingMethod || response.ShippingMethod || '',
          shippingAddress: response.shippingAddress || response.ShippingAddress,
          shippingCity: response.shippingCity || response.ShippingCity,
          shippingRegion: response.shippingRegion || response.ShippingRegion,
          selectedSedeId: response.selectedSedeId || response.SelectedSedeId,
          paymentMethod: response.paymentMethod || response.PaymentMethod || '',
          walletMethod: response.walletMethod || response.WalletMethod,
          requiresPaymentProof: response.requiresPaymentProof || response.RequiresPaymentProof || false,
          status: response.status || response.Status || '',
          subtotal: response.subtotal || response.Subtotal || 0,
          shippingCost: response.shippingCost || response.ShippingCost || 0,
          total: response.total || response.Total || 0,
          trackingUrl: response.trackingUrl || response.TrackingUrl,
          estimatedDelivery: response.estimatedDelivery || response.EstimatedDelivery,
          paymentProofUrl: response.paymentProofUrl || response.PaymentProofUrl,
          createdAt: response.createdAt || response.CreatedAt || '',
          updatedAt: response.updatedAt || response.UpdatedAt,
          items: (response.items || response.Items || []).map((item: any) => ({
            productId: (item.productId || item.ProductId || '').toString(),
            productName: item.productName || item.ProductName || '',
            quantity: item.quantity || item.Quantity || 0,
            unitPrice: item.unitPrice || item.UnitPrice || 0,
            subtotal: item.subtotal || item.Subtotal || 0
          }))
        };
        
        console.log('Mapped order:', mappedOrder);
        return mappedOrder;
      }),
      catchError((error: any) => {
        console.error('Error in getOrderById:', error);
        
        // Si es un HttpErrorResponse, usar sus propiedades
        if (error instanceof HttpErrorResponse) {
          console.error('HTTP Error status:', error.status);
          console.error('HTTP Error body:', error.error);
          
          let errorMessage = 'Error al cargar los detalles del pedido';
          
          if (error.status === 404) {
            errorMessage = 'Pedido no encontrado';
          } else if (error.status === 403) {
            errorMessage = 'No tienes permiso para ver este pedido';
          } else if (error.status === 401) {
            errorMessage = 'No estás autenticado. Por favor, inicia sesión.';
          } else if (error.error?.message) {
            errorMessage = error.error.message;
          } else if (error.message) {
            errorMessage = error.message;
          }
          
          return throwError(() => ({
            status: error.status,
            statusText: error.statusText,
            error: error.error,
            message: errorMessage
          }));
        }
        
        // Si es un error lanzado manualmente (desde el map)
        return throwError(() => ({
          status: undefined,
          statusText: undefined,
          error: undefined,
          message: error.message || 'Error al cargar los detalles del pedido'
        }));
      })
    );
  }

  getUserOrders(): Observable<WebOrder[]> {
    return this.http.get<WebOrder[]>(`${this.apiUrl}/my-orders`);
  }

  getAllOrders(params?: GetAllOrdersParams): Observable<PagedResult<WebOrder>> {
    let httpParams = new HttpParams();
    
    if (params?.status) {
      httpParams = httpParams.set('status', params.status);
    }
    if (params?.searchTerm) {
      httpParams = httpParams.set('searchTerm', params.searchTerm);
    }
    if (params?.startDate) {
      httpParams = httpParams.set('startDate', params.startDate);
    }
    if (params?.endDate) {
      httpParams = httpParams.set('endDate', params.endDate);
    }
    if (params?.page) {
      httpParams = httpParams.set('page', params.page.toString());
    }
    if (params?.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<PagedResult<WebOrder>>(`${this.apiUrl}/admin/all`, { params: httpParams });
  }

  updateOrderStatus(orderId: string, status: string, trackingUrl?: string, estimatedDelivery?: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${orderId}/status`, {
      status,
      trackingUrl,
      estimatedDelivery
    });
  }

  updatePaymentProof(orderId: string, paymentProofUrl: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${orderId}/payment-proof`, {
      paymentProofUrl
    });
  }

  approveOrder(orderId: string, sendPaymentVerifiedEmail: boolean = false): Observable<any> {
    return this.http.post(`${this.apiUrl}/${orderId}/approve`, {
      sendPaymentVerifiedEmail
    });
  }

  rejectOrder(orderId: string, reason: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${orderId}/reject`, {
      reason
    });
  }

  markOrderAsPickedUp(id: string, rating: number, comment?: string, wouldRecommend: boolean = true): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/mark-as-picked-up`, {
      rating,
      comment,
      wouldRecommend
    });
  }
}


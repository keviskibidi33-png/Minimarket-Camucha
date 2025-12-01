import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface DocumentViewSettings {
  defaultViewMode: 'preview' | 'direct';
  directPrint: boolean;
  boletaTemplateActive: boolean;
  facturaTemplateActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class DocumentSettingsService {
  private readonly apiUrl = `${environment.apiUrl}/document-settings`;

  constructor(private http: HttpClient) {}

  getViewSettings(): Observable<DocumentViewSettings> {
    return this.http.get<DocumentViewSettings>(`${this.apiUrl}/view-settings`).pipe(
      catchError((error) => {
        // Si es 404, devolver valores por defecto en lugar de error
        if (error.status === 404) {
          return of({
            defaultViewMode: 'preview' as const,
            directPrint: false,
            boletaTemplateActive: true,
            facturaTemplateActive: true
          });
        }
        // Para otros errores, re-lanzar
        throw error;
      })
    );
  }

  updateViewSettings(settings: DocumentViewSettings): Observable<DocumentViewSettings> {
    // El backend espera el objeto envuelto en una propiedad "settings"
    return this.http.put<DocumentViewSettings>(`${this.apiUrl}/view-settings`, { settings }).pipe(
      catchError((error) => {
        console.error('Error updating view settings:', error);
        // Re-lanzar el error para que el componente pueda manejarlo
        throw error;
      })
    );
  }

  getPreviewPdf(documentType: 'Boleta' | 'Factura' = 'Boleta', settings?: any): Observable<Blob> {
    const requestBody = {
      documentType: documentType,
      settings: settings ? {
        companyName: settings.companyName,
        companyRuc: settings.companyRuc,
        companyAddress: settings.companyAddress,
        companyPhone: settings.companyPhone,
        companyEmail: settings.companyEmail,
        logoUrl: settings.logoUrl
      } : null
    };

    return this.http.post(`${this.apiUrl}/preview-pdf`, requestBody, {
      responseType: 'blob'
    }).pipe(
      catchError((error) => {
        console.error('Error getting preview PDF:', error);
        throw error;
      })
    );
  }
}


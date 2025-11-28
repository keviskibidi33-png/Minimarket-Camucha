import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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
    return this.http.get<DocumentViewSettings>(`${this.apiUrl}/view-settings`);
  }

  updateViewSettings(settings: DocumentViewSettings): Observable<DocumentViewSettings> {
    return this.http.put<DocumentViewSettings>(`${this.apiUrl}/view-settings`, settings);
  }
}


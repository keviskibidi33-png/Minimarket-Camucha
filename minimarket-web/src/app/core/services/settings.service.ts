import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SystemSettings {
  id: string;
  key: string;
  value: string;
  description: string;
  category: string;
  isActive: boolean;
}

export interface UpdateSystemSettings {
  key: string;
  value: string;
  description?: string;
  isActive: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private readonly apiUrl = `${environment.apiUrl}/settings`;

  constructor(private http: HttpClient) {}

  getAll(category?: string): Observable<SystemSettings[]> {
    let params = new HttpParams();
    if (category) {
      params = params.set('category', category);
    }
    return this.http.get<SystemSettings[]>(this.apiUrl, { params });
  }

  getByKey(key: string): Observable<SystemSettings | null> {
    return this.http.get<SystemSettings | null>(`${this.apiUrl}/${key}`);
  }

  update(key: string, setting: UpdateSystemSettings): Observable<SystemSettings> {
    return this.http.put<SystemSettings>(`${this.apiUrl}/${key}`, setting);
  }

  // Helper para obtener configuración específica
  getSettingValue(key: string, defaultValue: string = ''): Observable<string> {
    return new Observable(observer => {
      this.getByKey(key).subscribe({
        next: (setting) => {
          observer.next(setting?.value || defaultValue);
          observer.complete();
        },
        error: () => {
          observer.next(defaultValue);
          observer.complete();
        }
      });
    });
  }

  // Helper para verificar si una configuración está activa
  isSettingEnabled(key: string, defaultValue: boolean = false): Observable<boolean> {
    return new Observable(observer => {
      this.getByKey(key).subscribe({
        next: (setting) => {
          if (!setting) {
            observer.next(defaultValue);
          } else {
            observer.next(setting.value.toLowerCase() === 'true' || setting.value === '1');
          }
          observer.complete();
        },
        error: () => {
          observer.next(defaultValue);
          observer.complete();
        }
      });
    });
  }
}


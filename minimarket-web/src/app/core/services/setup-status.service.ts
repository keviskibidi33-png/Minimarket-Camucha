import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { BrandSettingsService } from './brand-settings.service';

@Injectable({
  providedIn: 'root'
})
export class SetupStatusService {
  private readonly apiUrl = `${environment.apiUrl}/brand-settings`;
  private readonly setupCompleteKey = 'admin_setup_complete';
  
  isSetupComplete = signal<boolean | null>(null);

  constructor(
    private http: HttpClient,
    private brandSettingsService: BrandSettingsService
  ) {
    // Verificar estado guardado en localStorage
    const savedStatus = localStorage.getItem(this.setupCompleteKey);
    if (savedStatus === 'true') {
      this.isSetupComplete.set(true);
    } else if (savedStatus === 'false') {
      this.isSetupComplete.set(false);
    }
  }

  /**
   * Verifica si el setup está completo revisando BrandSettings
   */
  checkSetupComplete(): Observable<boolean> {
    return this.brandSettingsService.get().pipe(
      map(settings => {
        if (!settings) {
          return false;
        }

        // Verificar campos esenciales
        const hasStoreName = !!settings.storeName && settings.storeName.trim().length > 0;
        const hasPhone = !!settings.phone && settings.phone.trim().length > 0;
        const hasPrimaryColor = !!settings.primaryColor;
        const hasSecondaryColor = !!settings.secondaryColor;

        // El setup está completo si tiene al menos nombre de tienda, teléfono y colores
        const complete = hasStoreName && hasPhone && hasPrimaryColor && hasSecondaryColor;
        
        this.isSetupComplete.set(complete);
        localStorage.setItem(this.setupCompleteKey, complete.toString());
        
        return complete;
      }),
      catchError(() => {
        // En caso de error, asumir que no está completo
        this.isSetupComplete.set(false);
        localStorage.setItem(this.setupCompleteKey, 'false');
        return of(false);
      })
    );
  }

  /**
   * Marca el setup como completo
   */
  markSetupComplete(): void {
    this.isSetupComplete.set(true);
    localStorage.setItem(this.setupCompleteKey, 'true');
  }

  /**
   * Marca el setup como incompleto
   */
  markSetupIncomplete(): void {
    this.isSetupComplete.set(false);
    localStorage.setItem(this.setupCompleteKey, 'false');
  }

  /**
   * Obtiene el estado actual (sin hacer petición HTTP)
   */
  getSetupStatus(): boolean | null {
    return this.isSetupComplete();
  }
}


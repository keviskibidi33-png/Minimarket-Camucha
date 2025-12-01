import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface BrandSettings {
  id: string;
  logoUrl: string;
  logoEmoji?: string;
  storeName: string;
  faviconUrl?: string;
  primaryColor: string;
  secondaryColor: string;
  buttonColor: string;
  textColor: string;
  hoverColor: string;
  description?: string;
  slogan?: string;
  phone?: string;
  whatsAppPhone?: string;
  email?: string;
  address?: string;
  ruc?: string;
  // Métodos de pago
  yapePhone?: string;
  plinPhone?: string;
  yapeQRUrl?: string;
  plinQRUrl?: string;
  yapeEnabled: boolean;
  plinEnabled: boolean;
  // Cuenta bancaria
  bankName?: string;
  bankAccountType?: string;
  bankAccountNumber?: string;
  bankCCI?: string;
  bankAccountVisible: boolean;
  // Opciones de envío
  deliveryType: string;
  deliveryCost?: number;
  deliveryZones?: string;
  // Personalización de página principal
  homeTitle?: string;
  homeSubtitle?: string;
  homeDescription?: string;
  homeBannerImageUrl?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface UpdateBrandSettings {
  logoUrl: string;
  logoEmoji?: string;
  storeName: string;
  faviconUrl?: string;
  primaryColor: string;
  secondaryColor: string;
  buttonColor: string;
  textColor: string;
  hoverColor: string;
  description?: string;
  slogan?: string;
  phone?: string;
  whatsAppPhone?: string;
  email?: string;
  address?: string;
  ruc?: string;
  // Métodos de pago
  yapePhone?: string;
  plinPhone?: string;
  yapeQRUrl?: string;
  plinQRUrl?: string;
  yapeEnabled?: boolean;
  plinEnabled?: boolean;
  // Cuenta bancaria
  bankName?: string;
  bankAccountType?: string;
  bankAccountNumber?: string;
  bankCCI?: string;
  bankAccountVisible?: boolean;
  // Opciones de envío
  deliveryType?: string;
  deliveryCost?: number;
  deliveryZones?: string;
  // Personalización de página principal
  homeTitle?: string;
  homeSubtitle?: string;
  homeDescription?: string;
  homeBannerImageUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class BrandSettingsService {
  private readonly apiUrl = `${environment.apiUrl}/brandsettings`;
  private settingsSubject = new BehaviorSubject<BrandSettings | null>(null);
  public settings$ = this.settingsSubject.asObservable();

  constructor(private http: HttpClient) {
    // No cargar settings en el constructor para evitar problemas de inyección
    // Se cargarán cuando se llame explícitamente a get()
  }

  get(): Observable<BrandSettings | null> {
    return this.http.get<BrandSettings | null>(this.apiUrl).pipe(
      tap(settings => {
        this.settingsSubject.next(settings);
        // Aplicar estilos globales si hay settings
        if (settings) {
          this.applyStyles(settings);
        }
      })
    );
  }

  update(settings: UpdateBrandSettings): Observable<BrandSettings> {
    // El backend espera UpdateBrandSettingsCommand con una propiedad BrandSettings
    // Envolver el objeto en la estructura correcta
    const command = {
      brandSettings: settings
    };
    
    console.log('📤 Enviando al backend (wrapped):', JSON.stringify(command, null, 2));
    
    return this.http.put<BrandSettings>(this.apiUrl, command).pipe(
      tap(updatedSettings => {
        this.settingsSubject.next(updatedSettings);
        this.applyStyles(updatedSettings);
      })
    );
  }

  getCurrentSettings(): BrandSettings | null {
    return this.settingsSubject.value;
  }

  private loadSettings(): void {
    this.get().subscribe();
  }

  private applyStyles(settings: BrandSettings | UpdateBrandSettings): void {
    // Aplicar variables CSS dinámicas
    const root = document.documentElement;
    root.style.setProperty('--primary-color', settings.primaryColor);
    root.style.setProperty('--secondary-color', settings.secondaryColor);
    root.style.setProperty('--button-color', settings.buttonColor);
    root.style.setProperty('--text-color', settings.textColor);
    root.style.setProperty('--hover-color', settings.hoverColor);
  }
}


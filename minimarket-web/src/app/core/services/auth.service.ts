import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName?: string;
  lastName?: string;
  dni?: string;
  phone?: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface LoginResponse {
  token: string;
  expiration: string;
  userId: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  roles: string[];
  profileCompleted?: boolean;
}

export interface User {
  id: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly tokenKey = 'auth_token';
  private readonly userKey = 'auth_user';

  // Signals para estado reactivo
  public currentUser = signal<User | null>(this.getStoredUser());
  public isAuthenticated = signal<boolean>(this.hasToken());

  constructor(private http: HttpClient) {
    this.loadStoredAuth();
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        this.storeAuth(response);
        this.currentUser.set({
          id: response.userId,
          firstName: response.firstName,
          lastName: response.lastName,
          email: response.email,
          roles: response.roles
        });
        this.isAuthenticated.set(true);
        
        // Cargar perfil completo para obtener firstName y lastName si no están en la respuesta
        if (!response.firstName || !response.lastName) {
          this.loadUserProfilePrivate();
        }
      })
    );
  }

  private googleInitialized = false;

  initializeGoogleSignIn(): void {
    // Evitar inicialización múltiple
    if (this.googleInitialized) {
      return;
    }

    // Verificar si Google Identity Services está cargado
    if (typeof (window as any).google === 'undefined') {
      console.error('Google Identity Services no está cargado');
      return;
    }

    const clientId = environment.googleClientId;
    if (!clientId) {
      console.error('Google ClientId no está configurado');
      return;
    }

    // Log para debugging
    console.log('Inicializando Google Sign-In con Client ID:', clientId);
    console.log('Origen actual:', window.location.origin);

    // Inicializar Google Sign-In una sola vez
    try {
      (window as any).google.accounts.id.initialize({
        client_id: clientId,
        callback: (response: any) => {
          console.log('Google Sign-In callback recibido');
          this.handleGoogleSignIn(response.credential).subscribe({
            next: (loginResponse) => {
              this.storeAuth(loginResponse);
              this.currentUser.set({
                id: loginResponse.userId,
                firstName: loginResponse.firstName,
                lastName: loginResponse.lastName,
                email: loginResponse.email,
                roles: loginResponse.roles
              });
              this.isAuthenticated.set(true);
              
              // Cargar perfil completo para obtener firstName y lastName si no están en la respuesta
              if (!loginResponse.firstName || !loginResponse.lastName) {
                this.loadUserProfilePrivate();
              }
              
              // Redirigir según el estado del perfil y rol
              const isAdmin = loginResponse.roles?.includes('Administrador');
              if (loginResponse.profileCompleted === false) {
                if (isAdmin) {
                  window.location.href = '/auth/admin-setup';
                } else {
                  window.location.href = '/auth/complete-profile';
                }
              } else {
                if (isAdmin) {
                  window.location.href = '/admin';
                } else {
                  window.location.href = '/';
                }
              }
            },
            error: (error) => {
              console.error('Error en Google Sign-In:', error);
              alert('Error al iniciar sesión con Google. Por favor, intenta nuevamente.');
            }
          });
        },
        error_callback: (error: any) => {
          console.error('Error en inicialización de Google Sign-In:', error);
        }
      });
      this.googleInitialized = true;
    } catch (error) {
      console.error('Error al inicializar Google Sign-In:', error);
    }
  }

  loginWithGoogle(): void {
    // Asegurar que Google Identity Services esté inicializado
    this.initializeGoogleSignIn();
    
    // Intentar mostrar One Tap
    if (typeof (window as any).google !== 'undefined' && (window as any).google.accounts) {
      try {
        (window as any).google.accounts.id.prompt((notification: any) => {
          // Si One Tap no se puede mostrar, el usuario puede usar el botón renderizado
          if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
            console.log('One Tap no disponible, usar botón de Google');
          }
        });
      } catch (error) {
        console.log('Error al mostrar One Tap:', error);
      }
    }
  }

  renderGoogleButton(elementId: string): void {
    // Inicializar primero
    this.initializeGoogleSignIn();
    
    // Renderizar el botón de Google
    if (typeof (window as any).google !== 'undefined' && (window as any).google.accounts) {
      const element = document.getElementById(elementId);
      if (element) {
        // Obtener el ancho del contenedor en píxeles
        const containerWidth = element.offsetWidth || 400;
        
        (window as any).google.accounts.id.renderButton(element, {
          theme: 'outline',
          size: 'large',
          width: containerWidth, // Usar número en lugar de porcentaje
          text: 'signin_with',
          locale: 'es'
        });
      }
    }
  }

  private handleGoogleSignIn(credential: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/google-signin`, { credential });
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  private hasToken(): boolean {
    return !!this.getToken();
  }

  private storeAuth(response: LoginResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
      localStorage.setItem(this.userKey, JSON.stringify({
        id: response.userId,
        firstName: response.firstName,
        lastName: response.lastName,
        email: response.email,
        roles: response.roles
      }));
  }

  private getStoredUser(): User | null {
    const userStr = localStorage.getItem(this.userKey);
    if (!userStr) return null;
    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  }

  private loadStoredAuth(): void {
    if (this.hasToken() && this.currentUser()) {
      this.isAuthenticated.set(true);
      // Cargar perfil completo si firstName o lastName no están disponibles
      if (!this.currentUser()?.firstName || !this.currentUser()?.lastName) {
        this.loadUserProfilePrivate();
      }
    }
  }

  loadUserProfile(): Observable<UserProfile> {
    return this.getProfile().pipe(
      tap(profile => {
        const current = this.currentUser();
        if (current && profile) {
          this.currentUser.set({
            ...current,
            firstName: profile.firstName || current.firstName,
            lastName: profile.lastName || current.lastName
          });
          // Actualizar localStorage
          localStorage.setItem(this.userKey, JSON.stringify(this.currentUser()));
        }
      })
    );
  }

  private loadUserProfilePrivate(): void {
    this.getProfile().subscribe({
      next: (profile) => {
        const current = this.currentUser();
        if (current && profile) {
          this.currentUser.set({
            ...current,
            firstName: profile.firstName || current.firstName,
            lastName: profile.lastName || current.lastName
          });
          // Actualizar también en localStorage
          const updatedUser = this.currentUser();
          if (updatedUser) {
            localStorage.setItem(this.userKey, JSON.stringify(updatedUser));
          }
        }
      },
      error: (error) => {
        console.error('Error al cargar perfil del usuario:', error);
        // No hacer nada si falla, el usuario ya está autenticado
      }
    });
  }

  register(credentials: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/register`, credentials).pipe(
      tap(response => {
        this.storeAuth(response);
        this.currentUser.set({
          id: response.userId,
          firstName: response.firstName,
          lastName: response.lastName,
          email: response.email,
          roles: response.roles
        });
        this.isAuthenticated.set(true);
        
        // Cargar perfil completo para obtener firstName y lastName si no están en la respuesta
        if (!response.firstName || !response.lastName) {
          this.loadUserProfilePrivate();
        }
      })
    );
  }

  handleGoogleCallback(token: string): void {
    // Decodificar el token JWT para obtener la información del usuario
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const loginResponse: LoginResponse = {
        token: token,
        expiration: new Date(payload.exp * 1000).toISOString(),
        userId: payload.sub || payload.nameid,
        firstName: payload.firstName || payload.given_name,
        lastName: payload.lastName || payload.family_name,
        email: payload.email,
        roles: Array.isArray(payload.role) ? payload.role : payload.role ? [payload.role] : [],
        profileCompleted: payload.profileCompleted || false
      };

      this.storeAuth(loginResponse);
      this.currentUser.set({
        id: loginResponse.userId,
        firstName: loginResponse.firstName,
        lastName: loginResponse.lastName,
        email: loginResponse.email,
        roles: loginResponse.roles
      });
      this.isAuthenticated.set(true);
    } catch (error) {
      console.error('Error al procesar token de Google:', error);
    }
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/forgot-password`, request);
  }

  resetPassword(token: string, email: string, newPassword: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/reset-password`, {
      email,
      token,
      newPassword
    });
  }

  completeProfile(profileData: { 
    phone: string;
    // firstName, lastName y dni ya están en el perfil desde el registro
    address?: {
      label: string;
      fullName: string;
      phone: string;
      address: string;
      reference?: string;
      district: string;
      city: string;
      region: string;
      postalCode?: string;
      latitude?: number;
      longitude?: number;
      isDefault?: boolean;
    };
  }): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/complete-profile`, profileData);
  }

  // Perfil de usuario
  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`);
  }

  updateProfile(profileData: { firstName: string; lastName: string; phone: string }): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/profile`, profileData);
  }

  // Métodos de pago
  getPaymentMethods(): Observable<PaymentMethod[]> {
    return this.http.get<PaymentMethod[]>(`${this.apiUrl}/payment-methods`);
  }

  addPaymentMethod(paymentMethod: {
    cardHolderName: string;
    cardNumber: string;
    expiryMonth: number;
    expiryYear: number;
    isDefault?: boolean;
  }): Observable<PaymentMethod> {
    return this.http.post<PaymentMethod>(`${this.apiUrl}/payment-methods`, paymentMethod);
  }

  updatePaymentMethod(id: string, paymentMethod: {
    cardHolderName: string;
    expiryMonth: number;
    expiryYear: number;
    isDefault: boolean;
  }): Observable<PaymentMethod> {
    return this.http.put<PaymentMethod>(`${this.apiUrl}/payment-methods/${id}`, paymentMethod);
  }

  deletePaymentMethod(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/payment-methods/${id}`);
  }

  // Direcciones de envío
  getAddresses(): Observable<UserAddress[]> {
    return this.http.get<UserAddress[]>(`${this.apiUrl}/addresses`);
  }

  addAddress(address: {
    label: string;
    fullName: string;
    phone: string;
    address: string;
    reference?: string;
    district: string;
    city: string;
    region: string;
    postalCode?: string;
    latitude?: number;
    longitude?: number;
    isDefault?: boolean;
  }): Observable<UserAddress> {
    return this.http.post<UserAddress>(`${this.apiUrl}/addresses`, address);
  }

  updateAddress(id: string, address: {
    label: string;
    fullName: string;
    phone: string;
    address: string;
    reference?: string;
    district: string;
    city: string;
    region: string;
    postalCode?: string;
    latitude?: number;
    longitude?: number;
    isDefault: boolean;
  }): Observable<UserAddress> {
    return this.http.put<UserAddress>(`${this.apiUrl}/addresses/${id}`, address);
  }

  deleteAddress(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/addresses/${id}`);
  }
}

export interface PaymentMethod {
  id: string;
  cardHolderName: string;
  cardNumberMasked: string;
  cardType: string;
  expiryMonth: number;
  expiryYear: number;
  isDefault: boolean;
  last4Digits?: string;
}

export interface UserProfile {
  firstName?: string;
  lastName?: string;
  dni?: string;
  phone?: string;
  email?: string;
  profileCompleted?: boolean;
}

export interface UserAddress {
  id: string;
  label: string;
  isDifferentRecipient: boolean;
  fullName: string;
  firstName?: string;
  lastName?: string;
  dni?: string;
  phone: string;
  address: string;
  reference?: string;
  district: string;
  city: string;
  region: string;
  postalCode?: string;
  latitude?: number;
  longitude?: number;
  isDefault: boolean;
}



import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  private readonly apiUrl = `${environment.apiUrl}/translations`;
  private currentLanguage = new BehaviorSubject<string>('es');
  private translations = new BehaviorSubject<{ [key: string]: string }>({});
  
  public currentLanguage$ = this.currentLanguage.asObservable();
  public translations$ = this.translations.asObservable();

  constructor(private http: HttpClient) {
    // Cargar traducciones por defecto (español)
    this.loadTranslations('es');
  }

  loadTranslations(languageCode: string, category?: string): void {
    let url = `${this.apiUrl}/${languageCode}`;
    if (category) {
      url += `?category=${category}`;
    }

    this.http.get<{ [key: string]: string }>(url).subscribe({
      next: (translations) => {
        this.translations.next(translations);
        this.currentLanguage.next(languageCode);
      },
      error: (error) => {
        console.error('Error loading translations:', error);
        // Si falla, mantener traducciones actuales
      }
    });
  }

  translate(key: string, defaultValue?: string): string {
    const translations = this.translations.value;
    return translations[key] || defaultValue || key;
  }

  setLanguage(languageCode: string): void {
    if (this.currentLanguage.value !== languageCode) {
      this.loadTranslations(languageCode);
    }
  }

  getCurrentLanguage(): string {
    return this.currentLanguage.value;
  }
}


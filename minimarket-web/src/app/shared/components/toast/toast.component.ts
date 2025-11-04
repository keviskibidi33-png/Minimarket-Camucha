import { Component, signal, NgZone, DestroyRef, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.css'
})
export class ToastComponent implements OnInit {
  message = signal<string>('');
  type = signal<ToastType>('info');
  isVisible = signal(false);
  
  private readonly ngZone = inject(NgZone);
  private readonly destroyRef = inject(DestroyRef);
  private hideTimeoutId?: ReturnType<typeof setTimeout>;

  show(message: string, type: ToastType = 'info', duration: number = 3000): void {
    // Limpiar timeout anterior si existe
    if (this.hideTimeoutId) {
      clearTimeout(this.hideTimeoutId);
    }

    this.message.set(message);
    this.type.set(type);
    this.isVisible.set(true);

    // Usar NgZone.run para asegurar que setTimeout se ejecute dentro de la zona de Angular
    this.ngZone.run(() => {
      this.hideTimeoutId = setTimeout(() => {
        this.hide();
      }, duration);
    });
  }

  hide(): void {
    // Limpiar timeout si existe
    if (this.hideTimeoutId) {
      clearTimeout(this.hideTimeoutId);
      this.hideTimeoutId = undefined;
    }
    this.isVisible.set(false);
  }

  ngOnInit(): void {
    // Limpiar timeout cuando el componente se destruya
    this.destroyRef.onDestroy(() => {
      if (this.hideTimeoutId) {
        clearTimeout(this.hideTimeoutId);
      }
    });
  }

  getIcon(): string {
    switch (this.type()) {
      case 'success':
        return 'check_circle';
      case 'error':
        return 'error';
      case 'warning':
        return 'warning';
      default:
        return 'info';
    }
  }

  getColorClasses(): string {
    switch (this.type()) {
      case 'success':
        return 'bg-primary text-white border-primary shadow-lg shadow-primary/50';
      case 'error':
        return 'bg-red-500/95 dark:bg-red-600/95 border-red-600 dark:border-red-500 text-white shadow-red-500/50';
      case 'warning':
        return 'bg-yellow-500/95 dark:bg-yellow-600/95 border-yellow-600 dark:border-yellow-500 text-white shadow-yellow-500/50';
      default:
        return 'bg-blue-500/95 dark:bg-blue-600/95 border-blue-600 dark:border-blue-500 text-white shadow-blue-500/50';
    }
  }
}


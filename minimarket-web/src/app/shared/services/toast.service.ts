import { Injectable } from '@angular/core';
import { ToastComponent, ToastType } from '../components/toast/toast.component';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toastComponent?: ToastComponent;

  register(toastComponent: ToastComponent): void {
    this.toastComponent = toastComponent;
  }

  show(message: string, type: ToastType = 'info', duration?: number): void {
    this.toastComponent?.show(message, type, duration);
  }

  success(message: string, duration?: number): void {
    this.show(message, 'success', duration);
  }

  error(message: string, duration?: number): void {
    this.show(message, 'error', duration);
  }

  warning(message: string, duration?: number): void {
    this.show(message, 'warning', duration);
  }

  info(message: string, duration?: number): void {
    this.show(message, 'info', duration);
  }
}


import { Component, signal, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.css'
})
export class ToastComponent {
  message = signal<string>('');
  type = signal<ToastType>('info');
  isVisible = signal(false);

  show(message: string, type: ToastType = 'info', duration: number = 3000): void {
    this.message.set(message);
    this.type.set(type);
    this.isVisible.set(true);

    setTimeout(() => {
      this.hide();
    }, duration);
  }

  hide(): void {
    this.isVisible.set(false);
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
        return 'bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800 text-green-800 dark:text-green-300';
      case 'error':
        return 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800 text-red-800 dark:text-red-300';
      case 'warning':
        return 'bg-yellow-50 dark:bg-yellow-900/20 border-yellow-200 dark:border-yellow-800 text-yellow-800 dark:text-yellow-300';
      default:
        return 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800 text-blue-800 dark:text-blue-300';
    }
  }
}


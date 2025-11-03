import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex flex-col items-center justify-center p-8">
      <span class="material-symbols-outlined animate-spin text-primary-admin text-4xl mb-4">
        refresh
      </span>
      @if (message()) {
        <p class="text-gray-500 dark:text-gray-400">{{ message() }}</p>
      }
    </div>
  `
})
export class LoadingSpinnerComponent {
  message = input<string>('Cargando...');
}


import { Component, signal, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css'
})
export class ConfirmDialogComponent {
  title = input<string>('Confirmar');
  message = input<string>('¿Está seguro de realizar esta acción?');
  confirmText = input<string>('Confirmar');
  cancelText = input<string>('Cancelar');
  isOpen = signal(false);

  confirmed = output<void>();
  cancelled = output<void>();

  open(): void {
    this.isOpen.set(true);
  }

  close(): void {
    this.isOpen.set(false);
  }

  confirm(): void {
    this.confirmed.emit();
    this.close();
  }

  cancel(): void {
    this.cancelled.emit();
    this.close();
  }
}


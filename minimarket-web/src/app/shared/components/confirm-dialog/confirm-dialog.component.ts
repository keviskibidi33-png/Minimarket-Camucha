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
  isOpen = input<boolean>(false);

  confirmed = output<void>();
  cancelled = output<void>();

  confirm(): void {
    this.confirmed.emit();
  }

  cancel(): void {
    this.cancelled.emit();
  }
}


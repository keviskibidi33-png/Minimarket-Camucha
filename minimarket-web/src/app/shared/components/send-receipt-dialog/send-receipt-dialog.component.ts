import { Component, signal, input, output, effect, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-send-receipt-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './send-receipt-dialog.component.html',
  styleUrl: './send-receipt-dialog.component.css'
})
export class SendReceiptDialogComponent {
  isOpen = input.required<boolean>();
  saleNumber = input<string>('');
  documentType = input<string>('Boleta');
  customerEmail = input<string | null>(null);
  
  onClose = output<void>();
  onSend = output<{ email: string }>();

  sendForm: FormGroup;
  private readonly destroyRef = inject(DestroyRef);
  private effectCleanup?: ReturnType<typeof effect>;

  constructor(private fb: FormBuilder) {
    this.sendForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    // Actualizar email cuando cambie customerEmail
    // Ejecutar effect directamente en el constructor (contexto válido)
    this.effectCleanup = effect(() => {
      const email = this.customerEmail();
      if (email && this.isOpen()) {
        this.sendForm.patchValue({ email });
      }
    }, { allowSignalWrites: true });

    // Limpiar el effect cuando el componente se destruya
    this.destroyRef.onDestroy(() => {
      this.effectCleanup?.destroy();
    });
  }

  close(): void {
    this.sendForm.reset();
    this.onClose.emit();
  }

  send(): void {
    if (this.sendForm.valid) {
      this.onSend.emit({ email: this.sendForm.get('email')?.value });
      this.close();
    }
  }
}


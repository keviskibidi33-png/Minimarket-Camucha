import { Component, signal, input, output, effect } from '@angular/core';
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

  constructor(private fb: FormBuilder) {
    this.sendForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    // Actualizar email cuando cambie customerEmail
    effect(() => {
      const email = this.customerEmail();
      if (email && this.isOpen()) {
        this.sendForm.patchValue({ email });
      }
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


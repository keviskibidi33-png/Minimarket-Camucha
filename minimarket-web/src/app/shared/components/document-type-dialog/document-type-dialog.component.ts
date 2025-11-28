import { Component, signal, input, output, effect, afterNextRender, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Customer } from '../../../core/services/customers.service';

@Component({
  selector: 'app-document-type-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './document-type-dialog.component.html',
  styleUrl: './document-type-dialog.component.css'
})
export class DocumentTypeDialogComponent {
  selectedCustomer = input<Customer | null>(null);
  documentTypeSelected = output<'Boleta' | 'Factura'>();
  cancelled = output<void>();

  selectedType = signal<'Boleta' | 'Factura'>('Boleta');
  private readonly destroyRef = inject(DestroyRef);
  private effectCleanup?: ReturnType<typeof effect>;

  constructor() {
    // Inicializar tipo de documento basado en el cliente usando effect
    // Usar afterNextRender para asegurar contexto de inyección válido
    afterNextRender(() => {
      this.effectCleanup = effect(() => {
        const customer = this.selectedCustomer();
        // Si hay un cliente con RUC, sugerir Factura; si es DNI o no hay cliente, sugerir Boleta
        if (customer && customer.documentType === 'RUC' && customer.documentNumber.length === 11) {
          this.selectedType.set('Factura');
        } else {
          this.selectedType.set('Boleta');
        }
      }, { allowSignalWrites: true });
    });

    // Limpiar el effect cuando el componente se destruya (fuera del callback de afterNextRender)
    this.destroyRef.onDestroy(() => {
      this.effectCleanup?.destroy();
    });
  }

  canSelectFactura(): boolean {
    const customer = this.selectedCustomer();
    // Factura requiere cliente con RUC válido
    return customer !== null && 
           customer.documentType === 'RUC' && 
           customer.documentNumber.length === 11;
  }

  confirm(): void {
    // Validar que si es Factura, el cliente tenga RUC
    if (this.selectedType() === 'Factura' && !this.canSelectFactura()) {
      return; // No permitir confirmar si no cumple requisitos
    }
    this.documentTypeSelected.emit(this.selectedType());
  }

  cancel(): void {
    this.cancelled.emit();
  }
}


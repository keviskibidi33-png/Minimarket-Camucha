import { Component, Input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

export type OrderStatus = 'pending' | 'confirmed' | 'preparing' | 'shipped' | 'delivered' | 'ready_for_pickup' | 'cancelled';

interface StatusStep {
  key: string;
  label: string;
  icon: string;
  status: OrderStatus;
}

@Component({
  selector: 'app-order-status-tracker',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-status-tracker.component.html',
  styleUrl: './order-status-tracker.component.css'
})
export class OrderStatusTrackerComponent {
  @Input() status: string = 'pending';
  @Input() shippingMethod: string = 'pickup'; // 'delivery' o 'pickup'

  // Convertir el string a OrderStatus
  // Maneja cualquier estado que venga del backend, incluso si el admin lo cambia
  private getOrderStatus(): OrderStatus {
    const validStatuses: OrderStatus[] = ['pending', 'confirmed', 'preparing', 'shipped', 'delivered', 'ready_for_pickup', 'cancelled'];
    const normalizedStatus = this.status?.toLowerCase().trim();
    
    // Si el estado es válido, usarlo
    if (normalizedStatus && validStatuses.includes(normalizedStatus as OrderStatus)) {
      return normalizedStatus as OrderStatus;
    }
    
    // Si no es válido pero parece ser un estado completado, usar 'delivered'
    if (normalizedStatus && (normalizedStatus.includes('delivered') || normalizedStatus.includes('entregado') || normalizedStatus.includes('completado'))) {
      return 'delivered';
    }
    
    // Si no es válido pero parece ser un estado cancelado, usar 'cancelled'
    if (normalizedStatus && (normalizedStatus.includes('cancel') || normalizedStatus.includes('anulado'))) {
      return 'cancelled';
    }
    
    // Por defecto, usar 'pending' si no se reconoce
    return 'pending';
  }

  // Definir los pasos del proceso según el método de envío
  steps = computed<StatusStep[]>(() => {
    const currentStatus = this.getOrderStatus();
    if (this.shippingMethod === 'delivery') {
      return [
        { key: 'confirmed', label: 'CONFIRMADO', icon: 'check_circle', status: 'confirmed' },
        { key: 'preparing', label: 'EN PREPARACIÓN', icon: 'inventory_2', status: 'preparing' },
        { key: 'shipped', label: 'EN CAMINO', icon: 'local_shipping', status: 'shipped' },
        { key: 'delivered', label: 'ENTREGADO', icon: 'done_all', status: 'delivered' }
      ];
    } else {
      return [
        { key: 'confirmed', label: 'CONFIRMADO', icon: 'check_circle', status: 'confirmed' },
        { key: 'preparing', label: 'EN PREPARACIÓN', icon: 'inventory_2', status: 'preparing' },
        { key: 'ready_for_pickup', label: 'LISTO PARA RETIRO', icon: 'store', status: 'ready_for_pickup' }
      ];
    }
  });

  // Determinar el índice del estado actual
  // Si el estado no está en los pasos, intenta encontrar el más cercano
  currentStepIndex = computed(() => {
    const steps = this.steps();
    const currentStatus = this.getOrderStatus();
    let index = steps.findIndex(step => step.status === currentStatus);
    
    // Si no encuentra el estado exacto, intenta mapear estados similares
    if (index === -1) {
      // Mapeo de estados alternativos a estados conocidos
      const statusMapping: { [key: string]: OrderStatus } = {
        'in_progress': 'preparing',
        'processing': 'preparing',
        'on_way': 'shipped',
        'out_for_delivery': 'shipped',
        'completed': 'delivered',
        'finished': 'delivered',
        'ready': 'ready_for_pickup',
        'available': 'ready_for_pickup'
      };
      
      const mappedStatus = statusMapping[currentStatus] || currentStatus;
      index = steps.findIndex(step => step.status === mappedStatus);
    }
    
    // Si aún no encuentra, usar el último paso completado o el primero
    return index >= 0 ? index : 0;
  });

  // Verificar si un paso está completado
  isStepCompleted(stepIndex: number): boolean {
    const currentIndex = this.currentStepIndex();
    return currentIndex > stepIndex;
  }

  // Verificar si un paso está activo
  isStepActive(stepIndex: number): boolean {
    return this.currentStepIndex() === stepIndex;
  }

  // Obtener la clase de color para un paso
  getStepColorClass(stepIndex: number): string {
    if (this.isStepCompleted(stepIndex)) {
      return 'completed';
    } else if (this.isStepActive(stepIndex)) {
      return 'active';
    } else {
      return 'pending';
    }
  }

  // Obtener la clase de color para la línea entre pasos
  getLineColorClass(stepIndex: number): string {
    if (this.isStepCompleted(stepIndex + 1)) {
      return 'completed';
    } else if (this.isStepActive(stepIndex + 1)) {
      return 'active';
    } else {
      return 'pending';
    }
  }
}


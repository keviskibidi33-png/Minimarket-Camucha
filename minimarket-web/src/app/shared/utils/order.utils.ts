/**
 * Utilidades compartidas para el manejo de órdenes
 */

export interface OrderStatusConfig {
  class: string;
  text: string;
}

const ORDER_STATUS_MAP: Record<string, OrderStatusConfig> = {
  'pending': {
    class: 'bg-warning/20 text-warning dark:text-yellow-300',
    text: 'Pendiente'
  },
  'confirmed': {
    class: 'bg-info/20 text-info dark:text-blue-300',
    text: 'Confirmado'
  },
  'preparing': {
    class: 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300',
    text: 'Preparando'
  },
  'shipped': {
    class: 'bg-warning/20 text-warning dark:text-yellow-300',
    text: 'En Camino'
  },
  'delivered': {
    class: 'bg-success/20 text-success dark:text-green-300',
    text: 'Entregado'
  },
  'ready_for_pickup': {
    class: 'bg-success/20 text-success dark:text-green-300',
    text: 'Listo para Retiro'
  },
  'cancelled': {
    class: 'bg-danger/20 text-danger dark:text-red-300',
    text: 'Cancelado'
  }
};

const DEFAULT_STATUS: OrderStatusConfig = {
  class: 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300',
  text: 'Desconocido'
};

/**
 * Obtiene la clase CSS para el estado de una orden
 */
export function getOrderStatusClass(status: string): string {
  const normalizedStatus = status?.toLowerCase().trim() || '';
  return ORDER_STATUS_MAP[normalizedStatus]?.class || DEFAULT_STATUS.class;
}

/**
 * Obtiene el texto legible para el estado de una orden
 */
export function getOrderStatusText(status: string): string {
  const normalizedStatus = status?.toLowerCase().trim() || '';
  return ORDER_STATUS_MAP[normalizedStatus]?.text || status || DEFAULT_STATUS.text;
}

/**
 * Formatea una fecha en formato peruano (dd/mm/yyyy)
 */
export function formatDate(dateString: string | Date): string {
  const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
  return date.toLocaleDateString('es-PE', { 
    day: '2-digit', 
    month: '2-digit', 
    year: 'numeric' 
  });
}

/**
 * Formatea un precio en formato peruano (S/ XX.XX)
 */
export function formatPrice(price: number): string {
  return `S/ ${price.toFixed(2)}`;
}

export function getShippingMethodText(shippingMethod: string): string {
  const method = shippingMethod?.toLowerCase().trim() || '';
  if (method === 'delivery' || method === 'domicilio' || method === 'envio') {
    return 'Delivery';
  }
  if (method === 'pickup' || method === 'recojo' || method === 'retiro') {
    return 'Recojo';
  }
  return shippingMethod || 'N/A';
}

export function getShippingMethodClass(shippingMethod: string): string {
  const method = shippingMethod?.toLowerCase().trim() || '';
  if (method === 'delivery' || method === 'domicilio' || method === 'envio') {
    return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300';
  }
  if (method === 'pickup' || method === 'recojo' || method === 'retiro') {
    return 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300';
  }
  return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300';
}


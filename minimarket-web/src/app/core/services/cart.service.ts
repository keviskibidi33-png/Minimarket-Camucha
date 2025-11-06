import { Injectable, signal } from '@angular/core';

export interface CartItem {
  productId: number;
  productName: string;
  productImageUrl?: string;
  unitPrice: number;
  quantity: number;
  subtotal: number;
}

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartItems = signal<CartItem[]>([]);

  constructor() {
    // Cargar carrito desde localStorage si existe
    this.loadCartFromStorage();
  }

  // Obtener items del carrito
  getCartItems() {
    return this.cartItems.asReadonly();
  }

  // Obtener cantidad total de items
  getTotalItems() {
    return this.cartItems().reduce((sum, item) => sum + item.quantity, 0);
  }

  // Obtener subtotal total
  getSubtotal() {
    return this.cartItems().reduce((sum, item) => sum + item.subtotal, 0);
  }

  /**
   * Convierte un GUID (string) a un número consistente para usar como productId
   * Esta función garantiza que el mismo GUID siempre produce el mismo número
   */
  private guidToProductId(guid: string | number): number {
    // Si ya es un número, devolverlo
    if (typeof guid === 'number') {
      return guid;
    }
    
    // Crear un hash consistente del GUID
    let hash = 0;
    for (let i = 0; i < guid.length; i++) {
      const char = guid.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convertir a entero de 32 bits
    }
    return Math.abs(hash);
  }

  // Agregar producto al carrito
  addToCart(product: {
    id: string | number;
    name: string;
    imageUrl?: string;
    salePrice: number;
    stock: number;
  }, quantity: number = 1) {
    const currentItems = this.cartItems();
    // Convertir GUID a número de forma consistente
    const productId = this.guidToProductId(product.id);
    const existingItem = currentItems.find(item => item.productId === productId);

    if (existingItem) {
      // Si ya existe, aumentar cantidad
      this.updateQuantity(productId, existingItem.quantity + quantity);
    } else {
      // Si no existe, agregar nuevo item
      const newItem: CartItem = {
        productId: productId,
        productName: product.name,
        productImageUrl: product.imageUrl,
        unitPrice: product.salePrice,
        quantity: quantity,
        subtotal: product.salePrice * quantity
      };
      this.cartItems.set([...currentItems, newItem]);
      this.saveCartToStorage();
    }
  }

  // Actualizar cantidad de un item
  updateQuantity(productId: number, quantity: number) {
    if (quantity <= 0) {
      this.removeFromCart(productId);
      return;
    }

    const currentItems = this.cartItems();
    const updatedItems = currentItems.map(item => {
      if (item.productId === productId) {
        return {
          ...item,
          quantity: quantity,
          subtotal: item.unitPrice * quantity
        };
      }
      return item;
    });
    this.cartItems.set(updatedItems);
    this.saveCartToStorage();
  }

  // Eliminar item del carrito
  removeFromCart(productId: number) {
    const currentItems = this.cartItems();
    const updatedItems = currentItems.filter(item => item.productId !== productId);
    this.cartItems.set(updatedItems);
    this.saveCartToStorage();
  }

  // Limpiar carrito
  clearCart(clearCheckoutData: boolean = true) {
    this.cartItems.set([]);
    this.saveCartToStorage();
    // Limpiar también los datos de checkout cuando se limpia el carrito manualmente
    // Pero NO limpiar cuando se está confirmando un pedido (clearCheckoutData = false)
    if (clearCheckoutData) {
      this.clearCheckoutData();
    }
  }

  // Limpiar datos de checkout
  clearCheckoutData() {
    localStorage.removeItem('checkout-shipping');
    localStorage.removeItem('checkout-payment');
    localStorage.removeItem('checkout-total');
    localStorage.removeItem('checkout-items');
    // No limpiar current-order-number para mantener el historial del último pedido
  }

  // Guardar carrito en localStorage
  private saveCartToStorage() {
    try {
      localStorage.setItem('cart', JSON.stringify(this.cartItems()));
    } catch (error) {
      console.error('Error saving cart to storage:', error);
    }
  }

  // Cargar carrito desde localStorage
  private loadCartFromStorage() {
    try {
      const stored = localStorage.getItem('cart');
      if (stored) {
        const items = JSON.parse(stored) as CartItem[];
        // Consolidar duplicados: si hay múltiples items con el mismo productId, combinarlos
        const consolidatedItems = this.consolidateDuplicateItems(items);
        this.cartItems.set(consolidatedItems);
        // Guardar la versión consolidada de vuelta
        if (consolidatedItems.length !== items.length) {
          this.saveCartToStorage();
        }
      }
    } catch (error) {
      console.error('Error loading cart from storage:', error);
    }
  }

  /**
   * Consolida items duplicados sumando sus cantidades
   * Esto corrige cualquier inconsistencia previa en el localStorage
   */
  private consolidateDuplicateItems(items: CartItem[]): CartItem[] {
    const itemMap = new Map<number, CartItem>();
    
    for (const item of items) {
      const existingItem = itemMap.get(item.productId);
      if (existingItem) {
        // Si ya existe, sumar las cantidades
        existingItem.quantity += item.quantity;
        existingItem.subtotal = existingItem.unitPrice * existingItem.quantity;
      } else {
        // Si no existe, agregarlo
        itemMap.set(item.productId, { ...item });
      }
    }
    
    return Array.from(itemMap.values());
  }
}


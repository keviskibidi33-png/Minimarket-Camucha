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

  // Agregar producto al carrito
  addToCart(product: {
    id: number;
    name: string;
    imageUrl?: string;
    salePrice: number;
    stock: number;
  }, quantity: number = 1) {
    const currentItems = this.cartItems();
    const existingItem = currentItems.find(item => item.productId === product.id);

    if (existingItem) {
      // Si ya existe, aumentar cantidad
      this.updateQuantity(product.id, existingItem.quantity + quantity);
    } else {
      // Si no existe, agregar nuevo item
      const newItem: CartItem = {
        productId: product.id,
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
  clearCart() {
    this.cartItems.set([]);
    this.saveCartToStorage();
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
        this.cartItems.set(items);
      }
    } catch (error) {
      console.error('Error loading cart from storage:', error);
    }
  }
}


import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CartService } from '../../../../core/services/cart.service';
import { CheckoutStepperComponent } from '../../../../shared/components/checkout-stepper/checkout-stepper.component';

@Component({
  selector: 'app-confirmation',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule,
    CheckoutStepperComponent
  ],
  templateUrl: './confirmation.component.html',
  styleUrl: './confirmation.component.css'
})
export class ConfirmationComponent implements OnInit {
  cartItems = this.cartService.getCartItems();
  shippingData: any = null;
  paymentData: any = null;
  subtotal = computed(() => this.cartItems().reduce((sum, item) => sum + item.subtotal, 0));
  shippingCost = signal(0);
  total = computed(() => this.subtotal() + this.shippingCost());
  orderNumber = signal('');

  constructor(
    private cartService: CartService,
    private router: Router
  ) {}

  ngOnInit() {
    // Cargar datos de envío y pago
    const shipping = localStorage.getItem('checkout-shipping');
    const payment = localStorage.getItem('checkout-payment');
    
    if (shipping) {
      this.shippingData = JSON.parse(shipping);
      this.shippingCost.set(this.shippingData.shippingCost || 0);
    }
    
    if (payment) {
      this.paymentData = JSON.parse(payment);
    }
    
    // Verificar que el carrito no esté vacío
    if (this.cartItems().length === 0 || !shipping || !payment) {
      this.router.navigate(['/carrito']);
      return;
    }
    
    // Generar número de orden
    this.orderNumber.set('ORD-' + Date.now().toString().slice(-8));
  }

  confirmOrder() {
    // TODO: Enviar orden al backend
    console.log('Confirming order:', {
      items: this.cartItems(),
      shipping: this.shippingData,
      payment: this.paymentData,
      total: this.total()
    });
    
    // Limpiar carrito y datos de checkout
    this.cartService.clearCart();
    localStorage.removeItem('checkout-shipping');
    localStorage.removeItem('checkout-payment');
    
    // Navegar a página de éxito (o mantener aquí con mensaje)
    // this.router.navigate(['/checkout/exito']);
  }

  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }

  getPaymentMethodName(): string {
    if (!this.paymentData) return '';
    switch (this.paymentData.paymentMethod) {
      case 'card': return 'Tarjeta de Crédito/Débito';
      case 'cash': return 'Efectivo al Recibir';
      case 'transfer': return 'Transferencia Bancaria';
      default: return '';
    }
  }
}


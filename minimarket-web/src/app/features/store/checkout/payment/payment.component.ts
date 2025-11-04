import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../../core/services/cart.service';
import { CheckoutStepperComponent } from '../../../../shared/components/checkout-stepper/checkout-stepper.component';
import { StoreHeaderComponent } from '../../../../shared/components/store-header/store-header.component';

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    FormsModule,
    CheckoutStepperComponent,
    StoreHeaderComponent
  ],
  templateUrl: './payment.component.html',
  styleUrl: './payment.component.css'
})
export class PaymentComponent implements OnInit {
  paymentMethod = signal<'card' | 'cash' | 'transfer'>('card');
  
  // Card details
  cardNumber = signal('');
  cardName = signal('');
  expiryDate = signal('');
  cvv = signal('');
  
  cartItems!: ReturnType<typeof CartService.prototype.getCartItems>;
  shippingData: any = null;
  subtotal = computed(() => this.cartItems().reduce((sum: number, item: any) => sum + item.subtotal, 0));
  shippingCost = signal(0);
  total = computed(() => this.subtotal() + this.shippingCost());

  constructor(
    private cartService: CartService,
    private router: Router
  ) {
    // Inicializar cartItems después del constructor
    this.cartItems = this.cartService.getCartItems();
  }

  ngOnInit() {
    // Cargar datos de envío
    const shipping = localStorage.getItem('checkout-shipping');
    if (shipping) {
      this.shippingData = JSON.parse(shipping);
      this.shippingCost.set(this.shippingData.shippingCost || 0);
    }
    
    // Verificar que el carrito no esté vacío
    if (this.cartItems().length === 0) {
      this.router.navigate(['/carrito']);
    }
  }


  continueToConfirmation() {
    // Guardar datos de pago
    const paymentData = {
      paymentMethod: this.paymentMethod(),
      cardNumber: this.cardNumber(),
      cardName: this.cardName(),
      expiryDate: this.expiryDate(),
      cvv: this.cvv()
    };
    localStorage.setItem('checkout-payment', JSON.stringify(paymentData));
    
    // Navegar al siguiente paso
    this.router.navigate(['/checkout/confirmacion']);
  }

  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }
}


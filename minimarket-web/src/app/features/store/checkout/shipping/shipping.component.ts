import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../../core/services/cart.service';
import { StoreHeaderComponent } from '../../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../../shared/components/store-footer/store-footer.component';
import { CheckoutStepperComponent } from '../../../../shared/components/checkout-stepper/checkout-stepper.component';

@Component({
  selector: 'app-shipping',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    FormsModule, 
    StoreHeaderComponent, 
    StoreFooterComponent,
    CheckoutStepperComponent
  ],
  templateUrl: './shipping.component.html',
  styleUrl: './shipping.component.css'
})
export class ShippingComponent implements OnInit {
  // Contact info
  email = signal('');
  
  // Shipping address
  firstName = signal('');
  lastName = signal('');
  address = signal('');
  city = signal('');
  region = signal('');
  
  // Shipping method
  shippingMethod = signal<string>('delivery'); // 'delivery' o 'pickup'
  shippingCost = computed(() => this.shippingMethod() === 'delivery' ? 3.50 : 0);
  
  cartItems = this.cartService.getCartItems();
  subtotal = computed(() => this.cartItems().reduce((sum, item) => sum + item.subtotal, 0));
  total = computed(() => this.subtotal() + this.shippingCost());

  constructor(
    private cartService: CartService,
    private router: Router
  ) {}

  ngOnInit() {
    // Verificar que el carrito no esté vacío
    if (this.cartItems().length === 0) {
      this.router.navigate(['/carrito']);
    }
  }

  continueToPayment() {
    // Guardar datos en localStorage o servicio
    const shippingData = {
      email: this.email(),
      firstName: this.firstName(),
      lastName: this.lastName(),
      address: this.address(),
      city: this.city(),
      region: this.region(),
      shippingMethod: this.shippingMethod() as 'delivery' | 'pickup',
      shippingCost: this.shippingCost()
    };
    localStorage.setItem('checkout-shipping', JSON.stringify(shippingData));
    
    // Navegar al siguiente paso
    this.router.navigate(['/checkout/pago']);
  }

  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }
}


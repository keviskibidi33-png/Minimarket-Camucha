import { Component, OnInit, signal, computed, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../../../core/services/cart.service';
import { ShippingService } from '../../../../core/services/shipping.service';
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
  
  // Coordinates for distance calculation (ejemplo: tienda en Lima Centro)
  private storeLat = -12.0464; // Latitud de la tienda (ejemplo)
  private storeLon = -77.0428; // Longitud de la tienda (ejemplo)
  customerLat = signal<number | null>(null);
  customerLon = signal<number | null>(null);
  
  // Shipping method
  shippingMethod = signal<'delivery' | 'pickup'>('delivery');
  shippingCost = signal(0);
  isCalculatingShipping = signal(false);
  shippingCalculationDetails = signal('');
  
  cartItems!: ReturnType<typeof CartService.prototype.getCartItems>;
  subtotal = computed(() => this.cartItems().reduce((sum: number, item: any) => sum + item.subtotal, 0));
  totalWeight = computed(() => {
    // Calcular peso total estimado (puedes mejorar esto con peso real de productos)
    // Estimación: 0.5kg por producto en promedio
    return this.cartItems().length * 0.5;
  });
  total = computed(() => this.subtotal() + this.shippingCost());

  constructor(
    private cartService: CartService,
    private shippingService: ShippingService,
    private router: Router,
    private ngZone: NgZone
  ) {
    // Inicializar cartItems después del constructor
    this.cartItems = this.cartService.getCartItems();
  }

  ngOnInit() {
    // Verificar que el carrito no esté vacío
    if (this.cartItems().length === 0) {
      this.router.navigate(['/carrito']);
      return;
    }

    // Calcular shipping inicial después de que el componente esté inicializado
    // Usar NgZone.run para asegurar que se ejecute dentro de la zona de Angular
    this.ngZone.run(() => {
      if (this.shippingMethod() === 'delivery') {
        this.calculateShippingCost();
      } else {
        this.shippingCost.set(0);
        this.shippingCalculationDetails.set('Retiro en tienda - Sin costo de envío');
      }
    });
  }

  calculateShippingCost() {
    if (this.shippingMethod() === 'pickup') {
      this.shippingCost.set(0);
      return;
    }

    // Calcular distancia (si tenemos coordenadas del cliente)
    let distance = 5; // Distancia por defecto en km (si no hay coordenadas)
    
    if (this.customerLat() && this.customerLon()) {
      distance = this.shippingService.calculateDistance(
        this.storeLat,
        this.storeLon,
        this.customerLat()!,
        this.customerLon()!
      );
    }

    this.isCalculatingShipping.set(true);
    
    this.shippingService.calculateShipping({
      subtotal: this.subtotal(),
      totalWeight: this.totalWeight(),
      distance: distance,
      zoneName: this.city() || undefined,
      deliveryMethod: this.shippingMethod()
    }).subscribe({
      next: (response) => {
        this.shippingCost.set(response.shippingCost);
        this.shippingCalculationDetails.set(response.calculationDetails);
        this.isCalculatingShipping.set(false);
      },
      error: (error) => {
        console.error('Error calculating shipping:', error);
        // Usar tarifa por defecto en caso de error
        this.shippingCost.set(3.50);
        this.shippingCalculationDetails.set('Tarifa por defecto aplicada');
        this.isCalculatingShipping.set(false);
      }
    });
  }

  onShippingMethodChange(method: 'delivery' | 'pickup') {
    this.shippingMethod.set(method);
    if (method === 'delivery') {
      this.calculateShippingCost();
    } else {
      this.shippingCost.set(0);
      this.shippingCalculationDetails.set('Retiro en tienda - Sin costo de envío');
    }
  }

  onAddressChange() {
    // Cuando el usuario cambia la dirección, recalcular distancia
    // Nota: En producción, usarías un servicio de geocoding para obtener coordenadas
    // Por ahora, usamos una distancia estimada basada en la ciudad
    if (this.shippingMethod() === 'delivery' && this.address()) {
      this.calculateShippingCost();
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


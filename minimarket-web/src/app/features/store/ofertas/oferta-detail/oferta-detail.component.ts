import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { OfertasService, Oferta } from '../../../../core/services/ofertas.service';
import { ProductsService, Product } from '../../../../core/services/products.service';
import { AnalyticsService } from '../../../../core/services/analytics.service';
import { StoreHeaderComponent } from '../../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../../shared/components/store-footer/store-footer.component';
import { CartService } from '../../../../core/services/cart.service';
import { ToastService } from '../../../../shared/services/toast.service';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Component({
  selector: 'app-oferta-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, StoreHeaderComponent, StoreFooterComponent],
  templateUrl: './oferta-detail.component.html',
  styleUrl: './oferta-detail.component.css'
})
export class OfertaDetailComponent implements OnInit {
  oferta = signal<Oferta | null>(null);
  products = signal<Product[]>([]);
  isLoading = signal(true);
  error = signal<string | null>(null);

  constructor(
    private ofertasService: OfertasService,
    private productsService: ProductsService,
    private analyticsService: AnalyticsService,
    private route: ActivatedRoute,
    public router: Router,
    private cartService: CartService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    const ofertaId = this.route.snapshot.paramMap.get('id');
    if (!ofertaId) {
      this.router.navigate(['/tienda/ofertas']);
      return;
    }

    this.loadOferta(ofertaId);
  }

  loadOferta(id: string): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.ofertasService.getById(id).subscribe({
      next: (oferta) => {
        this.oferta.set(oferta);
        this.loadProducts(oferta);
        
        // Trackear vista de página de oferta
        this.analyticsService.trackPageView(`tienda/ofertas/${id}`).subscribe({
          error: (error) => console.error('Error tracking page view:', error)
        });
      },
      error: (error) => {
        console.error('Error loading oferta:', error);
        this.error.set('No se pudo cargar la oferta');
        this.isLoading.set(false);
      }
    });
  }

  loadProducts(oferta: Oferta): void {
    const productIds = oferta.productosIds || [];
    const categoryIds = oferta.categoriasIds || [];

    if (productIds.length === 0 && categoryIds.length === 0) {
      this.products.set([]);
      this.isLoading.set(false);
      return;
    }

    // Cargar productos por IDs
    const productRequests = productIds.map(id => 
      this.productsService.getById(id).pipe(
        catchError(() => of(null))
      )
    );

    // Cargar productos por categorías
    const categoryRequests = categoryIds.map(categoryId =>
      this.productsService.getAll({ categoryId, isActive: true, page: 1, pageSize: 100 }).pipe(
        map(result => result.items),
        catchError(() => of([]))
      )
    );

    // Combinar todas las peticiones
    const allRequests = [...productRequests, ...categoryRequests];

    if (allRequests.length === 0) {
      this.products.set([]);
      this.isLoading.set(false);
      return;
    }

    forkJoin(allRequests).subscribe({
      next: (results: (Product | Product[] | null)[]) => {
        const productMap = new Map<string, Product>();

        // Procesar productos directos (primeros N resultados son Product | null)
        for (let i = 0; i < productIds.length; i++) {
          const result = results[i];
          if (result && typeof result === 'object' && 'id' in result && !Array.isArray(result)) {
            const product = result as Product;
            if (product.isActive && product.stock > 0) {
              productMap.set(product.id, product);
            }
          }
        }

        // Procesar productos de categorías (resto de resultados son Product[])
        for (let i = productIds.length; i < results.length; i++) {
          const result = results[i];
          if (Array.isArray(result)) {
            const categoryProducts = result as Product[];
            categoryProducts.forEach(product => {
              if (product.isActive && product.stock > 0 && !productMap.has(product.id)) {
                productMap.set(product.id, product);
              }
            });
          }
        }

        // Convertir map a array (ya están filtrados por activos y stock)
        const uniqueProducts = Array.from(productMap.values());

        this.products.set(uniqueProducts);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.products.set([]);
        this.isLoading.set(false);
      }
    });
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-PE', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  getDescuentoText(oferta: Oferta): string {
    if (oferta.descuentoTipo === 0) {
      return `${oferta.descuentoValor}% OFF`;
    } else {
      return `S/ ${oferta.descuentoValor} OFF`;
    }
  }

  calculateDiscountedPrice(product: Product, oferta: Oferta): number {
    let discountedPrice = product.salePrice;
    
    if (oferta.descuentoTipo === 0) {
      // Descuento por porcentaje
      const discount = product.salePrice * (oferta.descuentoValor / 100);
      discountedPrice = product.salePrice - discount;
    } else {
      // Descuento por monto fijo
      discountedPrice = Math.max(0, product.salePrice - oferta.descuentoValor);
    }
    
    return discountedPrice;
  }

  isExpired(oferta: Oferta): boolean {
    const now = new Date();
    const fin = new Date(oferta.fechaFin);
    return now > fin;
  }

  isUpcoming(oferta: Oferta): boolean {
    const now = new Date();
    const inicio = new Date(oferta.fechaInicio);
    return now < inicio;
  }

  addToCart(product: Product): void {
    const currentOferta = this.oferta();
    if (!currentOferta || product.stock === 0) {
      return;
    }

    // IMPORTANTE: Solo aplicar descuento cuando se agrega desde esta página de oferta
    // Si el mismo producto se agrega desde otro lugar (productos, home, etc.), 
    // se usará el precio normal (product.salePrice)
    const discountedPrice = this.calculateDiscountedPrice(product, currentOferta);
    
    // Agregar al carrito con el precio con descuento aplicado
    // Este precio solo se aplica cuando se agrega desde la página de detalle de oferta
    this.cartService.addToCart({
      id: product.id,
      name: product.name,
      imageUrl: product.imageUrl,
      salePrice: discountedPrice, // Precio con descuento - solo desde esta página
      stock: product.stock
    }, 1);

    const totalItems = this.cartService.getTotalItems();
    const message = totalItems === 1 
      ? `"${product.name}" agregado al carrito`
      : `"${product.name}" agregado al carrito (Total: ${totalItems} productos)`;
    
    this.toastService.success(message, 3000);
  }
}


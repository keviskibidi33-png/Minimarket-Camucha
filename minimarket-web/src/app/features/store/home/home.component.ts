import { Component, OnInit, signal, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { ProductsService, Product } from '../../../core/services/products.service';
import { CategoriesService, CategoryDto } from '../../../core/services/categories.service';
import { BrandSettingsService, BrandSettings } from '../../../core/services/brand-settings.service';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { OfertasService, Oferta } from '../../../core/services/ofertas.service';
import { StoreHeaderComponent } from '../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../shared/components/store-footer/store-footer.component';
import { ProductCardComponent } from '../../../shared/components/product-card/product-card.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    StoreHeaderComponent, 
    StoreFooterComponent, 
    ProductCardComponent
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('categoriesScrollContainer', { static: false }) categoriesScrollContainer!: ElementRef<HTMLDivElement>;
  @ViewChild('categoriesWrapper', { static: false }) categoriesWrapper!: ElementRef<HTMLDivElement>;
  @ViewChild('productsScrollContainer', { static: false }) productsScrollContainer!: ElementRef<HTMLDivElement>;
  @ViewChild('productsWrapper', { static: false }) productsWrapper!: ElementRef<HTMLDivElement>;
  
  featuredProducts = signal<Product[]>([]);
  duplicatedFeaturedProducts = signal<Product[]>([]); // Para el scroll infinito de productos
  categories = signal<CategoryDto[]>([]);
  popularCategories = signal<CategoryDto[]>([]);
  duplicatedCategories = signal<CategoryDto[]>([]); // Para el scroll infinito de categorías
  activeOfertas = signal<Oferta[]>([]);
  isLoading = signal(true);
  brandSettings = signal<BrandSettings | null>(null);
  
  // ==================== Propiedades del Carrusel Infinito de Categorías ====================
  private categoriesScrollAnimationId: number | null = null;
  private isCategoriesAutoScrolling = false;
  private categoriesScrollDirection: 'left' | 'right' = 'right';
  private categoriesSingleSetWidth = 0;
  
  // ==================== Propiedades del Carrusel Infinito de Productos ====================
  private productsScrollAnimationId: number | null = null;
  private isProductsAutoScrolling = false;
  private productsScrollDirection: 'left' | 'right' = 'right';
  private productsSingleSetWidth = 0;
  
  // ==================== Propiedades Compartidas ====================
  private readonly SCROLL_SPEED = 0.5; // Píxeles por frame (60fps)
  private readonly INITIALIZATION_DELAY = 500; // ms para esperar renderizado
  private readonly RETRY_DELAY = 300; // ms para reintentos
  private readonly MOBILE_BREAKPOINT = 768; // md breakpoint de Tailwind (768px)
  private resizeListener?: () => void;

  constructor(
    private productsService: ProductsService,
    private categoriesService: CategoriesService,
    private brandSettingsService: BrandSettingsService,
    private analyticsService: AnalyticsService,
    private ofertasService: OfertasService,
    private titleService: Title
  ) {}

  ngOnInit() {
    // Trackear vista de página
    this.analyticsService.trackPageView('home').subscribe({
      error: (error) => console.error('Error tracking page view:', error)
    });
    
    this.loadBrandSettings();
    this.loadFeaturedProducts();
    this.loadCategories();
    this.loadOfertas();
  }

  ngAfterViewInit() {
    // Inicializar los carruseles infinitos después del renderizado completo
    setTimeout(() => {
      this.initializeCategoriesCarousel();
      this.initializeProductsCarousel();
    }, this.INITIALIZATION_DELAY);
    
    // Agregar listener para cambios de tamaño de ventana
    this.resizeListener = () => {
      this.handleResize();
    };
    window.addEventListener('resize', this.resizeListener);
  }

  ngOnDestroy() {
    // Limpiar recursos al destruir el componente
    this.pauseCategoriesAutoScroll();
    this.pauseProductsAutoScroll();
    
    // Remover listener de resize
    if (this.resizeListener) {
      window.removeEventListener('resize', this.resizeListener);
    }
  }
  
  /**
   * Detecta si el dispositivo es móvil (ancho < 768px)
   */
  private isMobile(): boolean {
    return window.innerWidth < this.MOBILE_BREAKPOINT;
  }
  
  /**
   * Maneja cambios en el tamaño de la ventana
   * Ajusta el comportamiento del scroll según el tamaño de pantalla
   */
  private handleResize(): void {
    if (this.isMobile()) {
      // En móvil: activar scroll automático si no está activo
      if (!this.isCategoriesAutoScrolling && this.categoriesSingleSetWidth > 0) {
        this.startCategoriesAutoScroll();
      }
      if (!this.isProductsAutoScrolling && this.productsSingleSetWidth > 0) {
        this.startProductsAutoScroll();
      }
    } else {
      // En escritorio: desactivar scroll automático
      if (this.isCategoriesAutoScrolling) {
        this.pauseCategoriesAutoScroll();
      }
      if (this.isProductsAutoScrolling) {
        this.pauseProductsAutoScroll();
      }
    }
  }

  loadBrandSettings() {
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        this.brandSettings.set(settings);
        // Actualizar título de la página
        if (settings?.storeName) {
          this.titleService.setTitle(settings.storeName);
        } else {
          this.titleService.setTitle('Minimarket Camucha');
        }
      },
      error: (error) => {
        console.error('Error loading brand settings:', error);
        this.titleService.setTitle('Minimarket Camucha');
      }
    });
  }


  loadFeaturedProducts() {
    this.isLoading.set(true);
    this.productsService.getAll({ 
      isActive: true, 
      pageSize: 100 // Cargar más para filtrar los destacados
    }).subscribe({
      next: (result) => {
        const products = result.items || (Array.isArray(result) ? result : []);
        // Filtrar productos destacados (con home: true en paginas)
        const featured = products
          .filter(p => p.paginas && p.paginas['home'] === true)
          .sort((a, b) => {
            const ordenA = a.paginas?.['home_orden'] || 999;
            const ordenB = b.paginas?.['home_orden'] || 999;
            return ordenA - ordenB;
          })
          .slice(0, 10); // Limitar a 10 productos destacados
        this.featuredProducts.set(featured);
        
        // Preparar productos para el carrusel infinito
        if (featured.length > 0) {
          // Duplicar 3 veces para crear efecto infinito sin interrupciones
          this.duplicatedFeaturedProducts.set([...featured, ...featured, ...featured]);
          
          // Inicializar carrusel después de que los productos estén renderizados
          setTimeout(() => {
            this.initializeProductsCarousel();
          }, 100);
        }
        
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading featured products:', error);
        this.isLoading.set(false);
      }
    });
  }

  loadCategories() {
    this.categoriesService.getAllWithoutPagination().subscribe({
      next: (allCategories) => {
        // Filtrar solo categorías activas
        const activeCategories = allCategories.filter((cat: any) => cat.isActive);
        this.categories.set(activeCategories);
        
        // Filtrar categorías populares (con orden definido y > 0)
        const popular = activeCategories
          .filter((cat: any) => cat.orden !== undefined && cat.orden > 0)
          .sort((a: any, b: any) => (a.orden || 999) - (b.orden || 999));
        
        // Si no hay categorías con orden, tomar las primeras activas (sin límite)
        if (popular.length === 0) {
          this.popularCategories.set(activeCategories);
        } else {
          this.popularCategories.set(popular);
        }
        
        // Preparar categorías para el carrusel infinito
        const popularCats = this.popularCategories();
        if (popularCats.length > 0) {
          // Duplicar 3 veces para crear efecto infinito sin interrupciones
          // [A, B, C] -> [A, B, C, A, B, C, A, B, C]
          this.duplicatedCategories.set([...popularCats, ...popularCats, ...popularCats]);
          
          // Inicializar carrusel después de que las categorías estén renderizadas
          setTimeout(() => this.initializeCategoriesCarousel(), 400);
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  loadOfertas() {
    this.ofertasService.getAll(true).subscribe({
      next: (ofertas) => {
        // Filtrar solo las ofertas activas y que estén dentro del rango de fechas
        const now = new Date();
        const activeOfertas = ofertas.filter(oferta => {
          const inicio = new Date(oferta.fechaInicio);
          const fin = new Date(oferta.fechaFin);
          return oferta.activa && now >= inicio && now <= fin;
        });
        // Ordenar por orden y luego por fecha de inicio
        activeOfertas.sort((a, b) => {
          if (a.orden !== b.orden) {
            return a.orden - b.orden;
          }
          return new Date(a.fechaInicio).getTime() - new Date(b.fechaInicio).getTime();
        });
        // Limitar a las primeras 3 ofertas para el carrusel
        this.activeOfertas.set(activeOfertas.slice(0, 3));
      },
      error: (error) => {
        console.error('Error loading ofertas:', error);
      }
    });
  }

  getCategoryIcon(categoryName: string): string {
    const name = categoryName.toLowerCase();
    if (name.includes('bebida') || name.includes('drink') || name.includes('licor')) {
      return 'local_bar';
    } else if (name.includes('snack') || name.includes('dulce')) {
      return 'icecream';
    } else if (name.includes('grocer') || name.includes('abarrote') || name.includes('comida')) {
      return 'local_mall';
    } else if (name.includes('limpieza') || name.includes('household') || name.includes('hogar')) {
      return 'cleaning_services';
    }
    return 'shopping_bag';
  }

  // ==================== Lógica del Carrusel Infinito ====================
  
  /**
   * Inicializa el carrusel infinito bidireccional
   * Verifica que todos los elementos DOM estén listos y calcula las dimensiones necesarias
   */
  private initializeCategoriesCarousel(): void {
    if (!this.validateCarouselElements()) {
      setTimeout(() => this.initializeCategoriesCarousel(), this.RETRY_DELAY);
      return;
    }
    
    const container = this.categoriesScrollContainer.nativeElement;
    const wrapper = this.categoriesWrapper.nativeElement;
    const popularCats = this.popularCategories();
    
    if (popularCats.length === 0) {
      return; // No hay categorías para mostrar
    }
    
    // Esperar renderizado completo del DOM
    setTimeout(() => {
      const dimensions = this.calculateCarouselDimensions(container, wrapper);
      
      if (!dimensions.isValid) {
        setTimeout(() => this.initializeCategoriesCarousel(), this.RETRY_DELAY);
        return;
      }
      
      if (dimensions.needsScroll) {
        this.categoriesSingleSetWidth = dimensions.singleSetWidth;
        // Iniciar en el rango seguro (segunda copia) para permitir scroll en ambas direcciones
        // sin saltos inmediatos
        container.scrollLeft = this.categoriesSingleSetWidth;
        
        // Solo iniciar scroll automático en móvil
        if (this.isMobile()) {
          this.startCategoriesAutoScroll();
        }
      }
    }, 100);
  }

  /**
   * Valida que los elementos del carrusel estén disponibles
   */
  private validateCarouselElements(): boolean {
    return !!(
      this.categoriesScrollContainer?.nativeElement &&
      this.categoriesWrapper?.nativeElement
    );
  }

  /**
   * Calcula las dimensiones necesarias para el carrusel infinito
   */
  private calculateCarouselDimensions(
    container: HTMLElement,
    wrapper: HTMLElement
  ): { isValid: boolean; needsScroll: boolean; singleSetWidth: number } {
    const wrapperWidth = wrapper.scrollWidth;
    const containerWidth = container.clientWidth;
    const singleSetWidth = wrapperWidth / 3; // 3 copias duplicadas
    
    return {
      isValid: singleSetWidth > 0 && containerWidth > 0,
      needsScroll: wrapperWidth > containerWidth,
      singleSetWidth
    };
  }

  /**
   * Inicia el scroll automático infinito de categorías en la dirección configurada
   * Utiliza requestAnimationFrame para animación suave a 60fps
   * NO reinicia si ya está activo - solo inicia si está detenido
   */
  private startCategoriesAutoScroll(): void {
    // Si ya está scrolleando, no hacer nada (preservar estado actual)
    if (this.isCategoriesAutoScrolling) return;
    
    if (this.categoriesSingleSetWidth <= 0) return;
    
    const container = this.categoriesScrollContainer?.nativeElement;
    if (!container) return;
    
    // Preservar la posición actual antes de iniciar
    const preservedPosition = container.scrollLeft;
    
    this.isCategoriesAutoScrolling = true;
    
    const animate = (): void => {
      if (!this.isCategoriesAutoScrolling || !container) {
        this.categoriesScrollAnimationId = null;
        return;
      }
      
      const currentScroll = container.scrollLeft;
      const newScroll = this.calculateCategoriesNextScrollPosition(currentScroll);
      
      // Aplicar el nuevo scroll con lógica de bucle infinito
      container.scrollLeft = this.applyCategoriesInfiniteLoop(newScroll);
      
      this.categoriesScrollAnimationId = requestAnimationFrame(animate);
    };
    
    // Asegurar que la posición se mantenga al iniciar
    if (container.scrollLeft !== preservedPosition) {
      container.scrollLeft = preservedPosition;
    }
    
    this.categoriesScrollAnimationId = requestAnimationFrame(animate);
  }

  /**
   * Calcula la siguiente posición de scroll de categorías según la dirección
   */
  private calculateCategoriesNextScrollPosition(currentScroll: number): number {
    return this.categoriesScrollDirection === 'right'
      ? currentScroll + this.SCROLL_SPEED
      : currentScroll - this.SCROLL_SPEED;
  }

  /**
   * Aplica la lógica de bucle infinito al scroll de categorías
   * Cuando se alcanza un límite, salta al punto equivalente en otra copia
   * Mantiene la continuidad visual sin saltos bruscos
   */
  private applyCategoriesInfiniteLoop(newScroll: number): number {
    const totalWidth = this.categoriesSingleSetWidth * 3; // 3 copias duplicadas
    
    if (this.categoriesScrollDirection === 'right') {
      // Scroll hacia la derecha: al llegar al final de la tercera copia,
      // saltar al inicio de la segunda copia (mismo contenido visual)
      if (newScroll >= totalWidth) {
        // Calcular cuánto se pasó del límite
        const overflow = newScroll - totalWidth;
        // Saltar al inicio de la segunda copia más el overflow
        return this.categoriesSingleSetWidth + overflow;
      }
      // Si el scroll es negativo (puede pasar al cambiar de dirección),
      // saltar al final de la segunda copia manteniendo la posición relativa
      if (newScroll < 0) {
        // Calcular cuánto se pasó del límite (negativo)
        const underflow = newScroll;
        // Saltar al final de la segunda copia más el underflow
        // Esto mantiene la posición relativa correcta
        return this.categoriesSingleSetWidth * 2 + underflow;
      }
      return newScroll;
    } else {
      // Scroll hacia la izquierda: al llegar antes del inicio (0),
      // saltar al final de la segunda copia
      if (newScroll < 0) {
        // Calcular cuánto se pasó del límite (negativo)
        const underflow = newScroll;
        // Saltar al final de la segunda copia más el underflow
        return this.categoriesSingleSetWidth * 2 + underflow;
      }
      // Si el scroll excede el límite (puede pasar al cambiar de dirección),
      // saltar al inicio de la primera copia manteniendo la posición relativa
      if (newScroll >= totalWidth) {
        // Calcular la posición relativa dentro de la copia siguiente
        const overflow = newScroll - totalWidth;
        // Saltar al inicio de la primera copia más el overflow
        return overflow;
      }
      return newScroll;
    }
  }

  /**
   * Cambia la dirección del scroll infinito de categorías sin resetear la posición visual
   * Normaliza la posición al rango seguro (segunda copia) manteniendo la posición relativa
   */
  private changeCategoriesScrollDirection(direction: 'left' | 'right'): void {
    // Solo cambiar la dirección si es diferente a la actual
    if (this.categoriesScrollDirection !== direction) {
      const container = this.categoriesScrollContainer?.nativeElement;
      
      if (container && this.categoriesSingleSetWidth > 0) {
        // Obtener la posición actual
        const currentPosition = container.scrollLeft;
        
        // Calcular la posición relativa dentro de una copia (0 a singleSetWidth)
        // Esto preserva la posición visual exacta
        const relativePosition = ((currentPosition % this.categoriesSingleSetWidth) + this.categoriesSingleSetWidth) % this.categoriesSingleSetWidth;
        
        // Cambiar la dirección
        this.categoriesScrollDirection = direction;
        
        // Normalizar la posición al rango seguro (segunda copia)
        // Esto evita saltos inmediatos cuando el scroll está cerca de los límites
        const safePosition = this.categoriesSingleSetWidth + relativePosition;
        
        // Solo ajustar si la posición actual no está en el rango seguro
        // (entre singleSetWidth y singleSetWidth * 2)
        const safeRangeStart = this.categoriesSingleSetWidth;
        const safeRangeEnd = this.categoriesSingleSetWidth * 2;
        
        if (currentPosition < safeRangeStart || currentPosition >= safeRangeEnd) {
          // Ajustar a la posición normalizada en el rango seguro
          // Esto mantiene la posición visual exacta pero evita saltos
          container.scrollLeft = safePosition;
        }
        // Si ya está en el rango seguro, mantener la posición exacta
      } else {
        // Si no hay contenedor o singleSetWidth no está calculado, solo cambiar dirección
        this.categoriesScrollDirection = direction;
      }
    }
    
    // Si el scroll automático no está activo, iniciarlo
    if (!this.isCategoriesAutoScrolling) {
      this.startCategoriesAutoScroll();
    }
  }

  /**
   * Pausa el scroll automático de categorías (útil para hover o interacción manual)
   */
  private pauseCategoriesAutoScroll(): void {
    this.isCategoriesAutoScrolling = false;
    if (this.categoriesScrollAnimationId !== null) {
      cancelAnimationFrame(this.categoriesScrollAnimationId);
      this.categoriesScrollAnimationId = null;
    }
  }

  /**
   * Reanuda el scroll automático de categorías después de una pausa
   * NO reinicializa el carrusel para preservar la posición actual
   */
  private resumeCategoriesAutoScroll(): void {
    if (!this.isCategoriesAutoScrolling && this.popularCategories().length > 0) {
      // Verificar que el carrusel ya está inicializado
      if (this.categoriesSingleSetWidth > 0 && this.categoriesScrollContainer?.nativeElement) {
        // Simplemente reanudar el scroll desde donde estaba
        setTimeout(() => {
          if (!this.isCategoriesAutoScrolling) {
            this.startCategoriesAutoScroll();
          }
        }, 150);
      } else {
        // Si no está inicializado, inicializarlo (solo la primera vez)
        setTimeout(() => {
          if (!this.isCategoriesAutoScrolling) {
            this.initializeCategoriesCarousel();
          }
        }, 150);
      }
    }
  }

  // ==================== Event Handlers ====================
  
  /**
   * Maneja el evento de scroll manual del usuario
   * Pausa el scroll automático temporalmente solo en móvil
   */
  onCategoriesScroll(): void {
    // Solo pausar scroll automático en móvil cuando el usuario hace scroll manual
    if (this.isMobile() && this.isCategoriesAutoScrolling) {
      this.pauseCategoriesAutoScroll();
      
      // Reanudar después de un periodo de inactividad (solo en móvil)
      setTimeout(() => {
        if (!this.isCategoriesAutoScrolling && this.isMobile()) {
          this.resumeCategoriesAutoScroll();
        }
      }, 1500);
    }
  }

  /**
   * Maneja el evento de hover sobre el carrusel
   * Pausa el scroll automático para permitir interacción
   */
  onCarouselMouseEnter(): void {
    // Solo pausar scroll automático en móvil
    if (this.isMobile()) {
      this.pauseCategoriesAutoScroll();
    }
  }

  /**
   * Maneja el evento de salida del hover
   * Reanuda el scroll automático solo en móvil
   */
  onCarouselMouseLeave(): void {
    // Solo reanudar scroll automático en móvil
    if (this.isMobile()) {
      this.resumeCategoriesAutoScroll();
    }
  }

  // ==================== Controles de Navegación ====================
  
  /**
   * Controla el desplazamiento hacia la izquierda
   * En escritorio: mueve el scroll manualmente
   * En móvil: cambia la dirección del scroll automático
   */
  scrollCategoriesLeft(): void {
    if (!this.categoriesScrollContainer?.nativeElement) return;
    
    const container = this.categoriesScrollContainer.nativeElement;
    
    if (this.isMobile()) {
      // En móvil: cambiar dirección del scroll automático
      this.changeCategoriesScrollDirection('left');
    } else {
      // En escritorio: mover manualmente sin scroll automático
      const currentPosition = container.scrollLeft;
      const scrollAmount = this.categoriesSingleSetWidth > 0 ? this.categoriesSingleSetWidth / 2 : 200; // Mover la mitad de una copia o 200px
      const newPosition = currentPosition - scrollAmount;
      
      // Aplicar bucle infinito si es necesario
      if (newPosition < 0) {
        const totalWidth = this.categoriesSingleSetWidth * 3;
        container.scrollLeft = this.categoriesSingleSetWidth * 2 + newPosition;
      } else {
        container.scrollLeft = newPosition;
      }
    }
  }

  /**
   * Controla el desplazamiento hacia la derecha
   * En escritorio: mueve el scroll manualmente
   * En móvil: cambia la dirección del scroll automático
   */
  scrollCategoriesRight(): void {
    if (!this.categoriesScrollContainer?.nativeElement) return;
    
    const container = this.categoriesScrollContainer.nativeElement;
    
    if (this.isMobile()) {
      // En móvil: cambiar dirección del scroll automático
      this.changeCategoriesScrollDirection('right');
    } else {
      // En escritorio: mover manualmente sin scroll automático
      const currentPosition = container.scrollLeft;
      const scrollAmount = this.categoriesSingleSetWidth > 0 ? this.categoriesSingleSetWidth / 2 : 200; // Mover la mitad de una copia o 200px
      const newPosition = currentPosition + scrollAmount;
      
      // Aplicar bucle infinito si es necesario
      const totalWidth = this.categoriesSingleSetWidth * 3;
      if (newPosition >= totalWidth) {
        container.scrollLeft = this.categoriesSingleSetWidth + (newPosition - totalWidth);
      } else {
        container.scrollLeft = newPosition;
      }
    }
  }

  // ==================== Lógica del Carrusel Infinito de Productos ====================
  
  /**
   * Inicializa el carrusel infinito de productos destacados
   */
  private initializeProductsCarousel(): void {
    if (!this.validateProductsCarouselElements()) {
      setTimeout(() => this.initializeProductsCarousel(), this.RETRY_DELAY);
      return;
    }
    
    const container = this.productsScrollContainer.nativeElement;
    const wrapper = this.productsWrapper.nativeElement;
    const featured = this.featuredProducts();
    
    if (featured.length === 0) {
      return; // No hay productos para mostrar
    }
    
    // Esperar renderizado completo del DOM
    setTimeout(() => {
      const dimensions = this.calculateCarouselDimensions(container, wrapper);
      
      if (!dimensions.isValid) {
        setTimeout(() => this.initializeProductsCarousel(), this.RETRY_DELAY);
        return;
      }
      
      if (dimensions.needsScroll) {
        this.productsSingleSetWidth = dimensions.singleSetWidth;
        // Iniciar en el rango seguro (segunda copia)
        container.scrollLeft = this.productsSingleSetWidth;
        
        // Solo iniciar scroll automático en móvil
        if (this.isMobile()) {
          this.startProductsAutoScroll();
        }
      }
    }, 100);
  }

  /**
   * Valida que los elementos del carrusel de productos estén disponibles
   */
  private validateProductsCarouselElements(): boolean {
    return !!(
      this.productsScrollContainer?.nativeElement &&
      this.productsWrapper?.nativeElement
    );
  }

  /**
   * Inicia el scroll automático infinito de productos
   */
  private startProductsAutoScroll(): void {
    if (this.isProductsAutoScrolling) return;
    if (this.productsSingleSetWidth <= 0) return;
    
    const container = this.productsScrollContainer?.nativeElement;
    if (!container) return;
    
    const preservedPosition = container.scrollLeft;
    this.isProductsAutoScrolling = true;
    
    const animate = (): void => {
      if (!this.isProductsAutoScrolling || !container) {
        this.productsScrollAnimationId = null;
        return;
      }
      
      const currentScroll = container.scrollLeft;
      const newScroll = this.calculateProductsNextScrollPosition(currentScroll);
      container.scrollLeft = this.applyProductsInfiniteLoop(newScroll);
      
      this.productsScrollAnimationId = requestAnimationFrame(animate);
    };
    
    if (container.scrollLeft !== preservedPosition) {
      container.scrollLeft = preservedPosition;
    }
    
    this.productsScrollAnimationId = requestAnimationFrame(animate);
  }

  /**
   * Calcula la siguiente posición de scroll de productos según la dirección
   */
  private calculateProductsNextScrollPosition(currentScroll: number): number {
    return this.productsScrollDirection === 'right'
      ? currentScroll + this.SCROLL_SPEED
      : currentScroll - this.SCROLL_SPEED;
  }

  /**
   * Aplica la lógica de bucle infinito al scroll de productos
   */
  private applyProductsInfiniteLoop(newScroll: number): number {
    const totalWidth = this.productsSingleSetWidth * 3;
    
    if (this.productsScrollDirection === 'right') {
      if (newScroll >= totalWidth) {
        const overflow = newScroll - totalWidth;
        return this.productsSingleSetWidth + overflow;
      }
      if (newScroll < 0) {
        const underflow = newScroll;
        return this.productsSingleSetWidth * 2 + underflow;
      }
      return newScroll;
    } else {
      if (newScroll < 0) {
        const underflow = newScroll;
        return this.productsSingleSetWidth * 2 + underflow;
      }
      if (newScroll >= totalWidth) {
        const overflow = newScroll - totalWidth;
        return overflow;
      }
      return newScroll;
    }
  }

  /**
   * Cambia la dirección del scroll infinito de productos
   */
  private changeProductsScrollDirection(direction: 'left' | 'right'): void {
    if (this.productsScrollDirection !== direction) {
      const container = this.productsScrollContainer?.nativeElement;
      
      if (container && this.productsSingleSetWidth > 0) {
        const currentPosition = container.scrollLeft;
        const relativePosition = ((currentPosition % this.productsSingleSetWidth) + this.productsSingleSetWidth) % this.productsSingleSetWidth;
        
        this.productsScrollDirection = direction;
        
        const safePosition = this.productsSingleSetWidth + relativePosition;
        const safeRangeStart = this.productsSingleSetWidth;
        const safeRangeEnd = this.productsSingleSetWidth * 2;
        
        if (currentPosition < safeRangeStart || currentPosition >= safeRangeEnd) {
          container.scrollLeft = safePosition;
        }
      } else {
        this.productsScrollDirection = direction;
      }
    }
    
    if (!this.isProductsAutoScrolling) {
      this.startProductsAutoScroll();
    }
  }

  /**
   * Pausa el scroll automático de productos
   */
  private pauseProductsAutoScroll(): void {
    this.isProductsAutoScrolling = false;
    if (this.productsScrollAnimationId !== null) {
      cancelAnimationFrame(this.productsScrollAnimationId);
      this.productsScrollAnimationId = null;
    }
  }

  /**
   * Reanuda el scroll automático de productos
   */
  private resumeProductsAutoScroll(): void {
    if (!this.isProductsAutoScrolling && this.featuredProducts().length > 0) {
      if (this.productsSingleSetWidth > 0 && this.productsScrollContainer?.nativeElement) {
        setTimeout(() => {
          if (!this.isProductsAutoScrolling) {
            this.startProductsAutoScroll();
          }
        }, 150);
      } else {
        setTimeout(() => {
          if (!this.isProductsAutoScrolling) {
            this.initializeProductsCarousel();
          }
        }, 150);
      }
    }
  }

  /**
   * Maneja el evento de scroll manual de productos
   */
  onProductsScroll(): void {
    if (this.isMobile() && this.isProductsAutoScrolling) {
      this.pauseProductsAutoScroll();
      
      setTimeout(() => {
        if (!this.isProductsAutoScrolling && this.isMobile()) {
          this.resumeProductsAutoScroll();
        }
      }, 1500);
    }
  }

  /**
   * Maneja el evento de entrada del hover en productos
   */
  onProductsCarouselMouseEnter(): void {
    if (this.isMobile()) {
      this.pauseProductsAutoScroll();
    }
  }

  /**
   * Maneja el evento de salida del hover en productos
   */
  onProductsCarouselMouseLeave(): void {
    if (this.isMobile()) {
      this.resumeProductsAutoScroll();
    }
  }

  /**
   * Controla el desplazamiento de productos hacia la izquierda
   */
  scrollProductsLeft(): void {
    if (!this.productsScrollContainer?.nativeElement) return;
    
    const container = this.productsScrollContainer.nativeElement;
    
    if (this.isMobile()) {
      this.changeProductsScrollDirection('left');
    } else {
      const currentPosition = container.scrollLeft;
      const scrollAmount = this.productsSingleSetWidth > 0 ? this.productsSingleSetWidth / 2 : 200;
      const newPosition = currentPosition - scrollAmount;
      
      if (newPosition < 0) {
        const totalWidth = this.productsSingleSetWidth * 3;
        container.scrollLeft = this.productsSingleSetWidth * 2 + newPosition;
      } else {
        container.scrollLeft = newPosition;
      }
    }
  }

  /**
   * Controla el desplazamiento de productos hacia la derecha
   */
  scrollProductsRight(): void {
    if (!this.productsScrollContainer?.nativeElement) return;
    
    const container = this.productsScrollContainer.nativeElement;
    
    if (this.isMobile()) {
      this.changeProductsScrollDirection('right');
    } else {
      const currentPosition = container.scrollLeft;
      const scrollAmount = this.productsSingleSetWidth > 0 ? this.productsSingleSetWidth / 2 : 200;
      const newPosition = currentPosition + scrollAmount;
      
      const totalWidth = this.productsSingleSetWidth * 3;
      if (newPosition >= totalWidth) {
        container.scrollLeft = this.productsSingleSetWidth + (newPosition - totalWidth);
      } else {
        container.scrollLeft = newPosition;
      }
    }
  }
}


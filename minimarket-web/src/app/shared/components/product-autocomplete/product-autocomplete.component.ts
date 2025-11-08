import { Component, Input, Output, EventEmitter, signal, OnInit, OnDestroy, ElementRef, ViewChild, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductsService, Product } from '../../../core/services/products.service';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-product-autocomplete',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './product-autocomplete.component.html',
  styleUrl: './product-autocomplete.component.css'
})
export class ProductAutocompleteComponent implements OnInit, OnDestroy {
  @Input() placeholder: string = 'Buscar productos...';
  @Output() search = new EventEmitter<string>();
  @Output() productSelected = new EventEmitter<Product>();

  @ViewChild('inputRef', { static: false }) inputRef!: ElementRef<HTMLInputElement>;
  @ViewChild('dropdownRef', { static: false }) dropdownRef!: ElementRef<HTMLDivElement>;

  searchTerm = signal('');
  suggestions = signal<Product[]>([]);
  isLoading = signal(false);
  showDropdown = signal(false);
  selectedIndex = signal(-1);

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();
  private maxResults = 8; // Máximo de resultados a mostrar

  constructor(private productsService: ProductsService) {
    
    // Debounce para evitar demasiadas búsquedas
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(term => {
      if (term && term.trim().length >= 2) {
        this.searchProducts(term.trim());
      } else {
        this.suggestions.set([]);
        this.showDropdown.set(false);
        this.isLoading.set(false);
      }
    });
  }

  ngOnInit(): void {
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent): void {
    // Verificar si el click fue fuera del componente
    if (this.dropdownRef?.nativeElement && this.inputRef?.nativeElement) {
      const target = event.target as Node;
      const clickedInside = this.dropdownRef.nativeElement.contains(target) ||
                           this.inputRef.nativeElement.contains(target);
      if (!clickedInside) {
        // Delay para permitir clicks en el dropdown
        setTimeout(() => {
          this.showDropdown.set(false);
        }, 150);
      }
    }
  }

  onInputChange(value: string): void {
    this.searchTerm.set(value);
    const trimmed = value?.trim() || '';
    
    if (trimmed.length >= 2) {
      this.showDropdown.set(true);
      this.isLoading.set(true);
      this.searchSubject.next(trimmed);
    } else {
      this.suggestions.set([]);
      this.showDropdown.set(false);
      this.isLoading.set(false);
    }
  }

  onInputFocus(): void {
    if (this.suggestions().length > 0) {
      this.showDropdown.set(true);
    }
  }

  onInputBlur(): void {
    // No ocultar inmediatamente, dejar que onClickOutside lo maneje
    // Esto permite que los clicks en el dropdown funcionen
  }

  onDropdownMouseLeave(): void {
    // Ocultar el dropdown cuando el mouse sale, pero solo si el input no tiene foco
    setTimeout(() => {
      if (this.inputRef?.nativeElement && document.activeElement !== this.inputRef.nativeElement) {
        this.showDropdown.set(false);
      }
    }, 200);
  }

  onDropdownMouseEnter(): void {
    // Mantener el dropdown visible cuando el mouse entra
    this.showDropdown.set(true);
  }

  onKeyDown(event: KeyboardEvent): void {
    const suggestions = this.suggestions();
    
    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.selectedIndex.update(idx => 
        idx < suggestions.length - 1 ? idx + 1 : idx
      );
      this.scrollToSelected();
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.selectedIndex.update(idx => idx > 0 ? idx - 1 : -1);
      this.scrollToSelected();
    } else if (event.key === 'Enter') {
      event.preventDefault();
      if (this.selectedIndex() >= 0 && this.selectedIndex() < suggestions.length) {
        this.selectProduct(suggestions[this.selectedIndex()]);
      } else if (this.searchTerm().trim().length > 0) {
        this.onSearch();
      }
    } else if (event.key === 'Escape') {
      this.showDropdown.set(false);
      this.selectedIndex.set(-1);
    }
  }

  private scrollToSelected(): void {
    setTimeout(() => {
      const selectedElement = document.querySelector('.suggestion-item.selected');
      if (selectedElement) {
        selectedElement.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
      }
    }, 0);
  }

  private searchProducts(term: string): void {
    this.isLoading.set(true);
    this.showDropdown.set(true);
    
    this.productsService.getAll({
      searchTerm: term,
      isActive: true,
      page: 1,
      pageSize: this.maxResults
    }).subscribe({
      next: (result) => {
        // Manejar diferentes formatos de respuesta
        let products: Product[] = [];
        if (result && typeof result === 'object') {
          if (Array.isArray(result)) {
            products = result;
          } else if (result.items && Array.isArray(result.items)) {
            products = result.items;
          }
        }
        
        // Ordenar por relevancia: coincidencias exactas primero, luego por nombre
        const sorted = this.sortByRelevance(products, term);
        this.suggestions.set(sorted.slice(0, this.maxResults));
        this.selectedIndex.set(-1);
        this.isLoading.set(false);
        
        // Mostrar dropdown si hay resultados
        const hasResults = sorted.length > 0;
        if (hasResults) {
          this.showDropdown.set(true);
        } else {
          this.showDropdown.set(false);
        }
      },
      error: (error) => {
        console.error('Error searching products:', error);
        this.suggestions.set([]);
        this.showDropdown.set(false);
        this.isLoading.set(false);
      }
    });
  }

  private sortByRelevance(products: Product[], term: string): Product[] {
    const termLower = term.toLowerCase();
    return products.sort((a, b) => {
      const aNameLower = a.name.toLowerCase();
      const bNameLower = b.name.toLowerCase();
      
      // Coincidencia exacta en el nombre
      if (aNameLower === termLower && bNameLower !== termLower) return -1;
      if (bNameLower === termLower && aNameLower !== termLower) return 1;
      
      // Empieza con el término
      if (aNameLower.startsWith(termLower) && !bNameLower.startsWith(termLower)) return -1;
      if (bNameLower.startsWith(termLower) && !aNameLower.startsWith(termLower)) return 1;
      
      // Coincidencia en código
      if (a.code.toLowerCase() === termLower && b.code.toLowerCase() !== termLower) return -1;
      if (b.code.toLowerCase() === termLower && a.code.toLowerCase() !== termLower) return 1;
      
      // Ordenar alfabéticamente
      return aNameLower.localeCompare(bNameLower);
    });
  }

  selectProduct(product: Product): void {
    this.searchTerm.set(product.name);
    this.showDropdown.set(false);
    this.selectedIndex.set(-1);
    this.productSelected.emit(product);
    
    // También emitir el evento de búsqueda
    this.search.emit(product.name);
  }

  onSearch(): void {
    const term = this.searchTerm().trim();
    if (term) {
      this.showDropdown.set(false);
      this.search.emit(term);
    }
  }

  clearSearch(): void {
    this.searchTerm.set('');
    this.suggestions.set([]);
    this.showDropdown.set(false);
    this.selectedIndex.set(-1);
  }

  getPriceFormatted(price: number): string {
    return `S/ ${price.toFixed(2)}`;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}


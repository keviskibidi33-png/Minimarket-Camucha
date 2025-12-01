import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { OfertasService, Oferta, CreateOferta, UpdateOferta } from '../../../core/services/ofertas.service';
import { CategoriesService } from '../../../core/services/categories.service';
import { ProductsService } from '../../../core/services/products.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-ofertas',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './ofertas.component.html',
  styleUrl: './ofertas.component.css'
})
export class OfertasComponent implements OnInit {
  ofertas = signal<Oferta[]>([]);
  categories = signal<any[]>([]);
  products = signal<any[]>([]);
  isLoading = signal(false);
  showForm = signal(false);
  editingOferta = signal<Oferta | null>(null);

  // Form data
  nombre = signal('');
  descripcion = signal('');
  descuentoTipo = signal<0 | 1>(0); // 0 = Porcentaje, 1 = MontoFijo
  descuentoValor = signal(0);
  aplicarA = signal<'categorias' | 'productos'>('categorias');
  categoriasIds = signal<string[]>([]);
  productosIds = signal<string[]>([]);
  fechaInicio = signal('');
  fechaFin = signal('');
  fechaPreset = signal<string>(''); // Opción predefinida seleccionada
  activa = signal(true);
  orden = signal(0);
  imagenUrl = signal('');

  constructor(
    private ofertasService: OfertasService,
    private categoriesService: CategoriesService,
    private productsService: ProductsService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadOfertas();
    this.loadCategories();
    this.loadProducts();
  }

  loadOfertas(): void {
    this.isLoading.set(true);
    this.ofertasService.getAll().subscribe({
      next: (ofertas) => {
        this.ofertas.set(ofertas);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading ofertas:', error);
        this.toastService.error('Error al cargar ofertas');
        this.isLoading.set(false);
      }
    });
  }

  loadCategories(): void {
    this.categoriesService.getAllWithoutPagination().subscribe({
      next: (categories) => {
        this.categories.set(categories);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  loadProducts(): void {
    this.productsService.getAll({ pageSize: 1000 }).subscribe({
      next: (result) => {
        const products = result.items || (Array.isArray(result) ? result : []);
        this.products.set(products);
      },
      error: (error) => {
        console.error('Error loading products:', error);
      }
    });
  }

  openForm(oferta?: Oferta): void {
    if (oferta) {
      this.editingOferta.set(oferta);
      this.nombre.set(oferta.nombre);
      this.descripcion.set(oferta.descripcion || '');
      this.descuentoTipo.set(oferta.descuentoTipo as 0 | 1);
      this.descuentoValor.set(oferta.descuentoValor);
      this.categoriasIds.set(oferta.categoriasIds);
      this.productosIds.set(oferta.productosIds);
      this.aplicarA.set(oferta.categoriasIds.length > 0 ? 'categorias' : 'productos');
      this.fechaInicio.set(new Date(oferta.fechaInicio).toISOString().split('T')[0]);
      this.fechaFin.set(new Date(oferta.fechaFin).toISOString().split('T')[0]);
      this.fechaPreset.set(''); // Limpiar preset al editar
      this.activa.set(oferta.activa);
      this.orden.set(oferta.orden);
      this.imagenUrl.set(oferta.imagenUrl || '');
    } else {
      this.resetForm();
    }
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingOferta.set(null);
    this.resetForm();
  }

  resetForm(): void {
    this.nombre.set('');
    this.descripcion.set('');
    this.descuentoTipo.set(0);
    this.descuentoValor.set(0);
    this.aplicarA.set('categorias');
    this.categoriasIds.set([]);
    this.productosIds.set([]);
    this.fechaInicio.set('');
    this.fechaFin.set('');
    this.fechaPreset.set('');
    this.activa.set(true);
    this.orden.set(0);
    this.imagenUrl.set('');
  }

  applyDatePreset(preset: string): void {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    
    let startDate: Date;
    let endDate: Date;

    switch (preset) {
      case 'today':
        startDate = new Date(today);
        endDate = new Date(today);
        break;
      case '3days':
        startDate = new Date(today);
        endDate = new Date(today);
        endDate.setDate(endDate.getDate() + 2); // Hoy + 2 días = 3 días total
        break;
      case '1week':
        startDate = new Date(today);
        endDate = new Date(today);
        endDate.setDate(endDate.getDate() + 6); // Hoy + 6 días = 7 días total
        break;
      case '1month':
        startDate = new Date(today);
        endDate = new Date(today);
        endDate.setMonth(endDate.getMonth() + 1);
        endDate.setDate(endDate.getDate() - 1); // Un mes completo desde hoy
        break;
      default:
        return;
    }

    // Formatear fechas como YYYY-MM-DD para el input type="date"
    this.fechaInicio.set(startDate.toISOString().split('T')[0]);
    this.fechaFin.set(endDate.toISOString().split('T')[0]);
    this.fechaPreset.set(preset);
  }

  onDateChange(): void {
    // Si el usuario edita manualmente las fechas, limpiar el preset
    this.fechaPreset.set('');
  }

  toggleCategoria(categoriaId: string): void {
    const ids = [...this.categoriasIds()];
    const index = ids.indexOf(categoriaId);
    if (index > -1) {
      ids.splice(index, 1);
    } else {
      ids.push(categoriaId);
    }
    this.categoriasIds.set(ids);
  }

  toggleProducto(productoId: string): void {
    const ids = [...this.productosIds()];
    const index = ids.indexOf(productoId);
    if (index > -1) {
      ids.splice(index, 1);
    } else {
      ids.push(productoId);
    }
    this.productosIds.set(ids);
  }

  saveOferta(): void {
    if (!this.nombre().trim() || !this.fechaInicio() || !this.fechaFin()) {
      this.toastService.error('Complete todos los campos requeridos');
      return;
    }

    if (new Date(this.fechaInicio()) >= new Date(this.fechaFin())) {
      this.toastService.error('La fecha de inicio debe ser anterior a la fecha de fin');
      return;
    }

    const ofertaData: CreateOferta | UpdateOferta = {
      nombre: this.nombre(),
      descripcion: this.descripcion() || undefined,
      descuentoTipo: this.descuentoTipo(),
      descuentoValor: this.descuentoValor(),
      categoriasIds: this.aplicarA() === 'categorias' ? this.categoriasIds() : [],
      productosIds: this.aplicarA() === 'productos' ? this.productosIds() : [],
      fechaInicio: new Date(this.fechaInicio()).toISOString(),
      fechaFin: new Date(this.fechaFin()).toISOString(),
      activa: this.activa(),
      orden: this.orden(),
      imagenUrl: this.imagenUrl().trim() || undefined
    };

    const oferta = this.editingOferta();
    if (oferta) {
      this.ofertasService.update(oferta.id, ofertaData).subscribe({
        next: () => {
          this.toastService.success('Oferta actualizada correctamente');
          this.loadOfertas();
          this.closeForm();
        },
        error: (error) => {
          console.error('Error updating oferta:', error);
          this.toastService.error('Error al actualizar oferta');
        }
      });
    } else {
      this.ofertasService.create(ofertaData).subscribe({
        next: () => {
          this.toastService.success('Oferta creada correctamente');
          this.loadOfertas();
          this.closeForm();
        },
        error: (error) => {
          console.error('Error creating oferta:', error);
          this.toastService.error('Error al crear oferta');
        }
      });
    }
  }

  deleteOferta(oferta: Oferta): void {
    if (confirm(`¿Está seguro de eliminar la oferta "${oferta.nombre}"?`)) {
      this.ofertasService.delete(oferta.id).subscribe({
        next: () => {
          this.toastService.success('Oferta eliminada correctamente');
          this.loadOfertas();
        },
        error: (error) => {
          console.error('Error deleting oferta:', error);
          this.toastService.error('Error al eliminar oferta');
        }
      });
    }
  }

  isActive(oferta: Oferta): boolean {
    const now = new Date();
    const inicio = new Date(oferta.fechaInicio);
    const fin = new Date(oferta.fechaFin);
    return oferta.activa && now >= inicio && now <= fin;
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  getDescuentoText(): string {
    if (this.descuentoTipo() === 0) {
      return `${this.descuentoValor()}% OFF`;
    } else {
      return `S/ ${this.descuentoValor()} OFF`;
    }
  }

  formatPreviewDate(dateString: string): string {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleDateString('es-PE', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  getPreviewAplicacionText(): string {
    if (this.aplicarA() === 'categorias') {
      const count = this.categoriasIds().length;
      return count > 0 ? `Aplica a ${count} categoría${count !== 1 ? 's' : ''}` : 'No hay categorías seleccionadas';
    } else {
      const count = this.productosIds().length;
      return count > 0 ? `Aplica a ${count} producto${count !== 1 ? 's' : ''}` : 'No hay productos seleccionados';
    }
  }

  isPreviewActive(): boolean {
    if (!this.fechaInicio() || !this.fechaFin()) return false;
    const now = new Date();
    const inicio = new Date(this.fechaInicio());
    const fin = new Date(this.fechaFin());
    return this.activa() && now >= inicio && now <= fin;
  }

  getOrdenText(): string {
    const ordenValue = this.orden();
    if (ordenValue === 0) {
      return 'Aparecerá al final';
    } else if (ordenValue === 1) {
      return 'Aparecerá primero';
    } else {
      return `Aparecerá en posición ${ordenValue}`;
    }
  }
}


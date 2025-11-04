import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PagesService, Page, CreatePage, UpdatePage, CreatePageSection, UpdatePageSection } from '../../../core/services/pages.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-page-builder',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './page-builder.component.html',
  styleUrl: './page-builder.component.css'
})
export class PageBuilderComponent implements OnInit {
  pages = signal<Page[]>([]);
  isLoading = signal(false);
  showForm = signal(false);
  editingPage = signal<Page | null>(null);

  // Form data
  titulo = signal('');
  slug = signal('');
  tipoPlantilla = signal<0 | 1 | 2>(2);
  metaDescription = signal('');
  keywords = signal('');
  orden = signal(0);
  activa = signal(true);
  sections = signal<(CreatePageSection | UpdatePageSection)[]>([]);

  // Tipos de secciones disponibles
  seccionTipos = [
    { value: 0, label: 'Banner' },
    { value: 1, label: 'Texto e Imagen' },
    { value: 2, label: 'Grid de Productos' },
    { value: 3, label: 'Categorías' },
    { value: 4, label: 'Galería' },
    { value: 5, label: 'Testimonios' },
    { value: 6, label: 'CTA (Call to Action)' },
    { value: 7, label: 'Newsletter' }
  ];

  constructor(
    private pagesService: PagesService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadPages();
  }

  loadPages(): void {
    this.isLoading.set(true);
    this.pagesService.getAll().subscribe({
      next: (pages) => {
        this.pages.set(pages);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading pages:', error);
        this.toastService.error('Error al cargar páginas');
        this.isLoading.set(false);
      }
    });
  }

  openForm(page?: Page): void {
    if (page) {
      this.editingPage.set(page);
      this.titulo.set(page.titulo);
      this.slug.set(page.slug);
      this.tipoPlantilla.set(page.tipoPlantilla as 0 | 1 | 2);
      this.metaDescription.set(page.metaDescription || '');
      this.keywords.set(page.keywords || '');
      this.orden.set(page.orden);
      this.activa.set(page.activa);
      this.sections.set(page.sections.map(s => ({
        id: s.id,
        seccionTipo: s.seccionTipo,
        orden: s.orden,
        datos: s.datos
      })));
    } else {
      this.resetForm();
    }
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingPage.set(null);
    this.resetForm();
  }

  resetForm(): void {
    this.titulo.set('');
    this.slug.set('');
    this.tipoPlantilla.set(2);
    this.metaDescription.set('');
    this.keywords.set('');
    this.orden.set(0);
    this.activa.set(true);
    this.sections.set([]);
  }

  addSection(): void {
    const newSection: CreatePageSection = {
      seccionTipo: 0,
      orden: this.sections().length,
      datos: {}
    };
    this.sections.set([...this.sections(), newSection]);
  }

  removeSection(index: number): void {
    const sections = [...this.sections()];
    sections.splice(index, 1);
    // Reordenar
    sections.forEach((s, i) => {
      s.orden = i;
    });
    this.sections.set(sections);
  }

  moveSection(index: number, direction: 'up' | 'down'): void {
    const sections = [...this.sections()];
    if (direction === 'up' && index > 0) {
      [sections[index], sections[index - 1]] = [sections[index - 1], sections[index]];
      sections[index].orden = index;
      sections[index - 1].orden = index - 1;
    } else if (direction === 'down' && index < sections.length - 1) {
      [sections[index], sections[index + 1]] = [sections[index + 1], sections[index]];
      sections[index].orden = index;
      sections[index + 1].orden = index + 1;
    }
    this.sections.set(sections);
  }

  updateSectionData(index: number, key: string, value: any): void {
    const sections = [...this.sections()];
    if (!sections[index].datos) {
      sections[index].datos = {};
    }
    sections[index].datos[key] = value;
    this.sections.set(sections);
  }

  savePage(): void {
    if (!this.titulo().trim() || !this.slug().trim()) {
      this.toastService.error('Complete todos los campos requeridos');
      return;
    }

    const page = this.editingPage();
    if (page) {
      const updateData: UpdatePage = {
        titulo: this.titulo(),
        slug: this.slug(),
        tipoPlantilla: this.tipoPlantilla(),
        metaDescription: this.metaDescription() || undefined,
        keywords: this.keywords() || undefined,
        orden: this.orden(),
        activa: this.activa(),
        sections: this.sections().map(s => ({
          id: 'id' in s ? s.id : undefined,
          seccionTipo: s.seccionTipo,
          orden: s.orden,
          datos: s.datos
        }))
      };

      this.pagesService.update(page.id, updateData).subscribe({
        next: () => {
          this.toastService.success('Página actualizada correctamente');
          this.loadPages();
          this.closeForm();
        },
        error: (error) => {
          console.error('Error updating page:', error);
          this.toastService.error('Error al actualizar página');
        }
      });
    } else {
      const createData: CreatePage = {
        titulo: this.titulo(),
        slug: this.slug(),
        tipoPlantilla: this.tipoPlantilla(),
        metaDescription: this.metaDescription() || undefined,
        keywords: this.keywords() || undefined,
        orden: this.orden(),
        activa: this.activa(),
        sections: this.sections().map(s => ({
          seccionTipo: s.seccionTipo,
          orden: s.orden,
          datos: s.datos
        }))
      };

      this.pagesService.create(createData).subscribe({
        next: () => {
          this.toastService.success('Página creada correctamente');
          this.loadPages();
          this.closeForm();
        },
        error: (error) => {
          console.error('Error creating page:', error);
          this.toastService.error('Error al crear página');
        }
      });
    }
  }

  deletePage(page: Page): void {
    if (confirm(`¿Está seguro de eliminar la página "${page.titulo}"?`)) {
      this.pagesService.delete(page.id).subscribe({
        next: () => {
          this.toastService.success('Página eliminada correctamente');
          this.loadPages();
        },
        error: (error) => {
          console.error('Error deleting page:', error);
          this.toastService.error('Error al eliminar página');
        }
      });
    }
  }

  generateSlug(): void {
    const slug = this.titulo()
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '');
    this.slug.set(slug);
  }
}


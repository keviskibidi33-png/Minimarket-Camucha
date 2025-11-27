import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { QuillModule } from 'ngx-quill';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { PagesService, Page, CreatePage, UpdatePage, CreatePageSection, UpdatePageSection } from '../../../core/services/pages.service';
import { SettingsService, UpdateSystemSettings } from '../../../core/services/settings.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-page-builder',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, QuillModule],
  templateUrl: './page-builder.component.html',
  styleUrl: './page-builder.component.css'
})
export class PageBuilderComponent implements OnInit {
  pages = signal<Page[]>([]);
  isLoading = signal(false);
  showForm = signal(false);
  editingPage = signal<Page | null>(null);
  enableNewsInNavbar = signal(true); // Control global para activar/desactivar noticias en navbar

  // Form data - Blog de Noticias
  titulo = signal('');
  slug = signal('');
  tipoPlantilla = signal<0 | 1 | 2>(2);
  metaDescription = signal('');
  keywords = signal('');
  orden = signal(0);
  activa = signal(true);
  contenido = signal(''); // Contenido principal del artículo (simplificado)
  imagenDestacada = signal(''); // URL de imagen destacada
  fechaPublicacion = signal(''); // Fecha de publicación
  showPreview = signal(false);
  viewMode = signal<'edit' | 'preview' | 'split'>('split'); // Modo de visualización tipo WordPress

  // Configuración del editor Quill
  quillModules = {
    toolbar: [
      ['bold', 'italic', 'underline', 'strike'],
      ['blockquote', 'code-block'],
      [{ 'header': 1 }, { 'header': 2 }],
      [{ 'list': 'ordered'}, { 'list': 'bullet' }],
      [{ 'script': 'sub'}, { 'script': 'super' }],
      [{ 'indent': '-1'}, { 'indent': '+1' }],
      [{ 'direction': 'rtl' }],
      [{ 'size': ['small', false, 'large', 'huge'] }],
      [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
      [{ 'color': [] }, { 'background': [] }],
      [{ 'font': [] }],
      [{ 'align': [] }],
      ['clean'],
      ['link', 'image']
    ]
  };

  // Helper methods para Blog de Noticias
  getFeaturedImage(page: Page): string | null {
    // Buscar imagen destacada en las secciones o en datos
    if (page.sections && page.sections.length > 0) {
      for (const section of page.sections) {
        if (section.datos && section.datos['imagenUrl']) {
          return section.datos['imagenUrl'];
        }
        if (section.datos && section.datos['imagenDestacada']) {
          return section.datos['imagenDestacada'];
        }
      }
    }
    return null;
  }

  formatDate(dateString: string): string {
    if (!dateString) return '';
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('es-PE', { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric' 
      });
    } catch {
      return dateString;
    }
  }

  getContentPreview(page: Page): string {
    // Obtener preview del contenido
    if (page.sections && page.sections.length > 0) {
      const firstSection = page.sections[0];
      if (firstSection.datos && firstSection.datos['contenido']) {
        const content = firstSection.datos['contenido'];
        // Remover HTML tags para preview
        const text = content.replace(/<[^>]*>/g, '');
        return text.length > 50 ? text.substring(0, 50) + '...' : text;
      }
    }
    return 'Sin contenido';
  }

  constructor(
    private pagesService: PagesService,
    private settingsService: SettingsService,
    private toastService: ToastService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.loadPages();
    this.loadEnableNewsInNavbarSetting();
  }

  loadEnableNewsInNavbarSetting(): void {
    this.settingsService.getByKey('enable_news_in_navbar').subscribe({
      next: (setting) => {
        if (setting) {
          this.enableNewsInNavbar.set(setting.value.toLowerCase() === 'true' || setting.value === '1');
        } else {
          // Por defecto activado
          this.enableNewsInNavbar.set(true);
        }
      },
      error: () => {
        // Si hay error, asumir que está activado (comportamiento por defecto)
        this.enableNewsInNavbar.set(true);
      }
    });
  }

  toggleEnableNewsInNavbar(): void {
    const newValue = !this.enableNewsInNavbar();
    this.enableNewsInNavbar.set(newValue);
    
    // Guardar inmediatamente la configuración global
    const setting: UpdateSystemSettings = {
      key: 'enable_news_in_navbar',
      value: newValue ? 'true' : 'false',
      description: 'Activar o desactivar la funcionalidad de mostrar noticias en el navbar',
      isActive: true
    };
    
    this.settingsService.update('enable_news_in_navbar', setting).subscribe({
      next: () => {
        this.toastService.success(newValue ? 'Noticias en navbar activadas' : 'Noticias en navbar desactivadas');
      },
      error: (error) => {
        console.error('Error saving enable_news_in_navbar setting:', error);
        this.toastService.error('Error al guardar configuración');
        // Revertir el cambio
        this.enableNewsInNavbar.set(!newValue);
      }
    });
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
        this.isLoading.set(false);
        
        // Manejar diferentes tipos de errores
        if (error.status === 0) {
          // Error de conexión (backend no disponible)
          this.toastService.error('No se pudo conectar con el servidor. Verifique que el backend esté corriendo.');
        } else if (error.status === 500) {
          // Error del servidor (probablemente migración pendiente)
          this.toastService.error('Error del servidor. Por favor, reinicie el backend para aplicar las migraciones pendientes.');
        } else {
          this.toastService.error('Error al cargar páginas. Por favor, intente nuevamente.');
        }
        
        // Establecer array vacío para evitar errores en el template
        this.pages.set([]);
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
      
      // Extraer contenido e imagen destacada de las secciones
      if (page.sections && page.sections.length > 0) {
        const firstSection = page.sections[0];
        this.contenido.set(firstSection.datos?.['contenido'] || '');
        this.imagenDestacada.set(firstSection.datos?.['imagenUrl'] || firstSection.datos?.['imagenDestacada'] || '');
      } else {
        this.contenido.set('');
        this.imagenDestacada.set('');
      }
      
      // Fecha de publicación
      if (page.createdAt) {
        this.fechaPublicacion.set(new Date(page.createdAt).toISOString().split('T')[0]);
      } else {
        this.fechaPublicacion.set(new Date().toISOString().split('T')[0]);
      }
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
    this.contenido.set('');
    this.imagenDestacada.set('');
    this.fechaPublicacion.set(new Date().toISOString().split('T')[0]);
    this.showPreview.set(false);
  }


  savePage(): void {
    if (!this.titulo().trim() || !this.slug().trim()) {
      this.toastService.error('Complete todos los campos requeridos');
      return;
    }

    // Construir secciones simplificadas para blog
    const sections: (CreatePageSection | UpdatePageSection)[] = [];
    
    // Sección principal con contenido del blog
    if (this.contenido().trim() || this.imagenDestacada().trim()) {
      const mainSection: CreatePageSection = {
        seccionTipo: 1, // Texto e Imagen
        orden: 0,
        datos: {
          contenido: this.contenido(),
          imagenUrl: this.imagenDestacada(),
          imagenDestacada: this.imagenDestacada(),
          titulo: this.titulo()
        }
      };
      sections.push(mainSection);
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
        mostrarEnNavbar: page.mostrarEnNavbar ?? false, // Mantener el valor actual (se gestiona desde Configuraciones)
        sections: sections.map((s, index) => ({
          id: page.sections && page.sections[index] ? page.sections[index].id : undefined,
          seccionTipo: s.seccionTipo,
          orden: index,
          datos: s.datos
        }))
      };

      this.pagesService.update(page.id, updateData).subscribe({
        next: () => {
          this.toastService.success('Noticia actualizada correctamente');
          this.loadPages();
          this.closeForm();
        },
        error: (error) => {
          console.error('Error updating page:', error);
          this.toastService.error('Error al actualizar noticia');
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
        mostrarEnNavbar: false, // Por defecto no mostrar en navbar (se gestiona desde Configuraciones)
        sections: sections.map((s, index) => ({
          seccionTipo: s.seccionTipo,
          orden: index,
          datos: s.datos
        }))
      };

      this.pagesService.create(createData).subscribe({
        next: () => {
          this.toastService.success('Noticia creada correctamente');
          this.loadPages();
          this.closeForm();
        },
        error: (error) => {
          console.error('Error creating page:', error);
          this.toastService.error('Error al crear noticia');
        }
      });
    }
  }

  deletePage(page: Page): void {
    if (confirm(`¿Está seguro de eliminar la noticia "${page.titulo}"?`)) {
      this.pagesService.delete(page.id).subscribe({
        next: () => {
          this.toastService.success('Noticia eliminada correctamente');
          this.loadPages();
        },
        error: (error) => {
          console.error('Error deleting page:', error);
          // Mostrar mensaje específico según el tipo de error
          if (error.status === 422) {
            // Regla de negocio violada (última noticia activa)
            this.toastService.error(error.error?.message || 'No se puede eliminar la última noticia activa. Debe haber al menos una noticia activa en el sistema.');
          } else if (error.status === 500) {
            this.toastService.error('Error interno del servidor. Por favor, verifique que la base de datos esté actualizada y reinicie el servidor.');
          } else {
            this.toastService.error(error.error?.message || 'Error al eliminar noticia. Por favor, intente nuevamente.');
          }
        }
      });
    }
  }

  // Métodos para toggle rápido desde los cards
  toggleActiva(page: Page): void {
    const newActiva = !page.activa;
    
    // Si se está desactivando y es la última noticia activa, mostrar error
    if (!newActiva) {
      const activePages = this.pages().filter(p => p.activa && p.id !== page.id);
      if (activePages.length === 0) {
        this.toastService.error('No se puede desactivar la última noticia activa. Debe haber al menos una noticia activa en el sistema.');
        return;
      }
    }

    const updateData: UpdatePage = {
      titulo: page.titulo,
      slug: page.slug,
      tipoPlantilla: page.tipoPlantilla,
      metaDescription: page.metaDescription,
      keywords: page.keywords,
      orden: page.orden,
      activa: newActiva,
      mostrarEnNavbar: newActiva ? page.mostrarEnNavbar : false, // Si se desactiva, también quitar del navbar
      sections: page.sections.map(s => ({
        id: s.id,
        seccionTipo: s.seccionTipo,
        orden: s.orden,
        datos: s.datos
      }))
    };

    this.pagesService.update(page.id, updateData).subscribe({
      next: () => {
        this.toastService.success(newActiva ? 'Noticia publicada' : 'Noticia despublicada');
        this.loadPages();
      },
      error: (error) => {
        console.error('Error updating page:', error);
        this.toastService.error('Error al actualizar noticia');
      }
    });
  }

  toggleMostrarEnNavbar(page: Page): void {
    // Si la funcionalidad global está desactivada, no permitir cambios
    if (!this.enableNewsInNavbar()) {
      this.toastService.error('Primero debes activar "Mostrar Noticias en Navbar" en el control superior');
      return;
    }
    
    const newMostrarEnNavbar = !(page.mostrarEnNavbar || false);
    
    // Si se está desmarcando y es la última noticia visible en navbar, mostrar error
    if (!newMostrarEnNavbar && this.isLastVisibleInNavbar(page)) {
      this.toastService.error('Debe haber al menos una noticia visible en el navbar. No se puede desmarcar la última.');
      return;
    }

    const updateData: UpdatePage = {
      titulo: page.titulo,
      slug: page.slug,
      tipoPlantilla: page.tipoPlantilla,
      metaDescription: page.metaDescription,
      keywords: page.keywords,
      orden: page.orden,
      activa: page.activa,
      mostrarEnNavbar: newMostrarEnNavbar,
      sections: page.sections.map(s => ({
        id: s.id,
        seccionTipo: s.seccionTipo,
        orden: s.orden,
        datos: s.datos
      }))
    };

    this.pagesService.update(page.id, updateData).subscribe({
      next: () => {
        this.toastService.success(newMostrarEnNavbar ? 'Noticia agregada al navbar' : 'Noticia removida del navbar');
        this.loadPages();
      },
      error: (error) => {
        console.error('Error updating page:', error);
        this.toastService.error('Error al actualizar noticia');
      }
    });
  }

  isLastVisibleInNavbar(page: Page): boolean {
    // Verificar si es la última noticia visible en navbar (sin importar si está activa o no)
    const visibleInNavbar = this.pages().filter(p => p.mostrarEnNavbar);
    return visibleInNavbar.length === 1 && visibleInNavbar[0].id === page.id;
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

  togglePreview(): void {
    this.showPreview.set(!this.showPreview());
  }

  setViewMode(mode: 'edit' | 'preview' | 'split'): void {
    this.viewMode.set(mode);
    if (mode === 'preview') {
      this.showPreview.set(true);
    }
  }


  // Método para sanitizar HTML
  getSafeHtml(html: string): SafeHtml {
    if (!html) return this.sanitizer.bypassSecurityTrustHtml('');
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }

  // Computed para preview
  previewData = computed(() => ({
    titulo: this.titulo(),
    slug: this.slug(),
    tipoPlantilla: this.tipoPlantilla(),
    metaDescription: this.metaDescription(),
    keywords: this.keywords(),
    contenido: this.contenido(),
    imagenDestacada: this.imagenDestacada(),
    fechaPublicacion: this.fechaPublicacion()
  }));
}


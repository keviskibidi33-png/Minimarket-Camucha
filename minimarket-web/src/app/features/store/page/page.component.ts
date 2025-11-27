import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { PagesService, Page } from '../../../core/services/pages.service';
import { StoreHeaderComponent } from '../../../shared/components/store-header/store-header.component';
import { StoreFooterComponent } from '../../../shared/components/store-footer/store-footer.component';

@Component({
  selector: 'app-page',
  standalone: true,
  imports: [CommonModule, RouterModule, StoreHeaderComponent, StoreFooterComponent],
  templateUrl: './page.component.html',
  styleUrl: './page.component.css'
})
export class PageComponent implements OnInit {
  page = signal<Page | null>(null);
  isLoading = signal(true);
  error = signal<string | null>(null);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private pagesService: PagesService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const slug = params.get('slug');
      if (slug) {
        this.loadPage(slug);
      } else {
        this.error.set('Slug no proporcionado');
        this.isLoading.set(false);
      }
    });
  }

  loadPage(slug: string): void {
    this.isLoading.set(true);
    this.error.set(null);
    
    this.pagesService.getBySlug(slug).subscribe({
      next: (page) => {
        if (!page.activa) {
          this.error.set('Esta página no está disponible');
          this.isLoading.set(false);
          return;
        }
        this.page.set(page);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading page:', error);
        this.error.set('Página no encontrada');
        this.isLoading.set(false);
      }
    });
  }

  getOrderedSections(): any[] {
    if (!this.page()) return [];
    // Ordenar secciones por el campo 'orden' y luego por índice
    return [...this.page()!.sections].sort((a, b) => {
      if (a.orden !== undefined && b.orden !== undefined) {
        return a.orden - b.orden;
      }
      return 0;
    });
  }

  renderSection(section: any, pageTitle?: string): string {
    // Renderizar cada sección según su tipo
    let html = '';
    
    switch (section.seccionTipo) {
      case 0: // Banner
        html = this.renderBanner(section, pageTitle);
        break;
      case 1: // Texto e Imagen
        html = this.renderTextImage(section, pageTitle);
        break;
      case 2: // Grid de Productos
        html = this.renderProductGrid(section);
        break;
      case 3: // Categorías
        html = this.renderCategories(section);
        break;
      case 4: // Galería
        html = this.renderGallery(section);
        break;
      case 5: // Testimonios
        html = this.renderTestimonials(section);
        break;
      case 6: // CTA
        html = this.renderCTA(section);
        break;
      case 7: // Newsletter
        html = this.renderNewsletter(section);
        break;
      default:
        html = this.renderDefault(section, pageTitle);
    }
    
    return html;
  }

  getSafeHtml(html: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }

  private renderBanner(section: any, pageTitle?: string): string {
    const titulo = section.datos['titulo'] || '';
    const contenido = section.datos['contenido'] || '';
    const imagenUrl = section.datos['imagenUrl'] || '';
    
    // Si el título de la sección es igual al título de la página, no mostrar el título de la sección (evitar duplicación)
    const mostrarTitulo = titulo && titulo !== pageTitle;
    
    return `
      <div class="relative w-full h-96 rounded-xl overflow-hidden shadow-lg mb-8">
        ${imagenUrl ? `<div class="absolute inset-0 bg-cover bg-center" style="background-image: url('${imagenUrl}')"></div>` : ''}
        <div class="absolute inset-0 bg-black/40"></div>
        <div class="relative h-full flex flex-col items-center justify-center text-center text-white p-6">
          ${mostrarTitulo ? `<h1 class="text-4xl font-bold mb-4">${titulo}</h1>` : ''}
          ${contenido ? `<div class="prose prose-invert max-w-2xl">${contenido}</div>` : ''}
        </div>
      </div>
    `;
  }

  private renderTextImage(section: any, pageTitle?: string): string {
    const titulo = section.datos['titulo'] || '';
    const contenido = section.datos['contenido'] || '';
    const imagenUrl = section.datos['imagenUrl'] || section.datos['imagenDestacada'] || '';
    const posicion = section.datos['posicion'] || 'left'; // left o right
    
    // Si el título de la sección es igual al título de la página, no mostrar el título de la sección (evitar duplicación)
    const mostrarTitulo = titulo && titulo !== pageTitle;
    
    const imageHtml = imagenUrl ? `
      <div class="flex-shrink-0">
        <img src="${imagenUrl}" alt="${mostrarTitulo ? titulo : pageTitle || 'Imagen'}" class="w-full h-auto rounded-lg shadow-md">
      </div>
    ` : '';
    
    const contentHtml = `
      <div class="flex-1">
        ${mostrarTitulo ? `<h2 class="text-3xl font-bold mb-4">${titulo}</h2>` : ''}
        ${contenido ? `<div class="prose max-w-none dark:prose-invert">${contenido}</div>` : ''}
      </div>
    `;
    
    // Si hay imagen, mostrar en grid, si no, solo el contenido
    if (imagenUrl) {
      return `
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8 items-start mb-8">
          ${posicion === 'right' ? imageHtml + contentHtml : contentHtml + imageHtml}
        </div>
      `;
    } else {
      return `
        <div class="mb-8">
          ${contentHtml}
        </div>
      `;
    }
  }

  private renderProductGrid(section: any): string {
    // Por ahora solo mostrar un placeholder, se puede integrar con productos después
    return `
      <div class="mb-8">
        <p class="text-gray-500 dark:text-gray-400">Grid de Productos - Funcionalidad pendiente</p>
      </div>
    `;
  }

  private renderCategories(section: any): string {
    // Por ahora solo mostrar un placeholder
    return `
      <div class="mb-8">
        <p class="text-gray-500 dark:text-gray-400">Categorías - Funcionalidad pendiente</p>
      </div>
    `;
  }

  private renderGallery(section: any): string {
    const imagenes = section.datos['imagenes'] || [];
    if (imagenes.length === 0) return '';
    
    return `
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        ${imagenes.map((img: string) => `
          <img src="${img}" alt="Galería" class="w-full h-64 object-cover rounded-lg">
        `).join('')}
      </div>
    `;
  }

  private renderTestimonials(section: any): string {
    const testimonios = section.datos['testimonios'] || [];
    if (testimonios.length === 0) return '';
    
    return `
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
        ${testimonios.map((test: any) => `
          <div class="bg-white dark:bg-gray-800 p-6 rounded-lg shadow-md">
            <p class="text-gray-700 dark:text-gray-300 mb-4">"${test.texto || ''}"</p>
            <p class="font-semibold text-gray-900 dark:text-white">${test.autor || ''}</p>
          </div>
        `).join('')}
      </div>
    `;
  }

  private renderCTA(section: any): string {
    const titulo = section.datos['titulo'] || '';
    const contenido = section.datos['contenido'] || '';
    const botonTexto = section.datos['botonTexto'] || 'Más información';
    const botonUrl = section.datos['botonUrl'] || '#';
    
    return `
      <div class="bg-primary text-white rounded-xl p-8 text-center mb-8">
        ${titulo ? `<h2 class="text-3xl font-bold mb-4">${titulo}</h2>` : ''}
        ${contenido ? `<p class="mb-6">${contenido}</p>` : ''}
        <a href="${botonUrl}" class="inline-block bg-white text-primary px-6 py-3 rounded-lg font-semibold hover:bg-gray-100">
          ${botonTexto}
        </a>
      </div>
    `;
  }

  private renderNewsletter(section: any): string {
    const titulo = section.datos['titulo'] || 'Suscríbete a nuestro newsletter';
    const contenido = section.datos['contenido'] || '';
    
    return `
      <div class="bg-gray-100 dark:bg-gray-800 rounded-xl p-8 text-center mb-8">
        ${titulo ? `<h2 class="text-2xl font-bold mb-4">${titulo}</h2>` : ''}
        ${contenido ? `<p class="mb-6 text-gray-700 dark:text-gray-300">${contenido}</p>` : ''}
        <form class="flex gap-2 max-w-md mx-auto">
          <input type="email" placeholder="Tu email" class="flex-1 px-4 py-2 rounded-lg border border-gray-300 dark:border-gray-700">
          <button type="submit" class="bg-primary text-white px-6 py-2 rounded-lg font-semibold hover:bg-primary/90">
            Suscribirse
          </button>
        </form>
      </div>
    `;
  }

  private renderDefault(section: any, pageTitle?: string): string {
    const titulo = section.datos['titulo'] || '';
    const contenido = section.datos['contenido'] || '';
    
    // Si el título de la sección es igual al título de la página, no mostrar el título de la sección (evitar duplicación)
    const mostrarTitulo = titulo && titulo !== pageTitle;
    
    return `
      <div class="mb-8">
        ${mostrarTitulo ? `<h2 class="text-2xl font-bold mb-4">${titulo}</h2>` : ''}
        ${contenido ? `<div class="prose max-w-none dark:prose-invert">${contenido}</div>` : ''}
      </div>
    `;
  }
}


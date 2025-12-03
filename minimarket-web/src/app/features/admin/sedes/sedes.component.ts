import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SedesService, Sede, CreateSede, UpdateSede } from '../../../core/services/sedes.service';
import { ToastService } from '../../../shared/services/toast.service';
import { FilesService } from '../../../core/services/files.service';

@Component({
  selector: 'app-sedes',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './sedes.component.html',
  styleUrl: './sedes.component.css'
})
export class SedesComponent implements OnInit {
  sedes = signal<Sede[]>([]);
  isLoading = signal(false);
  showForm = signal(false);
  editingSede = signal<Sede | null>(null);

  // Form data
  nombre = signal('');
  direccion = signal('');
  ciudad = signal('');
  pais = signal('Perú');
  telefono = signal('');
  estado = signal(true);
  logoUrl = signal('');
  googleMapsUrl = signal('');
  horarios = signal<{ [key: string]: { abre: string; cierra: string } }>({
    lunes: { abre: '08:00', cierra: '18:00' },
    martes: { abre: '08:00', cierra: '18:00' },
    miercoles: { abre: '08:00', cierra: '18:00' },
    jueves: { abre: '08:00', cierra: '18:00' },
    viernes: { abre: '08:00', cierra: '18:00' },
    sabado: { abre: '08:00', cierra: '18:00' },
    domingo: { abre: '09:00', cierra: '14:00' }
  });

  diasSemana = ['lunes', 'martes', 'miercoles', 'jueves', 'viernes', 'sabado', 'domingo'];

  constructor(
    private sedesService: SedesService,
    private toastService: ToastService,
    private filesService: FilesService
  ) {}

  ngOnInit(): void {
    this.loadSedes();
  }

  loadSedes(): void {
    this.isLoading.set(true);
    this.sedesService.getAll().subscribe({
      next: (sedes) => {
        // Asegurar que siempre sea un array, incluso si viene null o undefined
        this.sedes.set(Array.isArray(sedes) ? sedes : []);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading sedes:', error);
        
        // Manejar diferentes tipos de errores
        let errorMessage = 'Error al cargar sedes';
        if (error?.status === 400) {
          errorMessage = error?.error?.message || 'Solicitud inválida al cargar sedes';
        } else if (error?.status === 500) {
          errorMessage = error?.error?.message || 'Error del servidor al cargar sedes';
        } else if (error?.status === 0) {
          errorMessage = 'No se pudo conectar con el servidor';
        }
        
        this.toastService.error(errorMessage);
        // Establecer lista vacía en caso de error para evitar que el componente se rompa
        this.sedes.set([]);
        this.isLoading.set(false);
      }
    });
  }

  openForm(sede?: Sede): void {
    if (sede) {
      this.editingSede.set(sede);
      this.nombre.set(sede.nombre);
      this.direccion.set(sede.direccion);
      this.ciudad.set(sede.ciudad);
      this.pais.set(sede.pais);
      this.telefono.set(sede.telefono || '');
      this.estado.set(sede.estado);
      this.logoUrl.set(sede.logoUrl || '');
      this.googleMapsUrl.set(sede.googleMapsUrl || '');
      this.horarios.set(sede.horarios || this.horarios());
    } else {
      this.resetForm();
    }
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingSede.set(null);
    this.resetForm();
  }

  resetForm(): void {
    this.nombre.set('');
    this.direccion.set('');
    this.ciudad.set('');
    this.pais.set('Perú');
    this.telefono.set('');
    this.estado.set(true);
    this.logoUrl.set('');
    this.googleMapsUrl.set('');
    this.horarios.set({
      lunes: { abre: '08:00', cierra: '18:00' },
      martes: { abre: '08:00', cierra: '18:00' },
      miercoles: { abre: '08:00', cierra: '18:00' },
      jueves: { abre: '08:00', cierra: '18:00' },
      viernes: { abre: '08:00', cierra: '18:00' },
      sabado: { abre: '08:00', cierra: '18:00' },
      domingo: { abre: '09:00', cierra: '14:00' }
    });
  }

  saveSede(): void {
    if (!this.nombre().trim() || !this.direccion().trim() || !this.ciudad().trim()) {
      this.toastService.error('Complete todos los campos requeridos');
      return;
    }

    // Validar URL de Google Maps si está presente
    const googleMapsUrl = this.googleMapsUrl().trim();
    if (googleMapsUrl) {
      try {
        const url = new URL(googleMapsUrl);
        // Validar que sea una URL de Google Maps
        const isValidGoogleMapsUrl = url.hostname.includes('google.com') || 
                                     url.hostname.includes('maps.app.goo.gl') ||
                                     url.hostname.includes('goo.gl') ||
                                     url.hostname.includes('maps.google.com');
        
        if (!isValidGoogleMapsUrl) {
          this.toastService.error('Por favor, ingrese una URL válida de Google Maps');
          return;
        }
      } catch (error) {
        this.toastService.error('La URL de Google Maps no es válida');
        return;
      }
    }

    const sedeData: CreateSede | UpdateSede = {
      nombre: this.nombre(),
      direccion: this.direccion(),
      ciudad: this.ciudad(),
      pais: this.pais(),
      latitud: 0, // Valor por defecto
      longitud: 0, // Valor por defecto
      telefono: this.telefono() || undefined,
      horarios: this.horarios(),
      logoUrl: this.logoUrl() || undefined,
      estado: this.estado(),
      googleMapsUrl: googleMapsUrl || undefined
    };

    const sede = this.editingSede();
    if (sede) {
      this.sedesService.update(sede.id, sedeData).subscribe({
        next: () => {
          this.toastService.success('Sede actualizada correctamente');
          this.loadSedes();
          this.closeForm();
        },
        error: (error) => {
          console.error('Error updating sede:', error);
          this.toastService.error('Error al actualizar sede');
        }
      });
    } else {
      this.sedesService.create(sedeData).subscribe({
        next: () => {
          this.toastService.success('Sede creada correctamente');
          this.loadSedes();
          this.closeForm();
        },
        error: (error) => {
          console.error('Error creating sede:', error);
          this.toastService.error('Error al crear sede');
        }
      });
    }
  }

  deleteSede(sede: Sede): void {
    if (confirm(`¿Está seguro de eliminar la sede "${sede.nombre}"?`)) {
      this.sedesService.delete(sede.id).subscribe({
        next: () => {
          this.toastService.success('Sede eliminada correctamente');
          this.loadSedes();
        },
        error: (error) => {
          console.error('Error deleting sede:', error);
          this.toastService.error('Error al eliminar sede');
        }
      });
    }
  }

  onLogoUpload(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    // Validar tipo de archivo
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      this.toastService.error('Solo se permiten archivos de imagen (JPG, PNG, WEBP)');
      return;
    }

    // Validar tamaño (10MB máximo - debe coincidir con el backend)
    const maxSize = 10 * 1024 * 1024; // 10MB
    if (file.size > maxSize) {
      this.toastService.error('El archivo excede el tamaño máximo de 10MB');
      return;
    }

    this.filesService.uploadFile(file, 'sedes').subscribe({
      next: (response) => {
        this.logoUrl.set(response.url);
        this.toastService.success('Logo subido exitosamente');
      },
      error: (error) => {
        console.error('Error uploading logo:', error);
        const errorMessage = error.error?.error || error.error?.message || 'Error al subir logo';
        this.toastService.error(errorMessage);
      }
    });
  }

  updateHorario(dia: string, campo: 'abre' | 'cierra', valor: string): void {
    const horarios = { ...this.horarios() };
    if (!horarios[dia]) {
      horarios[dia] = { abre: '08:00', cierra: '18:00' };
    }
    horarios[dia][campo] = valor;
    this.horarios.set(horarios);
  }

  getHorarioAbre(dia: string): string {
    return this.horarios()[dia]?.abre || '08:00';
  }

  getHorarioCierra(dia: string): string {
    return this.horarios()[dia]?.cierra || '18:00';
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement | null;
    if (img) {
      console.error('Error al cargar imagen de sede. URL:', img.src);
      // No ocultar la imagen, solo loguear el error
    }
  }

  onImageLoad(event: Event): void {
    // Imagen cargada correctamente
    const img = event.target as HTMLImageElement | null;
    if (img) {
      console.log('Imagen de sede cargada correctamente:', img.src);
    }
  }
}


import { Component, input, output, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpEventType } from '@angular/common/http';
import { ToastService } from '../../services/toast.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-image-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-upload.component.html',
  styleUrl: './image-upload.component.css'
})
export class ImageUploadComponent implements OnInit {
  currentImageUrl = input<string>('');
  folder = input<string>('products');
  
  imageUrl = output<string>();
  
  previewUrl = signal<string | null>(null);
  tempPreviewUrl = signal<string | null>(null); // Preview temporal (data URL)
  isUploading = signal(false);
  uploadProgress = signal(0);
  uploadedUrl = signal<string | null>(null); // URL final del servidor

  constructor(
    private http: HttpClient,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    const currentUrl = this.currentImageUrl();
    if (currentUrl) {
      this.previewUrl.set(currentUrl);
      this.uploadedUrl.set(currentUrl);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.processFile(input.files[0]);
      // Limpiar el input para permitir seleccionar el mismo archivo nuevamente
      input.value = '';
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'copy';
    }
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    
    if (event.dataTransfer && event.dataTransfer.files && event.dataTransfer.files.length > 0) {
      this.processFile(event.dataTransfer.files[0]);
    }
  }

  private processFile(file: File): void {
    // Validar tamaño (5MB máximo)
    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      this.toastService.error('El archivo excede el tamaño máximo de 5MB');
      return;
    }

    // Validar tipo
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      this.toastService.error('Tipo de archivo no permitido. Solo se permiten: JPG, PNG, WEBP');
      return;
    }

    // Previsualización temporal (data URL) - solo para mostrar mientras se sube
    const reader = new FileReader();
    reader.onload = (e) => {
      this.tempPreviewUrl.set(e.target?.result as string);
      // Mostrar preview temporal mientras se sube
      this.previewUrl.set(e.target?.result as string);
    };
    reader.readAsDataURL(file);

    // Upload
    this.uploadFile(file);
  }

  uploadFile(file: File): void {
    this.isUploading.set(true);
    this.uploadProgress.set(0);

    const formData = new FormData();
    formData.append('file', file);
    // No agregar 'folder' al FormData, solo va en el query string

    this.http.post<{ filePath: string; fileUrl: string }>(
      `${environment.apiUrl}/files/upload?folder=${this.folder()}`,
      formData,
      {
        reportProgress: true,
        observe: 'events'
        // HttpClient establece automáticamente Content-Type: multipart/form-data con el boundary correcto
      }
    ).subscribe({
      next: (event) => {
        if (event.type === HttpEventType.UploadProgress) {
          const progress = Math.round((100 * event.loaded) / (event.total || 1));
          this.uploadProgress.set(progress);
        } else if (event.type === HttpEventType.Response) {
          this.isUploading.set(false);
          this.uploadProgress.set(100);
          const response = event.body;
          if (response && response.fileUrl) {
            // Reemplazar preview temporal con URL del servidor
            this.uploadedUrl.set(response.fileUrl);
            this.previewUrl.set(response.fileUrl);
            this.tempPreviewUrl.set(null); // Limpiar preview temporal
            this.imageUrl.emit(response.fileUrl);
            this.toastService.success('Imagen subida exitosamente');
          }
        }
      },
      error: (error) => {
        this.isUploading.set(false);
        this.uploadProgress.set(0);
        // Si falla la subida, limpiar preview temporal
        this.tempPreviewUrl.set(null);
        // Si había una URL anterior, restaurarla
        if (this.uploadedUrl()) {
          this.previewUrl.set(this.uploadedUrl()!);
        } else {
          this.previewUrl.set(null);
        }
        console.error('Error uploading file:', error);
        
        // Mensaje de error más detallado
        let errorMessage = 'Error al subir la imagen';
        if (error.error?.error) {
          errorMessage = error.error.error;
        } else if (error.error?.message) {
          errorMessage = error.error.message;
        } else if (error.error?.errors && Array.isArray(error.error.errors) && error.error.errors.length > 0) {
          errorMessage = error.error.errors[0];
        } else if (error.message) {
          errorMessage = error.message;
        }
        
        // Si es un error de conexión, sugerir verificar el backend
        if (error.status === 0 || error.status === undefined) {
          errorMessage = 'Error de conexión. Verifica que el backend esté ejecutándose.';
        }
        
        // Si es un error 401, el usuario no está autenticado
        if (error.status === 401) {
          errorMessage = 'No estás autenticado. Por favor, inicia sesión.';
        }
        
        // Si es un error 403, el usuario no tiene permisos
        if (error.status === 403) {
          errorMessage = 'No tienes permisos para subir imágenes.';
        }
        
        this.toastService.error(errorMessage);
      }
    });
  }

  removeImage(): void {
    this.previewUrl.set(null);
    this.tempPreviewUrl.set(null);
    this.uploadedUrl.set(null);
    this.imageUrl.emit('');
    
    // Limpiar input
    const input = document.getElementById('file-input') as HTMLInputElement;
    if (input) {
      input.value = '';
    }
  }
}


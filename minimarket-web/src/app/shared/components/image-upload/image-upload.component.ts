import { Component, input, output, signal } from '@angular/core';
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
export class ImageUploadComponent {
  currentImageUrl = input<string>('');
  folder = input<string>('products');
  
  imageUrl = output<string>();
  
  previewUrl = signal<string | null>(null);
  isUploading = signal(false);
  uploadProgress = signal(0);

  constructor(
    private http: HttpClient,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    const currentUrl = this.currentImageUrl();
    if (currentUrl) {
      this.previewUrl.set(currentUrl);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
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

      // Previsualización
      const reader = new FileReader();
      reader.onload = (e) => {
        this.previewUrl.set(e.target?.result as string);
      };
      reader.readAsDataURL(file);

      // Upload
      this.uploadFile(file);
    }
  }

  uploadFile(file: File): void {
    this.isUploading.set(true);
    this.uploadProgress.set(0);

    const formData = new FormData();
    formData.append('file', file);
    formData.append('folder', this.folder());

    this.http.post<{ filePath: string; fileUrl: string }>(
      `${environment.apiUrl}/files/upload?folder=${this.folder()}`,
      formData,
      {
        reportProgress: true,
        observe: 'events'
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
          if (response) {
            this.previewUrl.set(response.fileUrl);
            this.imageUrl.emit(response.fileUrl);
            this.toastService.success('Imagen subida exitosamente');
          }
        }
      },
      error: (error) => {
        this.isUploading.set(false);
        this.uploadProgress.set(0);
        console.error('Error uploading file:', error);
        this.toastService.error(error.error?.error || 'Error al subir la imagen');
      }
    });
  }

  removeImage(): void {
    this.previewUrl.set(null);
    this.imageUrl.emit('');
    
    // Limpiar input
    const input = document.getElementById('file-input') as HTMLInputElement;
    if (input) {
      input.value = '';
    }
  }
}


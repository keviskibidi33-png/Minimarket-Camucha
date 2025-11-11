import { Component, input, output, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpEventType } from '@angular/common/http';
import { ToastService } from '../../services/toast.service';
import { environment } from '../../../../environments/environment';

// Emojis predefinidos para logos
const PREDEFINED_EMOJIS = [
  '🏪', '🏬', '🏮', '🛒', '🛍️', '🛎️', '🏨', '🏢', '🏠', '🏡',
  '🍕', '🍔', '🍟', '🌮', '🌯', '🥗', '🍱', '🍜', '🍝', '🍲',
  '🥤', '☕', '🍵', '🍶', '🍷', '🍸', '🍹', '🧃', '🧉', '🧊',
  '🍎', '🍊', '🍋', '🍌', '🍉', '🍇', '🍓', '🍈', '🍒', '🍑',
  '🥭', '🍍', '🥥', '🥝', '🍅', '🥑', '🥦', '🥬', '🥒', '🌶️',
  '🥕', '🌽', '🥔', '🍠', '🥐', '🥯', '🍞', '🥖', '🥨', '🧀',
  '🥚', '🍳', '🥞', '🥓', '🥩', '🍗', '🍖', '🌭', '🍔', '🍟',
  '🍕', '🌮', '🌯', '🥙', '🥪', '🌭', '🍿', '🧂', '🥫', '🍝',
  '💼', '📦', '📊', '💰', '💳', '🛒', '🛍️', '🎁', '🎀', '🎂'
];

@Component({
  selector: 'app-logo-selector',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './logo-selector.component.html',
  styleUrl: './logo-selector.component.css'
})
export class LogoSelectorComponent implements OnInit {
  currentLogoUrl = input<string>('');
  currentEmoji = input<string>('');
  
  logoUrl = output<string>();
  emoji = output<string>();
  
  selectedType = signal<'upload' | 'emoji'>('upload');
  previewUrl = signal<string | null>(null);
  tempPreviewUrl = signal<string | null>(null);
  uploadedUrl = signal<string | null>(null);
  selectedEmoji = signal<string>('');
  isUploading = signal(false);
  uploadProgress = signal(0);
  showEmojiPicker = signal(false);
  
  emojis = PREDEFINED_EMOJIS;

  constructor(
    private http: HttpClient,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    const currentUrl = this.currentLogoUrl();
    const currentEmoji = this.currentEmoji();
    
    if (currentEmoji) {
      this.selectedType.set('emoji');
      this.selectedEmoji.set(currentEmoji);
      this.emoji.emit(currentEmoji);
    } else if (currentUrl) {
      this.selectedType.set('upload');
      this.previewUrl.set(currentUrl);
      this.uploadedUrl.set(currentUrl);
      this.logoUrl.emit(currentUrl);
    }
  }

  onTypeChange(type: 'upload' | 'emoji'): void {
    this.selectedType.set(type);
    if (type === 'emoji') {
      // Limpiar imagen si se cambia a emoji
      this.previewUrl.set(null);
      this.uploadedUrl.set(null);
      this.tempPreviewUrl.set(null);
      this.logoUrl.emit('');
    } else {
      // Limpiar emoji si se cambia a upload
      this.selectedEmoji.set('');
      this.emoji.emit('');
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validar tamaño (5MB máximo)
      const maxSize = 5 * 1024 * 1024;
      if (file.size > maxSize) {
        this.toastService.error('El archivo excede el tamaño máximo de 5MB');
        return;
      }

      // Validar tipo
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/svg+xml'];
      if (!allowedTypes.includes(file.type)) {
        this.toastService.error('Tipo de archivo no permitido. Solo se permiten: JPG, PNG, WEBP, SVG');
        return;
      }

      // Previsualización temporal
      const reader = new FileReader();
      reader.onload = (e) => {
        this.tempPreviewUrl.set(e.target?.result as string);
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

    this.http.post<{ filePath: string; fileUrl: string }>(
      `${environment.apiUrl}/files/upload?folder=branding`,
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
          if (response && response.fileUrl) {
            this.uploadedUrl.set(response.fileUrl);
            this.previewUrl.set(response.fileUrl);
            this.tempPreviewUrl.set(null);
            this.logoUrl.emit(response.fileUrl);
            this.toastService.success('Logo subido exitosamente');
          }
        }
      },
      error: (error) => {
        this.isUploading.set(false);
        this.uploadProgress.set(0);
        this.tempPreviewUrl.set(null);
        if (this.uploadedUrl()) {
          this.previewUrl.set(this.uploadedUrl()!);
        } else {
          this.previewUrl.set(null);
        }
        console.error('Error uploading file:', error);
        const errorMessage = error.error?.error || error.error?.message || 'Error al subir la imagen';
        this.toastService.error(errorMessage);
      }
    });
  }

  selectEmoji(emoji: string): void {
    this.selectedEmoji.set(emoji);
    this.emoji.emit(emoji);
    this.showEmojiPicker.set(false);
    // Limpiar imagen cuando se selecciona emoji
    this.previewUrl.set(null);
    this.uploadedUrl.set(null);
    this.tempPreviewUrl.set(null);
    this.logoUrl.emit('');
  }

  removeLogo(): void {
    this.previewUrl.set(null);
    this.tempPreviewUrl.set(null);
    this.uploadedUrl.set(null);
    this.selectedEmoji.set('');
    this.logoUrl.emit('');
    this.emoji.emit('');
    
    // Limpiar input
    const input = document.getElementById('logo-file-input') as HTMLInputElement;
    if (input) {
      input.value = '';
    }
  }

  toggleEmojiPicker(): void {
    this.showEmojiPicker.set(!this.showEmojiPicker());
  }
}


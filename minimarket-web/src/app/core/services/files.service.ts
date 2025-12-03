import { Injectable } from '@angular/core';
import { HttpClient, HttpEventType } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface FileUploadResponse {
  filePath: string;
  fileUrl: string;
}

@Injectable({
  providedIn: 'root'
})
export class FilesService {
  private readonly apiUrl = `${environment.apiUrl}/files`;

  constructor(private http: HttpClient) {}

  uploadFile(file: File, folder: string = 'general'): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    // NO agregar 'folder' al FormData, solo va en el query string

    return this.http.post<FileUploadResponse>(
      `${this.apiUrl}/upload?folder=${folder}`,
      formData
    ).pipe(
      map(response => {
        // El backend puede devolver 'fileUrl' o 'url' como alias
        const url = response.fileUrl || (response as any).url || '';
        console.log('Archivo subido exitosamente. URL:', url);
        return { url };
      })
    );
  }
}


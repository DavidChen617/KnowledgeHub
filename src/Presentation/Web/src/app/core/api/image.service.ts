import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, UploadedImage } from './api.types';

@Injectable({ providedIn: 'root' })
export class ImageService {
  private readonly http = inject(HttpClient);

  async upload(files: File[]): Promise<UploadedImage[]> {
    const form = new FormData();
    files.forEach(f => form.append('files', f));
    const res = await firstValueFrom(
      this.http.post<ApiResponse<UploadedImage[]>>(`${environment.apiUrl}/api/images`, form)
    );
    return res.data ?? [];
  }
}

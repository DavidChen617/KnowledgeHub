import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './api.types';
import { CurrentUser } from '../auth/auth.service';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);

  async me(): Promise<CurrentUser> {
    const res = await firstValueFrom(
      this.http.get<ApiResponse<CurrentUser>>(`${environment.apiUrl}/api/users/me`)
    );
    return res.data!;
  }

  async updateAvatar(file: File): Promise<string> {
    const form = new FormData();
    form.append('file', file);
    const res = await firstValueFrom(
      this.http.patch<ApiResponse<{ avatarUrl: string }>>(`${environment.apiUrl}/api/users/me/avatar`, form)
    );
    return res.data!.avatarUrl;
  }
}

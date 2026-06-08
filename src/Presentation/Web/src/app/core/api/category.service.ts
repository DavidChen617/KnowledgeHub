import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, Category } from './api.types';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/categories`;

  async list(): Promise<Category[]> {
    const res = await firstValueFrom(this.http.get<ApiResponse<{ categories: Category[] }>>(this.base));
    return res.data?.categories ?? [];
  }

  async create(name: string): Promise<Category> {
    const res = await firstValueFrom(this.http.post<ApiResponse<Category>>(this.base, { name }));
    return res.data!;
  }

  async update(id: string, name: string): Promise<void> {
    await firstValueFrom(this.http.put(`${this.base}/${id}`, { name }));
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete(`${this.base}/${id}`));
  }
}

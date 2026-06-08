import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, Note, NoteSummary, NoteGraph, SearchResult, StructureResult, NoteStructureSummary } from './api.types';

@Injectable({ providedIn: 'root' })
export class NoteService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/notes`;

  async list(): Promise<NoteSummary[]> {
    const res = await firstValueFrom(this.http.get<ApiResponse<{ notes: NoteSummary[] }>>(this.base));
    return res.data?.notes ?? [];
  }

  async get(id: string): Promise<Note> {
    const res = await firstValueFrom(this.http.get<ApiResponse<Note>>(`${this.base}/${id}`));
    return res.data!;
  }

  async create(title = '新筆記'): Promise<{ noteId: string }> {
    const res = await firstValueFrom(
      this.http.post<ApiResponse<{ noteId: string }>>(this.base, { title, content: '' })
    );
    return res.data!;
  }

  async update(id: string, data: Partial<Pick<Note, 'title' | 'content' | 'categoryId' | 'linkedNoteIds'>>): Promise<void> {
    await firstValueFrom(this.http.put(`${this.base}/${id}`, data));
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete(`${this.base}/${id}`));
  }

  async structure(id: string, prompt = ''): Promise<StructureResult> {
    const res = await firstValueFrom(
      this.http.post<ApiResponse<StructureResult>>(`${this.base}/${id}/structure`, { prompt })
    );
    return res.data!;
  }

  async deleteShareLink(id: string): Promise<void> {
    await firstValueFrom(this.http.delete(`${this.base}/${id}/share`));
  }

  async createShareLink(id: string): Promise<string> {
    const res = await firstValueFrom(
      this.http.post<ApiResponse<{ url: string }>>(`${this.base}/${id}/share`, {})
    );
    const { pathname } = new URL(res.data!.url);
    return `${window.location.origin}${pathname}`;
  }

  async listStructures(id: string): Promise<NoteStructureSummary[]> {
    const res = await firstValueFrom(
      this.http.get<ApiResponse<{ structures: NoteStructureSummary[] }>>(`${this.base}/${id}/structures`)
    );
    return res.data?.structures ?? [];
  }

  async graph(): Promise<NoteGraph> {
    const res = await firstValueFrom(this.http.get<ApiResponse<NoteGraph>>(`${this.base}/graph`));
    return res.data!;
  }

  async search(query: string): Promise<SearchResult[]> {
    const res = await firstValueFrom(
      this.http.get<ApiResponse<{ results: SearchResult[] }>>(`${this.base}/search`, { params: { q: query } })
    );
    return res.data?.results ?? [];
  }
}

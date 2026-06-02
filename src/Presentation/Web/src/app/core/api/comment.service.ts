import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, Comment } from './api.types';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}`;

  async listForNote(noteId: string): Promise<Comment[]> {
    const res = await firstValueFrom(
      this.http.get<ApiResponse<{ comments: Comment[] }>>(`${this.base}/api/notes/${noteId}/comments`)
    );
    return res.data?.comments ?? [];
  }

  async listForShare(token: string): Promise<Comment[]> {
    const res = await firstValueFrom(
      this.http.get<ApiResponse<{ comments: Comment[] }>>(`${this.base}/share/${token}/comments`)
    );
    return res.data?.comments ?? [];
  }

  async addToNote(noteId: string, content: string, parentCommentId?: string): Promise<void> {
    await firstValueFrom(
      this.http.post(`${this.base}/api/notes/${noteId}/comments`, { content, parentCommentId })
    );
  }

  async addToShare(token: string, content: string, parentCommentId?: string): Promise<void> {
    await firstValueFrom(
      this.http.post(`${this.base}/share/${token}/comments`, { content, parentCommentId })
    );
  }
}

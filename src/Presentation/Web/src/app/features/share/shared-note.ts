import { Component, inject, signal, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { CommentService } from '../../core/api/comment.service';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../shared/services/toast.service';
import { ButtonComponent } from '../../shared/components/button';
import { SpinnerComponent } from '../../shared/components/spinner';
import { Comment } from '../../core/api/api.types';
import { renderMarkdown } from '../../shared/utils/markdown';
import { environment } from '../../../environments/environment';

interface SharedNote {
  noteId: string;
  title: string;
  content: string;
  authorName: string;
  authorEmail: string;
  authorAvatarUrl?: string;
}

@Component({
  selector: 'shared-note',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, ButtonComponent, SpinnerComponent],
  template: `
    <div class="min-h-screen bg-[var(--color-bg)]">

      <!-- Top bar -->
      <header class="border-b border-[var(--color-border)] px-6 h-12 flex items-center justify-between">
        <span class="font-display text-xs font-bold tracking-widest text-[var(--color-accent)] uppercase">
          KnowledgeHub
        </span>
        @if (!auth.isLoggedIn()) {
          <a routerLink="/login" class="font-mono text-xs text-[var(--color-muted)] hover:text-[var(--color-text)] transition-colors">
            登入
          </a>
        }
      </header>

      <main class="max-w-2xl mx-auto px-6 py-12">

        @if (loading()) {
          <div class="flex justify-center py-24"><spinner size="lg" /></div>
        } @else if (notFound()) {
          <div class="text-center py-24">
            <div class="font-display text-6xl text-[var(--color-border)] mb-4">404</div>
            <p class="font-mono text-sm text-[var(--color-muted)] mb-6">此分享連結不存在或已失效</p>
            <a routerLink="/" class="font-mono text-xs text-[var(--color-accent)] hover:underline">← 回到首頁</a>
          </div>
        } @else {
          <!-- Note header -->
          <div class="mb-8 page-enter">
            <h1 class="font-display text-3xl font-bold text-[var(--color-text)] mb-4">
              {{ note()?.title || '（無標題）' }}
            </h1>
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-full shrink-0 overflow-hidden">
                @if (note()?.authorAvatarUrl) {
                  <img [src]="note()!.authorAvatarUrl" [alt]="note()!.authorName" class="w-full h-full object-cover" />
                } @else {
                  <div class="w-full h-full bg-[var(--color-accent)] flex items-center justify-center text-white text-sm font-semibold">
                    {{ (note()?.authorName ?? '')[0]?.toUpperCase() }}
                  </div>
                }
              </div>
              <div>
                <div class="font-mono text-sm text-[var(--color-text)] font-semibold">{{ note()?.authorName }}</div>
                <div class="font-mono text-xs text-[var(--color-muted)]">{{ note()?.authorEmail }}</div>
              </div>
            </div>
          </div>

          <!-- Content -->
          <article
            class="prose prose-invert prose-sm max-w-none font-mono mb-16 page-enter"
            [innerHTML]="contentHtml()"
          ></article>

          <!-- Divider -->
          <div class="border-t border-[var(--color-border)] mb-8"></div>

          <!-- Comments -->
          <section>
            <h2 class="font-mono text-xs text-[var(--color-muted)] uppercase tracking-widest mb-6">留言</h2>

            @if (auth.isLoggedIn()) {
              <div class="mb-6 flex flex-col gap-2">
                <textarea
                  [(ngModel)]="newComment"
                  rows="3"
                  placeholder="輸入留言…"
                  class="w-full bg-[var(--color-surface)] border border-[var(--color-border)] text-[var(--color-text)] font-mono text-sm px-3 py-2 outline-none resize-none focus:border-[var(--color-accent)] placeholder:text-[var(--color-muted)]"
                ></textarea>
                <div class="flex justify-end">
                  <app-button size="sm" (click)="addComment()" [loading]="commentSending()">送出留言</app-button>
                </div>
              </div>
            } @else {
              <div class="bg-[var(--color-surface)] border border-[var(--color-border)] p-4 mb-6 font-mono text-xs text-[var(--color-muted)] text-center">
                <a routerLink="/login" class="text-[var(--color-accent)] hover:underline">登入</a>後即可留言
              </div>
            }

            @if (comments().length === 0) {
              <p class="font-mono text-xs text-[var(--color-muted)]">尚無留言</p>
            } @else {
              <div class="flex flex-col gap-4">
                @for (c of comments(); track c.commentId) {
                  <div class="flex gap-3">
                    <div class="w-7 h-7 rounded-full shrink-0 overflow-hidden mt-0.5">
                      @if (c.avatarUrl) {
                        <img [src]="c.avatarUrl" [alt]="c.username" class="w-full h-full object-cover" />
                      } @else {
                        <div class="w-full h-full bg-[var(--color-accent)] flex items-center justify-center text-white text-xs font-semibold">
                          {{ c.username.slice(0,1).toUpperCase() }}
                        </div>
                      }
                    </div>
                    <div class="flex-1 min-w-0">
                      <div class="flex items-baseline gap-2 mb-1">
                        <span class="font-mono text-xs text-[var(--color-accent)] font-semibold">{{ c.username }}</span>
                        <span class="font-mono text-[10px] text-[var(--color-muted)]">{{ formatDate(c.createdAt) }}</span>
                      </div>
                      <div class="font-mono text-sm text-[var(--color-text)] leading-relaxed">{{ c.content }}</div>
                    </div>
                  </div>
                }
              </div>
            }
          </section>
        }

      </main>
    </div>
  `,
})
export class SharedNoteComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly http = inject(HttpClient);
  private readonly commentService = inject(CommentService);
  private readonly toast = inject(ToastService);

  readonly note = signal<SharedNote | null>(null);
  readonly contentHtml = signal('');
  readonly comments = signal<Comment[]>([]);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly commentSending = signal(false);
  newComment = '';

  async ngOnInit() {
    const token = this.route.snapshot.paramMap.get('token')!;
    try {
      const res = await firstValueFrom(
        this.http.get<{ data: SharedNote; isSuccess: boolean }>(`${environment.apiUrl}/api/share/${token}`)
      );
      if (!res.isSuccess || !res.data) { this.notFound.set(true); return; }
      this.note.set(res.data);
      this.contentHtml.set(await renderMarkdown(res.data.content));
      this.comments.set(await this.commentService.listForShare(token));
    } catch {
      this.notFound.set(true);
    } finally {
      this.loading.set(false);
    }
  }

  async addComment() {
    if (!this.newComment.trim()) return;
    const token = this.route.snapshot.paramMap.get('token')!;
    this.commentSending.set(true);
    try {
      await this.commentService.addToShare(token, this.newComment);
      this.comments.set(await this.commentService.listForShare(token));
      this.newComment = '';
    } catch {
      this.toast.error('留言失敗');
    } finally {
      this.commentSending.set(false);
    }
  }

  formatDate(iso: string): string {
    const d = new Date(iso);
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    if (diffMins < 1) return '剛剛';
    if (diffMins < 60) return `${diffMins} 分鐘前`;
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} 小時前`;
    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays} 天前`;
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
}

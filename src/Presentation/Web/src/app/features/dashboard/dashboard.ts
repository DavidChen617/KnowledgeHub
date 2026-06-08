import { Component, inject, signal, computed, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { NoteService } from '../../core/api/note.service';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../shared/services/toast.service';
import { ButtonComponent } from '../../shared/components/button';
import { SpinnerComponent } from '../../shared/components/spinner';
import { NoteSummary } from '../../core/api/api.types';

@Component({
  selector: 'dashboard',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, ButtonComponent, SpinnerComponent],
  template: `
    <div class="min-h-screen bg-[var(--color-bg)]">

      <!-- Top bar -->
      <header class="border-b border-[var(--color-border)] px-6 h-12 flex items-center justify-between">
        <span class="font-display text-xs font-bold tracking-widest text-[var(--color-accent)] uppercase">
          KnowledgeHub
        </span>
        <div class="flex items-center gap-4">
          <a routerLink="/notes" class="font-mono text-xs text-[var(--color-muted)] hover:text-[var(--color-text)] transition-colors">筆記</a>
          <a routerLink="/graph" class="font-mono text-xs text-[var(--color-muted)] hover:text-[var(--color-text)] transition-colors">圖譜</a>
          <button
            (click)="auth.logout()"
            class="font-mono text-xs text-[var(--color-muted)] hover:text-[var(--color-text)] transition-colors"
          >
            登出
          </button>
        </div>
      </header>

      <main class="max-w-3xl mx-auto px-6 py-12">

        <!-- Welcome -->
        <div class="mb-10 page-enter">
          <p class="font-mono text-xs text-[var(--color-muted)] mb-1">歡迎回來</p>
          <h1 class="font-display text-3xl font-bold text-[var(--color-text)]">
            {{ auth.currentUser()?.username ?? '...' }}
          </h1>
        </div>

        <!-- Stats -->
        <div class="grid grid-cols-2 gap-px bg-[var(--color-border)] mb-12 page-enter">
          <div class="bg-[var(--color-surface)] p-6">
            <div class="font-display text-4xl font-extrabold text-[var(--color-accent)] mb-1">
              {{ notes().length }}
            </div>
            <div class="font-mono text-xs text-[var(--color-muted)] uppercase tracking-widest">筆記總數</div>
          </div>
          <div class="bg-[var(--color-surface)] p-6 flex items-center justify-end">
            <app-button (click)="createNote()" [loading]="creating()">
              + 新增筆記
            </app-button>
          </div>
        </div>

        <!-- Recent notes -->
        <section>
          <h2 class="font-mono text-xs text-[var(--color-muted)] uppercase tracking-widest mb-4">最近編輯</h2>

          @if (loading()) {
            <div class="flex justify-center py-12">
              <spinner />
            </div>
          } @else if (recentNotes().length === 0) {
            <div class="border border-dashed border-[var(--color-border)] p-12 text-center">
              <p class="font-mono text-xs text-[var(--color-muted)] mb-4">還沒有筆記</p>
              <app-button size="sm" (click)="createNote()">建立第一篇</app-button>
            </div>
          } @else {
            <div class="flex flex-col gap-px bg-[var(--color-border)] page-enter-stagger">
              @for (note of recentNotes(); track note.noteId) {
                <a
                  [routerLink]="['/notes', note.noteId]"
                  class="bg-[var(--color-surface)] px-5 py-4 flex items-center justify-between group hover:bg-[var(--color-surface-2)] transition-colors duration-150"
                >
                  <span class="font-display text-sm text-[var(--color-text)] group-hover:text-white transition-colors">
                    {{ note.title || '（無標題）' }}
                  </span>
                  <span class="font-mono text-xs text-[var(--color-muted)]">
                    {{ formatDate(note.updatedAt) }}
                  </span>
                </a>
              }
            </div>
          }
        </section>

      </main>
    </div>
  `,
})
export class DashboardComponent implements OnInit {
  readonly auth = inject(AuthService);
  private readonly noteService = inject(NoteService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly notes = signal<NoteSummary[]>([]);
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly recentNotes = computed(() => this.notes().slice(0, 10));

  async ngOnInit() {
    try {
      this.notes.set(await this.noteService.list());
    } finally {
      this.loading.set(false);
    }
  }

  async createNote() {
    this.creating.set(true);
    try {
      const { noteId } = await this.noteService.create();
      this.router.navigate(['/notes', noteId]);
    } catch {
      this.toast.error('建立失敗，請再試一次');
      this.creating.set(false);
    }
  }

  formatDate(iso: string): string {
    const d = new Date(iso);
    return `${d.getMonth() + 1}/${d.getDate()}`;
  }
}

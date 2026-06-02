import {
  Component, inject, signal, OnInit, OnDestroy, ChangeDetectionStrategy,
  ElementRef, ViewChild, DestroyRef,
} from '@angular/core';
import { Router } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { Subject, debounceTime, takeUntil, switchMap, from, of, catchError } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { toObservable } from '@angular/core/rxjs-interop';
import { NoteService } from '../../core/api/note.service';
import { SearchResult } from '../../core/api/api.types';
import { SpinnerComponent } from '../../shared/components/spinner';
import { renderMarkdown } from '../../shared/utils/markdown';

@Component({
  selector: 'search-overlay',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SpinnerComponent],
  template: `
    @if (open()) {
      <div
        class="fixed inset-0 z-50 flex items-start justify-center pt-20 px-4 overlay-enter"
        role="dialog"
        aria-modal="true"
        aria-label="搜尋"
      >
        <!-- Backdrop -->
        <div class="absolute inset-0 bg-black/70" (click)="close()"></div>

        <!-- Panel -->
        <div
          class="relative bg-[var(--color-surface)] border border-[var(--color-border)] flex overflow-hidden panel-enter"
          [class]="results().length > 0 ? 'w-full max-w-3xl' : 'w-full max-w-xl'"
          style="max-height: 70vh"
        >

          <!-- Left column: input + results -->
          <div
            class="flex flex-col"
            [class]="results().length > 0 ? 'w-72 shrink-0 border-r border-[var(--color-border)]' : 'flex-1'"
          >
            <!-- Input -->
            <div class="flex items-center gap-3 px-4 py-3 border-b border-[var(--color-border)] shrink-0">
              <span class="text-[var(--color-muted)] font-mono text-sm">⌕</span>
              <input
                #searchInput
                type="text"
                placeholder="搜尋筆記…"
                [value]="query()"
                (input)="onInput($event)"
                (keydown)="onKeydown($event)"
                class="flex-1 bg-transparent font-mono text-sm text-[var(--color-text)] outline-none placeholder:text-[var(--color-muted)]"
              />
              @if (searching()) {
                <spinner size="sm" />
              }
              <kbd class="font-mono text-[10px] text-[var(--color-muted)] border border-[var(--color-border)] px-1.5 py-0.5">ESC</kbd>
            </div>

            <!-- Results list -->
            <div class="overflow-y-auto flex-1">
              @if (results().length > 0) {
                <ul role="listbox">
                  @for (r of results(); track r.noteId; let i = $index) {
                    <li
                      role="option"
                      [attr.aria-selected]="i === activeIndex()"
                      (click)="navigate(r)"
                      (mouseenter)="activeIndex.set(i)"
                      class="px-4 py-2.5 cursor-pointer font-mono transition-colors duration-100"
                      [class]="i === activeIndex() ? 'bg-[var(--color-surface-2)] text-[var(--color-accent)]' : 'text-[var(--color-text)] hover:bg-[var(--color-surface-2)]'"
                    >
                      <div class="text-sm font-semibold truncate">{{ r.title || '（無標題）' }}</div>
                      @if (r.snippet) {
                        <div class="text-[10px] text-[var(--color-muted)] mt-0.5 truncate">{{ r.snippet }}</div>
                      }
                    </li>
                  }
                </ul>
              } @else if (query() && !searching()) {
                <div class="px-4 py-6 text-center font-mono text-xs text-[var(--color-muted)]">
                  找不到符合「{{ query() }}」的筆記
                </div>
              } @else {
                <div class="px-4 py-6 text-center font-mono text-xs text-[var(--color-muted)]">
                  輸入關鍵字搜尋筆記
                </div>
              }
            </div>

            <!-- Footer -->
            <div class="px-4 py-2 border-t border-[var(--color-border)] flex items-center gap-4 text-[10px] text-[var(--color-muted)] font-mono shrink-0">
              <span>↑↓ 導航</span>
              <span>↵ 開啟</span>
            </div>
          </div>

          <!-- Right column: preview -->
          @if (results().length > 0) {
            <div class="flex-1 overflow-y-auto flex flex-col min-w-0">
              <div class="px-5 py-3 border-b border-[var(--color-border)] shrink-0">
                <h3 class="text-sm font-semibold text-[var(--color-text)] font-mono truncate">
                  {{ previewTitle() || '（無標題）' }}
                </h3>
              </div>
              <div class="flex-1 px-5 py-4 overflow-y-auto">
                @if (previewLoading()) {
                  <div class="flex justify-center py-8">
                    <spinner size="sm" />
                  </div>
                } @else if (previewHtml()) {
                  <div
                    class="prose prose-invert prose-sm max-w-none text-xs leading-relaxed"
                    [innerHTML]="previewHtml()"
                  ></div>
                } @else {
                  <p class="text-xs text-[var(--color-muted)] font-mono">（無內容）</p>
                }
              </div>
            </div>
          }

        </div>
      </div>
    }
  `,
})
export class SearchOverlayComponent implements OnInit, OnDestroy {
  @ViewChild('searchInput') searchInputRef?: ElementRef<HTMLInputElement>;

  private readonly noteService = inject(NoteService);
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly destroy$ = new Subject<void>();
  private readonly search$ = new Subject<string>();

  readonly open = signal(false);
  readonly query = signal('');
  readonly results = signal<SearchResult[]>([]);
  readonly searching = signal(false);
  readonly activeIndex = signal(0);
  readonly previewHtml = signal<SafeHtml>('');
  readonly previewTitle = signal('');
  readonly previewLoading = signal(false);

  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    toObservable(this.activeIndex).pipe(
      debounceTime(200),
      switchMap(idx => {
        const r = this.results()[idx];
        if (!r) {
          this.previewHtml.set('');
          this.previewTitle.set('');
          return of(null);
        }
        this.previewTitle.set(r.title || '（無標題）');
        this.previewLoading.set(true);
        return from(
          this.noteService.get(r.noteId)
            .then(note => renderMarkdown(note.content))
        ).pipe(catchError(() => of(null)));
      }),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(html => {
      if (html) {
        this.previewHtml.set(this.sanitizer.bypassSecurityTrustHtml(html));
      } else {
        this.previewHtml.set('');
      }
      this.previewLoading.set(false);
    });
  }

  ngOnInit(): void {
    window.addEventListener('keydown', this.handleGlobalKey);

    this.search$.pipe(
      debounceTime(300),
      takeUntil(this.destroy$),
    ).subscribe(async q => {
      if (!q.trim()) {
        this.results.set([]);
        this.previewHtml.set('');
        return;
      }
      this.searching.set(true);
      try {
        this.results.set(await this.noteService.search(q));
        this.activeIndex.set(0);
      } finally {
        this.searching.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    window.removeEventListener('keydown', this.handleGlobalKey);
    this.destroy$.next();
    this.destroy$.complete();
  }

  private readonly handleGlobalKey = (e: KeyboardEvent) => {
    if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
      e.preventDefault();
      this.toggle();
      return;
    }
    if (e.key === 'Escape' && this.open()) {
      e.preventDefault();
      this.close();
    }
  };

  toggle() {
    if (this.open()) {
      this.close();
    } else {
      this.open.set(true);
      this.query.set('');
      this.results.set([]);
      this.previewHtml.set('');
      setTimeout(() => this.searchInputRef?.nativeElement.focus(), 50);
    }
  }

  close() {
    this.open.set(false);
    this.query.set('');
    this.results.set([]);
    this.previewHtml.set('');
  }

  onInput(e: Event) {
    const q = (e.target as HTMLInputElement).value;
    this.query.set(q);
    this.search$.next(q);
  }

  onKeydown(e: KeyboardEvent) {
    const len = this.results().length;
    if (e.key === 'Escape') { this.close(); return; }
    if (e.key === 'ArrowDown') { e.preventDefault(); this.activeIndex.update(i => Math.min(i + 1, len - 1)); }
    if (e.key === 'ArrowUp') { e.preventDefault(); this.activeIndex.update(i => Math.max(i - 1, 0)); }
    if (e.key === 'Enter') {
      const r = this.results()[this.activeIndex()];
      if (r) this.navigate(r);
    }
  }

  navigate(r: SearchResult) {
    this.close();
    this.router.navigate(['/notes', r.noteId]);
  }
}

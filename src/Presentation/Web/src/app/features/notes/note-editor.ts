import {
  Component, inject, signal, effect, OnInit, ChangeDetectionStrategy, DestroyRef, ViewChild, ElementRef,
} from '@angular/core';
import { createResizable } from '../../shared/utils/resizable';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime, distinctUntilChanged, map } from 'rxjs';
import { NoteService } from '../../core/api/note.service';
import { CategoryService } from '../../core/api/category.service';
import { CommentService } from '../../core/api/comment.service';
import { ImageService } from '../../core/api/image.service';
import { ToastService } from '../../shared/services/toast.service';
import { AuthService } from '../../core/auth/auth.service';
import { ButtonComponent } from '../../shared/components/button';
import { SpinnerComponent } from '../../shared/components/spinner';
import { MdEditorComponent } from './md-editor';
import { Note, Category, Comment, NoteStructureSummary } from '../../core/api/api.types';
import { renderMarkdown } from '../../shared/utils/markdown';

type ViewMode = 'edit' | 'split' | 'preview';

@Component({
  selector: 'note-editor',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, ButtonComponent, SpinnerComponent, MdEditorComponent],
  template: `
    @if (loading()) {
      <div class="min-h-screen flex items-center justify-center bg-[var(--color-bg)]">
        <spinner size="lg" />
      </div>
    } @else if (note()) {
      <div class="h-screen bg-[var(--color-bg)] flex flex-col overflow-hidden">

        <!-- Header -->
        <header class="border-b border-[var(--color-border)] bg-[var(--color-surface)] px-4 h-11 flex items-center gap-3 shrink-0">
          <button (click)="location.back()" class="text-sm text-[var(--color-muted)] hover:text-[var(--color-text)] transition-colors shrink-0 leading-none">
            ←
          </button>
          <div class="flex-1 min-w-0">
            <input
              type="text"
              [value]="note()!.title"
              (input)="onTitleChange($event)"
              placeholder="無標題"
              class="w-full bg-transparent text-sm font-medium text-[var(--color-text)] outline-none placeholder:text-[var(--color-muted)] truncate"
            />
          </div>
          <span class="text-xs text-[var(--color-muted)] shrink-0">
            {{ saving() ? '儲存中…' : '已儲存' }}
          </span>

          <!-- Mode toggle -->
          <div class="flex items-center border border-[var(--color-border)] shrink-0 overflow-hidden" role="group" aria-label="檢視模式">
            <button
              (click)="mode.set('edit')"
              [class]="'px-2.5 py-1 text-xs border-r border-[var(--color-border)] transition-colors ' + (mode() === 'edit' ? 'text-[var(--color-accent)] bg-[var(--color-surface-2)]' : 'text-[var(--color-muted)] hover:text-[var(--color-text)]')"
              title="編輯模式"
            >編輯</button>
            <button
              (click)="mode.set('split')"
              [class]="'px-2.5 py-1 text-xs border-r border-[var(--color-border)] transition-colors ' + (mode() === 'split' ? 'text-[var(--color-accent)] bg-[var(--color-surface-2)]' : 'text-[var(--color-muted)] hover:text-[var(--color-text)]')"
              title="分割模式"
            >分割</button>
            <button
              (click)="mode.set('preview')"
              [class]="'px-2.5 py-1 text-xs transition-colors ' + (mode() === 'preview' ? 'text-[var(--color-accent)] bg-[var(--color-surface-2)]' : 'text-[var(--color-muted)] hover:text-[var(--color-text)]')"
              title="預覽模式"
            >預覽</button>
          </div>

          <div class="flex items-center gap-1">
            <input
              type="text"
              [(ngModel)]="structurePrompt"
              placeholder="prompt"
              class="h-6 px-2 text-xs bg-[var(--color-surface)] border border-[var(--color-border)] text-[var(--color-text)] placeholder:text-[var(--color-muted)] outline-none focus:border-[var(--color-accent)] w-44"
              (keydown.enter)="structure()"
            />
            <app-button variant="ghost" size="sm" (click)="structure()" [loading]="structuring()">AI 整理</app-button>
          </div>
        </header>

        <!-- Toolbar (shown in edit/split mode) -->
        @if (mode() !== 'preview') {
          <div class="border-b border-[var(--color-border)] bg-[var(--color-surface)] px-2 h-9 flex items-center gap-0.5 shrink-0 overflow-x-auto">

            <button (click)="bold()" title="粗體 (Ctrl+B)" class="toolbar-btn font-bold">B</button>
            <button (click)="italic()" title="斜體 (Ctrl+I)" class="toolbar-btn italic">I</button>
            <button (click)="strikethrough()" title="刪除線" class="toolbar-btn" style="text-decoration:line-through">S</button>

            <span class="toolbar-sep" aria-hidden="true"></span>

            <button (click)="heading(1)" title="標題 1" class="toolbar-btn">H1</button>
            <button (click)="heading(2)" title="標題 2" class="toolbar-btn">H2</button>
            <button (click)="heading(3)" title="標題 3" class="toolbar-btn">H3</button>

            <span class="toolbar-sep" aria-hidden="true"></span>

            <button (click)="insertLink()" title="連結" class="toolbar-btn">
              <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                <path d="M7.775 3.275a.75.75 0 0 0 1.06 1.06l1.25-1.25a2 2 0 1 1 2.83 2.83l-2.5 2.5a2 2 0 0 1-2.83 0 .75.75 0 0 0-1.06 1.06 3.5 3.5 0 0 0 4.95 0l2.5-2.5a3.5 3.5 0 0 0-4.95-4.95l-1.25 1.25zm-4.69 9.64a2 2 0 0 1 0-2.83l2.5-2.5a2 2 0 0 1 2.83 0 .75.75 0 0 0 1.06-1.06 3.5 3.5 0 0 0-4.95 0l-2.5 2.5a3.5 3.5 0 0 0 4.95 4.95l1.25-1.25a.75.75 0 0 0-1.06-1.06l-1.25 1.25a2 2 0 0 1-2.83 0z"/>
              </svg>
            </button>
            <button (click)="inlineCode()" title="行內程式碼" class="toolbar-btn">&#96;abc&#96;</button>
            <button (click)="codeBlock()" title="程式碼區塊" class="toolbar-btn">&#96;&#96;&#96;</button>
            <button (click)="blockquote()" title="引用" class="toolbar-btn">
              <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                <path d="M3 3.5h2.5a.5.5 0 0 1 0 1H3a.5.5 0 0 1 0-1zm0 3h2.5a.5.5 0 0 1 0 1H3a.5.5 0 0 1 0-1zm0 3h2.5a.5.5 0 0 1 0 1H3a.5.5 0 0 1 0-1zM6.5 3.5H14a.5.5 0 0 1 0 1H6.5a.5.5 0 0 1 0-1zm0 3H14a.5.5 0 0 1 0 1H6.5a.5.5 0 0 1 0-1zm0 3H14a.5.5 0 0 1 0 1H6.5a.5.5 0 0 1 0-1z"/>
              </svg>
            </button>

            <span class="toolbar-sep" aria-hidden="true"></span>

            <button (click)="unorderedList()" title="無序清單" class="toolbar-btn">
              <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                <path d="M2 4a1 1 0 1 1 0-2 1 1 0 0 1 0 2zm3.75-1.5a.75.75 0 0 0 0 1.5h8.5a.75.75 0 0 0 0-1.5h-8.5zm0 5a.75.75 0 0 0 0 1.5h8.5a.75.75 0 0 0 0-1.5h-8.5zm0 5a.75.75 0 0 0 0 1.5h8.5a.75.75 0 0 0 0-1.5h-8.5zM2 9a1 1 0 1 1 0-2 1 1 0 0 1 0 2zm0 5a1 1 0 1 1 0-2 1 1 0 0 1 0 2z"/>
              </svg>
            </button>
            <button (click)="orderedList()" title="有序清單" class="toolbar-btn">
              <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                <path d="M5 3.25a.75.75 0 0 1 .75-.75h8.5a.75.75 0 0 1 0 1.5h-8.5A.75.75 0 0 1 5 3.25zm0 5a.75.75 0 0 1 .75-.75h8.5a.75.75 0 0 1 0 1.5h-8.5A.75.75 0 0 1 5 8.25zm0 5a.75.75 0 0 1 .75-.75h8.5a.75.75 0 0 1 0 1.5h-8.5A.75.75 0 0 1 5 13.25zM1 2.5h1v4H1V3h-.5V2L2 1.5V2.5zM1 8h1.5v.5H1.75l-.75.5v-.5L2.25 8H1V7.5l.5-.25V7H1V6.5h1.5v.5L1.5 7.5V8z"/>
              </svg>
            </button>
            <button (click)="taskList()" title="待辦清單" class="toolbar-btn">☐</button>

            <span class="toolbar-sep" aria-hidden="true"></span>

            <button (click)="horizontalRule()" title="水平線" class="toolbar-btn">—</button>

          </div>
        }

        <!-- Body -->
        <div class="flex flex-1 overflow-hidden">

          <!-- Editor pane -->
          @if (mode() !== 'preview') {
            <div
              #editorPane
              class="flex flex-col overflow-hidden relative"
              [class.flex-1]="mode() === 'edit' || split.width() === null"
              [class.shrink-0]="mode() === 'split' && split.width() !== null"
              [style.width.px]="mode() === 'split' ? split.width() : null"
            >
              <md-editor
                class="flex-1 block overflow-hidden"
                [value]="note()!.content"
                [notes]="allNotes()"
                (valueChange)="onContentChange($event)"
                (imageDropped)="onImageDropped($event)"
              />
              @if (imageUploading()) {
                <div class="absolute inset-0 bg-[var(--color-bg)]/60 flex items-center justify-center pointer-events-none">
                  <span class="text-xs text-[var(--color-muted)]">上傳圖片中…</span>
                </div>
              }
            </div>
          }

          <!-- Divider / split resize handle -->
          @if (mode() === 'split') {
            <div
              (mousedown)="startSplitResize($event)"
              class="w-1 shrink-0 cursor-col-resize bg-[var(--color-border)] hover:bg-[var(--color-accent)] transition-colors"
              aria-hidden="true"
            ></div>
          }

          <!-- Preview pane (split or preview mode) -->
          @if (mode() !== 'edit') {
            <div class="flex-1 overflow-y-auto bg-[var(--color-bg)]">
              <div
                class="max-w-3xl mx-auto px-8 py-8 prose prose-invert prose-sm"
                [innerHTML]="previewHtml()"
                (click)="onPreviewClick($event)"
              ></div>
            </div>
          }

          <!-- AI Structure pane (shown when a structure is selected) -->
          <div
            (mousedown)="aiPane.startResize($event)"
            class="w-1 shrink-0 cursor-col-resize bg-[var(--color-border)] hover:bg-[var(--color-accent)] transition-colors"
            [style.display]="activeStructure() ? null : 'none'"
            aria-hidden="true"
          ></div>
          @if (activeStructure()) {
            <div class="shrink-0 flex flex-col overflow-hidden bg-[var(--color-bg)]" [style.width.px]="aiPane.width()">
              <div class="flex items-center justify-between px-4 py-2.5 border-b border-[var(--color-border)] bg-[var(--color-surface)] shrink-0">
                <span class="text-xs font-semibold text-[var(--color-text)] truncate pr-2">{{ activeStructure()!.description }}</span>
                <div class="flex items-center gap-2 shrink-0">
                  <button
                    (click)="applyStructure(activeStructure()!)"
                    class="text-xs text-[var(--color-accent)] hover:underline whitespace-nowrap"
                  >套用到筆記</button>
                  <button
                    (click)="activeStructure.set(null)"
                    class="text-[var(--color-muted)] hover:text-[var(--color-text)] text-sm leading-none"
                    aria-label="關閉 AI 結構化面板"
                  >✕</button>
                </div>
              </div>
              <div class="flex-1 overflow-y-auto px-6 py-6">
                <div class="prose prose-invert prose-sm" [innerHTML]="structureHtml()"></div>
              </div>
              <div class="px-4 py-2 border-t border-[var(--color-border)] shrink-0">
                <span class="text-xs text-[var(--color-muted)]">{{ formatDate(activeStructure()!.createdAt) }}</span>
              </div>
            </div>
          }

          <!-- Sidebar -->
          @if (true) {
            <div
              (mousedown)="sidebar.startResize($event)"
              class="w-1 shrink-0 cursor-col-resize bg-[var(--color-border)] hover:bg-[var(--color-accent)] transition-colors"
              aria-hidden="true"
            ></div>
            <aside [style.width.px]="sidebar.width()" class="shrink-0 bg-[var(--color-surface)] overflow-y-auto flex flex-col text-sm">

              <!-- Category -->
              <div class="p-4 border-b border-[var(--color-border)]">
                <div class="text-xs text-[var(--color-muted)] uppercase tracking-widest mb-2 font-semibold">分類</div>
                <select
                  class="w-full bg-[var(--color-bg)] border border-[var(--color-border)] text-[var(--color-text)] text-sm px-2 py-1.5 outline-none focus:border-[var(--color-accent)]"
                  [ngModel]="note()!.categoryId ?? ''"
                  (change)="onCategoryChange($event)"
                >
                  <option value="">無分類</option>
                  @for (cat of categories(); track cat.id) {
                    <option [value]="cat.id">{{ cat.name }}</option>
                  }
                </select>
              </div>

              <!-- Linked notes -->
              <div class="p-4 border-b border-[var(--color-border)]">
                <div class="text-xs text-[var(--color-muted)] uppercase tracking-widest mb-2 font-semibold">連結筆記</div>
                @if (note()!.linkedNoteIds.length === 0) {
                  <p class="text-xs text-[var(--color-muted)]">無連結</p>
                } @else {
                  @for (id of note()!.linkedNoteIds; track id) {
                    <a
                      [routerLink]="['/notes', id]"
                      class="block text-xs text-[var(--color-accent)] hover:underline truncate mb-1"
                    >{{ linkedNoteMap().get(id) ?? id }}</a>
                  }
                }
              </div>

              <!-- Share links -->
              <div class="p-4 border-b border-[var(--color-border)]">
                <div class="text-xs text-[var(--color-muted)] uppercase tracking-widest mb-3 font-semibold">分享連結</div>
                @if (shareUrlRead()) {
                  <div class="flex gap-1">
                    <input
                      readonly
                      [value]="shareUrlRead()"
                      (click)="copyShare(shareUrlRead()!)"
                      class="flex-1 min-w-0 bg-[var(--color-bg)] border border-[var(--color-border)] text-[var(--color-accent)] text-xs px-2 py-1.5 cursor-pointer"
                      title="點擊複製"
                    />
                    <app-button variant="ghost" size="sm" (click)="deleteShare()">撤銷</app-button>
                  </div>
                } @else {
                  <app-button variant="ghost" size="sm" (click)="share()">生成</app-button>
                }
              </div>

              <!-- AI Structures -->
              <div class="p-4 border-b border-[var(--color-border)]">
                <div class="text-xs text-[var(--color-muted)] uppercase tracking-widest mb-3 font-semibold">AI 結構化版本</div>
                @if (structures().length === 0) {
                  <p class="text-xs text-[var(--color-muted)]">尚無結構化結果</p>
                } @else {
                  <div class="flex flex-col gap-1.5">
                    @for (s of structures(); track s.structureId) {
                      <button
                        (click)="selectStructure(s)"
                        [class]="'w-full text-left px-3 py-2 text-xs rounded-sm border transition-colors ' + (activeStructure()?.structureId === s.structureId ? 'border-[var(--color-accent)] bg-[var(--color-surface-2)] text-[var(--color-accent)]' : 'border-[var(--color-border)] hover:bg-[var(--color-surface-2)] text-[var(--color-text)]')"
                      >
                        <div class="font-medium truncate">{{ s.description }}</div>
                        <div class="text-[var(--color-muted)] mt-0.5">{{ formatDate(s.createdAt) }}</div>
                      </button>
                    }
                  </div>
                }
              </div>

              <!-- Comments -->
              <div class="p-4 flex-1">
                <div class="text-xs text-[var(--color-muted)] uppercase tracking-widest mb-3 font-semibold">留言</div>
                @if (auth.isLoggedIn()) {
                  <div class="flex flex-col gap-2 mb-3">
                    <textarea
                      [(ngModel)]="newComment"
                      rows="2"
                      placeholder="輸入留言…"
                      class="w-full bg-[var(--color-bg)] border border-[var(--color-border)] text-[var(--color-text)] text-sm px-2 py-1.5 outline-none resize-none focus:border-[var(--color-accent)] placeholder:text-[var(--color-muted)]"
                    ></textarea>
                    <app-button size="sm" (click)="addComment()" [loading]="commentSending()">送出</app-button>
                  </div>
                }
                @for (c of comments(); track c.commentId) {
                  <div class="mb-3 flex gap-2">
                    <div class="w-5 h-5 rounded-full shrink-0 overflow-hidden mt-0.5">
                      @if (c.avatarUrl) {
                        <img [src]="c.avatarUrl" [alt]="c.username" class="w-full h-full object-cover" />
                      } @else {
                        <div class="w-full h-full bg-[var(--color-accent)] flex items-center justify-center text-white text-[9px] font-semibold">
                          {{ c.username.slice(0,1).toUpperCase() }}
                        </div>
                      }
                    </div>
                    <div class="flex-1 min-w-0">
                      <div class="text-xs text-[var(--color-accent)] mb-0.5 font-semibold">{{ c.username }}</div>
                      <div class="text-xs text-[var(--color-text)] leading-relaxed">{{ c.content }}</div>
                      <button
                        (click)="likeComment(c.commentId)"
                        class="flex items-center gap-1 mt-1.5 text-[10px] font-mono transition-colors"
                        [class]="c.likedByMe ? 'text-[var(--color-accent)]' : 'text-[var(--color-muted)] hover:text-[var(--color-accent)]'"
                        [attr.aria-pressed]="c.likedByMe"
                        [attr.aria-label]="c.likedByMe ? '收回讚' : '按讚'"
                      >
                        <svg width="10" height="10" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                          <path d="M7.655 14.916v-.001h-.002l-.006-.003-.018-.01a22.066 22.066 0 0 1-3.744-2.584C2.045 10.731 0 8.35 0 5.5 0 2.836 2.086 1 4.25 1 5.797 1 7.153 1.802 8 3.02 8.847 1.802 10.203 1 11.75 1 13.914 1 16 2.836 16 5.5c0 2.85-2.044 5.231-3.886 6.818a22.08 22.08 0 0 1-3.744 2.584l-.018.01-.006.003h-.002Z"/>
                        </svg>
                        {{ c.likeCount }}
                      </button>
                    </div>
                  </div>
                }
              </div>

            </aside>
          }
        </div>
      </div>
    }
  `,
})
export class NoteEditorComponent implements OnInit {
  @ViewChild(MdEditorComponent) private editor?: MdEditorComponent;
  @ViewChild('editorPane') private editorPaneRef?: ElementRef<HTMLDivElement>;

  startSplitResize(e: MouseEvent) {
    this.split.startResize(e, this.editorPaneRef?.nativeElement?.offsetWidth);
  }

  readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly location = inject(Location);
  private readonly noteService = inject(NoteService);
  private readonly categoryService = inject(CategoryService);
  private readonly commentService = inject(CommentService);
  private readonly imageService = inject(ImageService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly sanitizer = inject(DomSanitizer);

  readonly note = signal<Note | null>(null);
  readonly categories = signal<Category[]>([]);
  readonly comments = signal<Comment[]>([]);
  readonly structures = signal<NoteStructureSummary[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly structuring = signal(false);
  readonly commentSending = signal(false);
  readonly imageUploading = signal(false);
  readonly mode = signal<ViewMode>('split');
  readonly shareUrlRead = signal<string | null>(null);
  readonly previewHtml = signal<SafeHtml>('');
  readonly activeStructure = signal<NoteStructureSummary | null>(null);
  readonly structureHtml = signal<SafeHtml>('');
  readonly linkedNoteMap = signal<Map<string, string>>(new Map());
  readonly allNotes = signal<{ noteId: string; title: string }[]>([]);
  readonly sidebar = createResizable({ defaultWidth: 224, min: 160, max: 480, direction: 'left' });
  readonly split = createResizable({ defaultWidth: null, min: 200, direction: 'right' });
  readonly aiPane = createResizable({ defaultWidth: 320, min: 240, max: 600, direction: 'left' });
  newComment = '';
  structurePrompt = '';

  private readonly save$ = new Subject<void>();
  private pendingTitle = '';
  private pendingContent = '';
  private renderCounter = 0;
  private structureRenderCounter = 0;

  constructor() {
    effect(() => {
      const mode = this.mode();
      const content = this.note()?.content ?? '';
      const noteMap = this.linkedNoteMap();
      if (mode === 'edit') return;
      const counter = ++this.renderCounter;
      renderMarkdown(content, noteMap).then(html => {
        if (counter === this.renderCounter) {
          this.previewHtml.set(this.sanitizer.bypassSecurityTrustHtml(html));
        }
      });
    });

    effect(() => {
      const s = this.activeStructure();
      if (!s) { this.structureHtml.set(''); return; }
      const counter = ++this.structureRenderCounter;
      renderMarkdown(s.content).then(html => {
        if (counter === this.structureRenderCounter) {
          this.structureHtml.set(this.sanitizer.bypassSecurityTrustHtml(html));
        }
      });
    });

    this.save$.pipe(
      debounceTime(1000),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(() => this.doSave());
  }

  ngOnInit() {
    this.route.paramMap.pipe(
      map(p => p.get('id')!),
      distinctUntilChanged(),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(id => this.loadNoteData(id));
  }

  private async loadNoteData(id: string) {
    this.loading.set(true);
    this.note.set(null);
    this.activeStructure.set(null);
    this.shareUrlRead.set(null);

    const [note, categories, comments, structures] = await Promise.all([
      this.noteService.get(id),
      this.categoryService.list(),
      this.commentService.listForNote(id),
      this.noteService.listStructures(id),
    ]);
    this.note.set(note);
    this.pendingTitle = note.title;
    this.pendingContent = note.content;
    this.categories.set(categories);
    this.comments.set(comments);
    this.structures.set(structures);
    if (note.sharedToken) {
      this.shareUrlRead.set(`${window.location.origin}/share/${note.sharedToken}`);
    }
    this.loading.set(false);

    this.noteService.list().then(all => {
      this.allNotes.set(all);
      this.linkedNoteMap.set(new Map(all.map(n => [n.noteId, n.title])));
    });
  }

  onPreviewClick(e: MouseEvent) {
    const anchor = (e.target as HTMLElement).closest('a[data-wiki-link]');
    if (!anchor) return;
    const id = anchor.getAttribute('data-wiki-link');
    if (id) {
      e.preventDefault();
      this.router.navigate(['/notes', id]);
    }
  }

  onTitleChange(e: Event) {
    this.pendingTitle = (e.target as HTMLInputElement).value;
    this.note.update(n => n ? { ...n, title: this.pendingTitle } : n);
    this.save$.next();
  }

  onContentChange(content: string) {
    this.pendingContent = content;
    this.note.update(n => n ? { ...n, content } : n);
    this.save$.next();
  }

  onCategoryChange(e: Event) {
    const val = (e.target as HTMLSelectElement).value;
    const categoryId = val || undefined;
    this.note.update(n => n ? { ...n, categoryId } : n);
    const id = this.route.snapshot.paramMap.get('id')!;
    this.noteService.update(id, { categoryId })
      .then(() => this.toast.success('分類已更新'))
      .catch(() => this.toast.error('更新失敗'));
  }

  private async doSave() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.saving.set(true);
    try {
      await this.noteService.update(id, { title: this.pendingTitle, content: this.pendingContent });
    } finally {
      this.saving.set(false);
    }
  }

  async structure() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.structuring.set(true);
    try {
      const result = await this.noteService.structure(id, this.structurePrompt);
      const newStructure: NoteStructureSummary = {
        structureId: result.structureId,
        description: result.description,
        content: result.content,
        createdAt: new Date().toISOString(),
      };
      this.structures.update(list => [newStructure, ...list]);
      this.activeStructure.set(newStructure);
      this.toast.success('AI 整理完成，結果已新增至側邊欄');
    } catch {
      this.toast.error('AI 整理失敗');
    } finally {
      this.structuring.set(false);
    }
  }

  selectStructure(s: NoteStructureSummary) {
    this.activeStructure.set(this.activeStructure()?.structureId === s.structureId ? null : s);
  }

  applyStructure(s: NoteStructureSummary) {
    this.note.update(n => n ? { ...n, title: s.description, content: s.content } : n);
    this.pendingTitle = s.description;
    this.pendingContent = s.content;
    this.save$.next();
    this.activeStructure.set(null);
    this.toast.success('已套用結構化版本');
  }

  async share() {
    const id = this.route.snapshot.paramMap.get('id')!;
    try {
      const url = await this.noteService.createShareLink(id);
      this.shareUrlRead.set(url);
      await navigator.clipboard.writeText(url);
      this.toast.success('分享連結已複製');
    } catch {
      this.toast.error('建立分享連結失敗');
    }
  }

  async deleteShare() {
    const id = this.route.snapshot.paramMap.get('id')!;
    try {
      await this.noteService.deleteShareLink(id);
      this.shareUrlRead.set(null);
      this.toast.success('分享連結已撤銷');
    } catch {
      this.toast.error('撤銷失敗');
    }
  }

  async copyShare(url: string) {
    await navigator.clipboard.writeText(url);
    this.toast.info('已複製');
  }

  async onImageDropped(file: File) {
    this.imageUploading.set(true);
    try {
      const results = await this.imageService.upload([file]);
      const url = results[0]?.url;
      if (url) {
        const markdown = `\n![${file.name}](${url})\n`;
        this.editor?.insertText(markdown);
      }
    } catch {
      this.toast.error('圖片上傳失敗');
    } finally {
      this.imageUploading.set(false);
    }
  }

  async likeComment(commentId: string) {
    const current = this.comments().find(x => x.commentId === commentId);
    if (!current) return;
    const isLiked = current.likedByMe;
    try {
      isLiked
        ? await this.commentService.unlike(commentId)
        : await this.commentService.like(commentId);
      this.comments.update(cs => cs.map(x =>
        x.commentId === commentId
          ? { ...x, likedByMe: !isLiked, likeCount: x.likeCount + (isLiked ? -1 : 1) }
          : x
      ));
    } catch (err) {
      const status = (err as { status?: number })?.status;
      if (!isLiked && status === 409) {
        this.comments.update(cs => cs.map(x =>
          x.commentId === commentId ? { ...x, likedByMe: true } : x
        ));
      } else {
        this.toast.error('操作失敗');
      }
    }
  }

  async addComment() {
    if (!this.newComment.trim()) return;
    const id = this.route.snapshot.paramMap.get('id')!;
    this.commentSending.set(true);
    try {
      await this.commentService.addToNote(id, this.newComment);
      const comments = await this.commentService.listForNote(id);
      this.comments.set(comments);
      this.newComment = '';
    } catch {
      this.toast.error('留言失敗');
    } finally {
      this.commentSending.set(false);
    }
  }

  formatDate(iso: string): string {
    const d = new Date(iso);
    return `${d.getMonth() + 1}/${d.getDate()} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
  }

  // Toolbar actions
  bold() { this.editor?.wrapSelection('**', '**', 'text'); }
  italic() { this.editor?.wrapSelection('*', '*', 'text'); }
  strikethrough() { this.editor?.wrapSelection('~~', '~~', 'text'); }
  heading(level: 1 | 2 | 3) { this.editor?.insertLinePrefix('#'.repeat(level) + ' '); }
  insertLink() { this.editor?.wrapSelection('[', '](url)', 'text'); }
  inlineCode() { this.editor?.wrapSelection('`', '`', 'code'); }
  codeBlock() { this.editor?.insertBlock('```\n\n```', 4); }
  blockquote() { this.editor?.insertLinePrefix('> '); }
  unorderedList() { this.editor?.insertLinePrefix('- '); }
  orderedList() { this.editor?.insertLinePrefix('1. '); }
  taskList() { this.editor?.insertLinePrefix('- [ ] '); }
  horizontalRule() { this.editor?.insertBlock('\n---\n'); }
}

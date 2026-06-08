import { Component, inject, signal, computed, OnInit, ChangeDetectionStrategy, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgTemplateOutlet } from '@angular/common';
import { NoteService } from '../../core/api/note.service';
import { CategoryService } from '../../core/api/category.service';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../shared/services/toast.service';
import { SpinnerComponent } from '../../shared/components/spinner';
import { DialogComponent } from '../../shared/components/dialog';
import { NoteSummary, Category } from '../../core/api/api.types';
import { createResizable } from '../../shared/utils/resizable';

@Component({
  selector: 'note-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, NgTemplateOutlet, SpinnerComponent, DialogComponent],
  template: `
    <div class="h-screen flex overflow-hidden bg-[var(--color-bg)]">

      <!-- ───── Left Sidebar ───── -->
      <aside class="shrink-0 bg-[var(--color-surface)] flex flex-col overflow-hidden" [style.width.px]="sidebar.width()">

        <!-- Search -->
        <div class="px-3 pt-3 pb-2">
          <label class="flex items-center gap-2 bg-[var(--color-surface-2)] border border-[var(--color-border)] px-3 py-1.5 rounded cursor-text">
            <svg width="13" height="13" viewBox="0 0 16 16" fill="var(--color-muted)" aria-hidden="true">
              <path d="M10.68 11.74a6 6 0 0 1-7.922-8.982 6 6 0 0 1 8.982 7.922l3.04 3.04a.75.75 0 1 1-1.06 1.06l-3.04-3.04ZM6 11a5 5 0 1 1 0-10 5 5 0 0 1 0 10Z"/>
            </svg>
            <input
              type="text"
              placeholder="Search keyword or tag."
              class="flex-1 bg-transparent text-sm text-[var(--color-text)] outline-none placeholder:text-[var(--color-muted)]"
              (input)="searchQuery.set($any($event.target).value)"
              [value]="searchQuery()"
            />
            <kbd class="text-[10px] text-[var(--color-muted)] font-mono bg-[var(--color-surface)] px-1 rounded">/</kbd>
          </label>
        </div>

        <!-- Create note -->
        <div class="px-3 pb-3">
          <button
            (click)="createNote()"
            [disabled]="creating()"
            class="w-full flex items-center justify-center gap-2 bg-[var(--color-accent)] text-white text-sm font-medium px-3 py-2 rounded hover:opacity-90 transition-opacity disabled:opacity-50"
          >
            <svg width="13" height="13" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
              <path d="M7.75 2a.75.75 0 0 1 .75.75V7h4.25a.75.75 0 0 1 0 1.5H8.5v4.25a.75.75 0 0 1-1.5 0V8.5H2.75a.75.75 0 0 1 0-1.5H7V2.75A.75.75 0 0 1 7.75 2Z"/>
            </svg>
            Create my note
          </button>
        </div>

        <!-- Navigation tree -->
        <nav class="flex-1 overflow-y-auto px-2 pb-2" aria-label="Notes navigation">

          <!-- Graph -->
          <a
            routerLink="/graph"
            class="w-full flex items-center gap-2 px-2 py-1.5 text-sm rounded transition-colors mb-2 text-[var(--color-muted)] hover:text-[var(--color-text)] hover:bg-[var(--color-surface-2)]"
          >
            <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
              <path d="M6 3.5a2.5 2.5 0 1 1 5 0 2.5 2.5 0 0 1-5 0Zm2.5-1a1 1 0 1 0 0 2 1 1 0 0 0 0-2ZM2.5 9a2.5 2.5 0 1 1 5 0 2.5 2.5 0 0 1-5 0Zm2.5-1a1 1 0 1 0 0 2 1 1 0 0 0 0-2Zm5.5 4.5a2.5 2.5 0 1 1 5 0 2.5 2.5 0 0 1-5 0Zm2.5-1a1 1 0 1 0 0 2 1 1 0 0 0 0-2ZM4.987 7.483a.75.75 0 0 1 .275 1.025l-.5.866a.75.75 0 1 1-1.299-.75l.5-.866a.75.75 0 0 1 1.024-.275Zm6.538-4.5a.75.75 0 0 1 .275 1.025l-.5.866a.75.75 0 1 1-1.299-.75l.5-.866a.75.75 0 0 1 1.024-.275Zm-.987 7.233a.75.75 0 0 1-.275 1.025l-.5.866a.75.75 0 1 1-1.299-.75l.5-.866a.75.75 0 0 1 1.074.725Z"/>
            </svg>
            <span>Graph View</span>
          </a>

          <!-- All notes -->
          <button
            (click)="selectCategory(null)"
            class="w-full flex items-center gap-2 px-2 py-1.5 text-sm rounded transition-colors mb-0.5"
            [class]="isSelected(null) ? 'text-[var(--color-text)] bg-[var(--color-surface-2)]' : 'text-[var(--color-muted)] hover:text-[var(--color-text)] hover:bg-[var(--color-surface-2)]'"
          >
            <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
              <path d="M1.5 1.75V13.5h13.75a.75.75 0 0 1 0 1.5H.75a.75.75 0 0 1-.75-.75V1.75a.75.75 0 0 1 1.5 0Z"/><path d="M5.75 4a.75.75 0 0 1 .75.75v7.5a.75.75 0 0 1-1.5 0v-7.5A.75.75 0 0 1 5.75 4Zm4 2.5a.75.75 0 0 1 .75.75v5a.75.75 0 0 1-1.5 0v-5a.75.75 0 0 1 .75-.75Zm4-2a.75.75 0 0 1 .75.75v7a.75.75 0 0 1-1.5 0v-7a.75.75 0 0 1 .75-.75Z"/>
            </svg>
            <span class="font-medium">所有筆記</span>
            <span class="ml-auto text-xs text-[var(--color-muted)]">{{ notes().length }}</span>
          </button>

          <!-- Categories (flat) -->
          @for (cat of categories(); track cat.id) {
            @if (renamingCategoryId() === cat.id) {
              <div class="flex items-center gap-1 px-2 py-1 mt-0.5">
                <input
                  #renameInput
                  type="text"
                  [value]="renameCategoryName"
                  (input)="renameCategoryName = $any($event.target).value"
                  (keydown.enter)="confirmRename(cat.id)"
                  (keydown.escape)="cancelRename()"
                  class="flex-1 bg-[var(--color-surface-2)] border border-[var(--color-accent)] text-sm text-[var(--color-text)] px-2 py-0.5 outline-none rounded-sm min-w-0"
                  aria-label="分類名稱"
                />
                <button (click)="confirmRename(cat.id)" class="text-[var(--color-accent)] text-sm hover:text-[var(--color-text)] transition-colors shrink-0" aria-label="確認">✓</button>
                <button (click)="cancelRename()" class="text-[var(--color-muted)] text-sm hover:text-[var(--color-text)] transition-colors shrink-0" aria-label="取消">✕</button>
              </div>
            } @else {
              <div
                class="group flex items-center rounded transition-colors"
                [class]="isSelected(cat.id) ? 'text-[var(--color-text)] bg-[var(--color-surface-2)]' : 'text-[var(--color-muted)] hover:text-[var(--color-text)] hover:bg-[var(--color-surface-2)]'"
              >
                <button
                  (click)="selectCategory(cat.id)"
                  class="flex-1 flex items-center gap-2 px-2 py-1.5 text-sm min-w-0"
                >
                  <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                    <path d="M1 7.775V2.75C1 1.784 1.784 1 2.75 1h5.025c.464 0 .91.184 1.238.513l6.25 6.25a1.75 1.75 0 0 1 0 2.474l-5.026 5.026a1.75 1.75 0 0 1-2.474 0l-6.25-6.25A1.752 1.752 0 0 1 1 7.775Zm1.5 0c0 .066.026.13.073.177l6.25 6.25a.25.25 0 0 0 .354 0l5.025-5.025a.25.25 0 0 0 0-.354l-6.25-6.25a.25.25 0 0 0-.177-.073H2.75a.25.25 0 0 0-.25.25ZM6 5a1 1 0 1 1 0 2 1 1 0 0 1 0-2Z"/>
                  </svg>
                  <span class="truncate">{{ cat.name }} ({{ categoryCounts().get(cat.id) ?? 0 }})</span>
                </button>
                <button
                  (click)="startRename(cat)"
                  class="opacity-0 group-hover:opacity-100 shrink-0 p-1.5 hover:text-[var(--color-text)] transition-opacity"
                  aria-label="重新命名分類"
                  title="重新命名"
                >
                  <svg width="12" height="12" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                    <path d="M11.013 1.427a1.75 1.75 0 0 1 2.474 0l1.086 1.086a1.75 1.75 0 0 1 0 2.474l-8.61 8.61c-.21.21-.47.364-.756.445l-3.251.93a.75.75 0 0 1-.927-.928l.929-3.25c.081-.286.235-.547.445-.758l8.61-8.61Z"/>
                  </svg>
                </button>
                <button
                  (click)="confirmDeleteCategory(cat)"
                  class="opacity-0 group-hover:opacity-100 shrink-0 p-1.5 hover:text-[var(--color-danger)] transition-opacity"
                  aria-label="刪除分類"
                  title="刪除"
                >
                  <svg width="12" height="12" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                    <path d="M11 1.75V3h2.25a.75.75 0 0 1 0 1.5H2.75a.75.75 0 0 1 0-1.5H5V1.75C5 .784 5.784 0 6.75 0h2.5C10.216 0 11 .784 11 1.75ZM4.496 6.675l.66 6.6a.25.25 0 0 0 .249.225h5.19a.25.25 0 0 0 .249-.225l.66-6.6a.75.75 0 0 1 1.492.149l-.66 6.6A1.748 1.748 0 0 1 10.595 15h-5.19a1.75 1.75 0 0 1-1.741-1.575l-.66-6.6a.75.75 0 1 1 1.492-.15Z"/>
                  </svg>
                </button>
              </div>
            }
          }

          <!-- Untagged -->
          @if (uncategorizedCount() > 0) {
            <div
              class="flex items-center rounded transition-colors"
              [class]="isSelected('__none') ? 'text-[var(--color-text)] bg-[var(--color-surface-2)]' : 'text-[var(--color-muted)] hover:text-[var(--color-text)] hover:bg-[var(--color-surface-2)]'"
            >
              <button
                (click)="selectCategory('__none')"
                class="flex-1 flex items-center gap-2 px-2 py-1.5 text-sm min-w-0"
              >
                <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                  <path d="M1 7.775V2.75C1 1.784 1.784 1 2.75 1h5.025c.464 0 .91.184 1.238.513l6.25 6.25a1.75 1.75 0 0 1 0 2.474l-5.026 5.026a1.75 1.75 0 0 1-2.474 0l-6.25-6.25A1.752 1.752 0 0 1 1 7.775Zm1.5 0c0 .066.026.13.073.177l6.25 6.25a.25.25 0 0 0 .354 0l5.025-5.025a.25.25 0 0 0 0-.354l-6.25-6.25a.25.25 0 0 0-.177-.073H2.75a.25.25 0 0 0-.25.25ZM6 5a1 1 0 1 1 0 2 1 1 0 0 1 0-2Z"/>
                </svg>
                <span>未分類 ({{ uncategorizedCount() }})</span>
              </button>
            </div>
          }

          <!-- 新增分類 -->
          @if (showNewCategoryInput()) {
            <div class="flex items-center gap-1 px-2 py-1 mt-0.5">
              <input
                #newCatInput
                autofocus
                type="text"
                placeholder="分類名稱"
                class="flex-1 bg-[var(--color-surface-2)] border border-[var(--color-accent)] text-sm text-[var(--color-text)] px-2 py-0.5 outline-none rounded-sm min-w-0"
                (input)="newCategoryName.set($any($event.target).value)"
                (keydown.enter)="createCategory()"
                (keydown.escape)="cancelNewCategory()"
              />
              <button (click)="createCategory()" class="text-[var(--color-accent)] text-sm hover:text-[var(--color-text)] transition-colors shrink-0" aria-label="確認">✓</button>
              <button (click)="cancelNewCategory()" class="text-[var(--color-muted)] text-sm hover:text-[var(--color-text)] transition-colors shrink-0" aria-label="取消">✕</button>
            </div>
          } @else {
            <button
              (click)="showNewCategoryInput.set(true)"
              class="w-full flex items-center gap-1.5 px-2 py-1.5 text-sm text-[var(--color-muted)] hover:text-[var(--color-text)] transition-colors mt-0.5"
            >
              <span>+</span> 新增分類
            </button>
          }

        </nav>

        <!-- User info -->
        <div class="border-t border-[var(--color-border)] px-3 py-2.5 flex items-center gap-2">
          <input #avatarInput type="file" accept="image/*" class="hidden" (change)="onAvatarChange($event)" aria-label="上傳頭貼" />
          <button
            (click)="avatarInput.click()"
            class="relative w-6 h-6 rounded-full shrink-0 overflow-hidden group focus:outline-none focus:ring-2 focus:ring-[var(--color-accent)]"
            title="更換頭貼"
            aria-label="更換頭貼"
          >
            @if (auth.currentUser()?.avatarUrl) {
              <img [src]="auth.currentUser()!.avatarUrl" alt="頭貼" class="w-full h-full object-cover" />
            } @else {
              <div class="w-full h-full bg-[var(--color-accent)] flex items-center justify-center text-white text-xs font-semibold">
                {{ userInitial() }}
              </div>
            }
            <div class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
              <svg width="10" height="10" viewBox="0 0 16 16" fill="white" aria-hidden="true">
                <path d="M11.013 1.427a1.75 1.75 0 0 1 2.474 0l1.086 1.086a1.75 1.75 0 0 1 0 2.474l-8.61 8.61c-.21.21-.47.364-.756.445l-3.251.93a.75.75 0 0 1-.927-.928l.929-3.25c.081-.286.235-.547.445-.758l8.61-8.61Z"/>
              </svg>
            </div>
          </button>
          <span class="flex-1 text-sm text-[var(--color-text)] truncate">{{ auth.currentUser()?.username }}</span>
          <button
            (click)="auth.logout()"
            class="text-[var(--color-muted)] hover:text-[var(--color-danger)] transition-colors"
            title="登出"
            aria-label="登出"
          >
            <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
              <path d="M2 2.75C2 1.784 2.784 1 3.75 1h2.5a.75.75 0 0 1 0 1.5h-2.5a.25.25 0 0 0-.25.25v10.5c0 .138.112.25.25.25h2.5a.75.75 0 0 1 0 1.5h-2.5A1.75 1.75 0 0 1 2 13.25Zm10.44 4.5-1.97-1.97a.75.75 0 0 1 1.06-1.06l3.25 3.25a.75.75 0 0 1 0 1.06l-3.25 3.25a.75.75 0 1 1-1.06-1.06l1.97-1.97H6.75a.75.75 0 0 1 0-1.5Z"/>
            </svg>
          </button>
        </div>

      </aside>
      <div
        (mousedown)="sidebar.startResize($event)"
        class="w-1 shrink-0 cursor-col-resize bg-[var(--color-border)] hover:bg-[var(--color-accent)] transition-colors"
        aria-hidden="true"
      ></div>

      <!-- ───── Main Content ───── -->
      <main class="flex-1 flex flex-col overflow-hidden">

        <!-- Header -->
        <div class="px-6 py-3 border-b border-[var(--color-border)] flex items-center justify-between shrink-0">
          <h1 class="text-base font-semibold text-[var(--color-text)]">{{ pageTitle() }}</h1>
          <div class="flex items-center gap-2">
            @if (selectedCategory() && selectedCategory() !== '__none') {
              <button
                (click)="createNoteInCategory()"
                [disabled]="creating()"
                class="flex items-center gap-1.5 text-xs text-[var(--color-accent)] border border-[var(--color-accent)] px-2.5 py-1 rounded hover:bg-[var(--color-accent)] hover:text-white transition-colors disabled:opacity-50"
              >
                <svg width="11" height="11" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                  <path d="M7.75 2a.75.75 0 0 1 .75.75V7h4.25a.75.75 0 0 1 0 1.5H8.5v4.25a.75.75 0 0 1-1.5 0V8.5H2.75a.75.75 0 0 1 0-1.5H7V2.75A.75.75 0 0 1 7.75 2Z"/>
                </svg>
                新增筆記
              </button>
            }
            <!-- View toggle -->
            <div class="flex border border-[var(--color-border)] rounded overflow-hidden">
              <button
                (click)="viewMode.set('grid')"
                class="p-1.5 transition-colors"
                [class]="viewMode() === 'grid' ? 'bg-[var(--color-surface-2)] text-[var(--color-text)]' : 'text-[var(--color-muted)] hover:text-[var(--color-text)]'"
                title="格狀"
                aria-label="格狀檢視"
              >
                <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                  <path d="M1 2.5A1.5 1.5 0 0 1 2.5 1h3A1.5 1.5 0 0 1 7 2.5v3A1.5 1.5 0 0 1 5.5 7h-3A1.5 1.5 0 0 1 1 5.5v-3Zm8 0A1.5 1.5 0 0 1 10.5 1h3A1.5 1.5 0 0 1 15 2.5v3A1.5 1.5 0 0 1 13.5 7h-3A1.5 1.5 0 0 1 9 5.5v-3Zm-8 8A1.5 1.5 0 0 1 2.5 9h3A1.5 1.5 0 0 1 7 10.5v3A1.5 1.5 0 0 1 5.5 15h-3A1.5 1.5 0 0 1 1 13.5v-3Zm8 0A1.5 1.5 0 0 1 10.5 9h3a1.5 1.5 0 0 1 1.5 1.5v3a1.5 1.5 0 0 1-1.5 1.5h-3A1.5 1.5 0 0 1 9 13.5v-3Z"/>
                </svg>
              </button>
              <button
                (click)="viewMode.set('list')"
                class="p-1.5 transition-colors"
                [class]="viewMode() === 'list' ? 'bg-[var(--color-surface-2)] text-[var(--color-text)]' : 'text-[var(--color-muted)] hover:text-[var(--color-text)]'"
                title="列表"
                aria-label="列表檢視"
              >
                <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                  <path d="M2 3.75A.75.75 0 0 1 2.75 3h10.5a.75.75 0 0 1 0 1.5H2.75A.75.75 0 0 1 2 3.75Zm0 4A.75.75 0 0 1 2.75 7h10.5a.75.75 0 0 1 0 1.5H2.75A.75.75 0 0 1 2 7.75Zm0 4a.75.75 0 0 1 .75-.75h10.5a.75.75 0 0 1 0 1.5H2.75a.75.75 0 0 1-.75-.75Z"/>
                </svg>
              </button>
            </div>
          </div>
        </div>

        <!-- Content body -->
        <div class="flex-1 overflow-y-auto p-6">
          @if (loading()) {
            <div class="flex justify-center py-16"><spinner /></div>

          } @else if (searchQuery()) {
            <!-- Search results -->
            <section>
              <h2 class="text-xs text-[var(--color-muted)] uppercase tracking-widest mb-4">
                搜尋結果「{{ searchQuery() }}」— {{ displayedNotes().length }} 筆
              </h2>
              @if (displayedNotes().length === 0) {
                <p class="text-sm text-[var(--color-muted)]">找不到符合的筆記</p>
              } @else {
                <ng-container [ngTemplateOutlet]="noteGrid" [ngTemplateOutletContext]="{ notes: displayedNotes() }"></ng-container>
              }
            </section>

          } @else if (selectedCategory() === null) {
            <!-- Overview: folders + uncategorized -->

            @if (categories().length > 0) {
              <section class="mb-8">
                <h2 class="flex items-center gap-2 text-sm font-semibold text-[var(--color-text)] mb-4">
                  <svg width="14" height="14" viewBox="0 0 16 16" fill="var(--color-muted)" aria-hidden="true">
                    <path d="M1 7.775V2.75C1 1.784 1.784 1 2.75 1h5.025c.464 0 .91.184 1.238.513l6.25 6.25a1.75 1.75 0 0 1 0 2.474l-5.026 5.026a1.75 1.75 0 0 1-2.474 0l-6.25-6.25A1.752 1.752 0 0 1 1 7.775Zm1.5 0c0 .066.026.13.073.177l6.25 6.25a.25.25 0 0 0 .354 0l5.025-5.025a.25.25 0 0 0 0-.354l-6.25-6.25a.25.25 0 0 0-.177-.073H2.75a.25.25 0 0 0-.25.25ZM6 5a1 1 0 1 1 0 2 1 1 0 0 1 0-2Z"/>
                  </svg>
                  分類
                  <span class="text-[var(--color-muted)] font-normal">{{ categories().length }}</span>
                </h2>

                @if (viewMode() === 'grid') {
                  <div class="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
                    @for (cat of categories(); track cat.id) {
                      <button
                        (click)="selectCategory(cat.id)"
                        class="text-left border border-[var(--color-border)] bg-[var(--color-surface)] p-4 rounded hover:border-[var(--color-accent)] hover:bg-[var(--color-surface-2)] transition-colors group"
                      >
                        <svg class="text-[var(--color-muted)] group-hover:text-[var(--color-accent)] transition-colors mb-3" width="22" height="22" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                          <path d="M1 7.775V2.75C1 1.784 1.784 1 2.75 1h5.025c.464 0 .91.184 1.238.513l6.25 6.25a1.75 1.75 0 0 1 0 2.474l-5.026 5.026a1.75 1.75 0 0 1-2.474 0l-6.25-6.25A1.752 1.752 0 0 1 1 7.775Zm1.5 0c0 .066.026.13.073.177l6.25 6.25a.25.25 0 0 0 .354 0l5.025-5.025a.25.25 0 0 0 0-.354l-6.25-6.25a.25.25 0 0 0-.177-.073H2.75a.25.25 0 0 0-.25.25ZM6 5a1 1 0 1 1 0 2 1 1 0 0 1 0-2Z"/>
                        </svg>
                        <div class="text-sm font-medium text-[var(--color-text)] truncate">{{ cat.name }}</div>
                        <div class="text-xs text-[var(--color-muted)] mt-1">{{ categoryCounts().get(cat.id) ?? 0 }} item(s)</div>
                      </button>
                    }
                  </div>
                } @else {
                  <div class="divide-y divide-[var(--color-border)] border border-[var(--color-border)] rounded overflow-hidden">
                    @for (cat of categories(); track cat.id) {
                      <button
                        (click)="selectCategory(cat.id)"
                        class="w-full flex items-center gap-3 px-4 py-3 text-left hover:bg-[var(--color-surface-2)] transition-colors"
                      >
                        <svg class="text-[var(--color-muted)] shrink-0" width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                          <path d="M1 7.775V2.75C1 1.784 1.784 1 2.75 1h5.025c.464 0 .91.184 1.238.513l6.25 6.25a1.75 1.75 0 0 1 0 2.474l-5.026 5.026a1.75 1.75 0 0 1-2.474 0l-6.25-6.25A1.752 1.752 0 0 1 1 7.775Zm1.5 0c0 .066.026.13.073.177l6.25 6.25a.25.25 0 0 0 .354 0l5.025-5.025a.25.25 0 0 0 0-.354l-6.25-6.25a.25.25 0 0 0-.177-.073H2.75a.25.25 0 0 0-.25.25ZM6 5a1 1 0 1 1 0 2 1 1 0 0 1 0-2Z"/>
                        </svg>
                        <span class="flex-1 text-sm text-[var(--color-text)]">{{ cat.name }}</span>
                        <span class="text-xs text-[var(--color-muted)]">{{ categoryCounts().get(cat.id) ?? 0 }} items</span>
                      </button>
                    }
                  </div>
                }
              </section>
            }

            @if (uncategorizedCount() > 0) {
              <section>
                <h2 class="flex items-center gap-2 text-sm font-semibold text-[var(--color-text)] mb-4">
                  <svg width="14" height="14" viewBox="0 0 16 16" fill="var(--color-muted)" aria-hidden="true">
                    <path d="M1 7.775V2.75C1 1.784 1.784 1 2.75 1h5.025c.464 0 .91.184 1.238.513l6.25 6.25a1.75 1.75 0 0 1 0 2.474l-5.026 5.026a1.75 1.75 0 0 1-2.474 0l-6.25-6.25A1.752 1.752 0 0 1 1 7.775Zm1.5 0c0 .066.026.13.073.177l6.25 6.25a.25.25 0 0 0 .354 0l5.025-5.025a.25.25 0 0 0 0-.354l-6.25-6.25a.25.25 0 0 0-.177-.073H2.75a.25.25 0 0 0-.25.25ZM6 5a1 1 0 1 1 0 2 1 1 0 0 1 0-2Z"/>
                  </svg>
                  Untagged
                  <span class="text-[var(--color-muted)] font-normal">{{ uncategorizedCount() }}</span>
                </h2>
                <ng-container [ngTemplateOutlet]="noteGrid" [ngTemplateOutletContext]="{ notes: uncategorizedNotes() }"></ng-container>
              </section>
            }

            @if (notes().length === 0) {
              <div class="flex flex-col items-center justify-center py-24 text-center">
                <svg class="text-[var(--color-muted)] mb-4" width="48" height="48" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                  <path d="M2 1.75A1.75 1.75 0 0 1 3.75 0h6.586c.464 0 .909.184 1.237.513l2.914 2.914c.329.328.513.773.513 1.237v9.586A1.75 1.75 0 0 1 13.25 16h-9.5A1.75 1.75 0 0 1 2 14.25Z"/>
                </svg>
                <p class="text-sm text-[var(--color-muted)] mb-4">還沒有任何筆記</p>
                <button
                  (click)="createNote()"
                  class="bg-[var(--color-accent)] text-white text-sm px-4 py-2 rounded hover:opacity-90 transition-opacity"
                >
                  建立第一篇筆記
                </button>
              </div>
            }

          } @else {
            <!-- Filtered: specific category or untagged -->
            @if (displayedNotes().length === 0) {
              <div class="flex flex-col items-center justify-center py-24 text-center">
                <p class="text-sm text-[var(--color-muted)]">這裡還沒有筆記</p>
              </div>
            } @else {
              <ng-container [ngTemplateOutlet]="noteGrid" [ngTemplateOutletContext]="{ notes: displayedNotes() }"></ng-container>
            }
          }
        </div>
      </main>
    </div>

    <!-- Note grid template -->
    <ng-template #noteGrid let-notes="notes">
      @if (viewMode() === 'grid') {
        <div class="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
          @for (note of notes; track note.noteId) {
            <div class="relative border border-[var(--color-border)] bg-[var(--color-surface)] rounded hover:border-[var(--color-accent)] hover:bg-[var(--color-surface-2)] transition-colors group">
              <a [routerLink]="['/notes', note.noteId]" class="block p-4 min-h-[110px]">
                <svg class="text-[var(--color-muted)] mb-3" width="20" height="20" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                  <path d="M2 1.75A1.75 1.75 0 0 1 3.75 0h6.586c.464 0 .909.184 1.237.513l2.914 2.914c.329.328.513.773.513 1.237v9.586A1.75 1.75 0 0 1 13.25 16h-9.5A1.75 1.75 0 0 1 2 14.25Z"/>
                </svg>
                <div class="text-sm font-medium text-[var(--color-text)] line-clamp-2 mb-2">
                  {{ note.title || '（無標題）' }}
                </div>
                <div class="text-xs text-[var(--color-muted)]">{{ formatDate(note.updatedAt) }}</div>
              </a>
              <button
                (click)="confirmDelete(note)"
                class="absolute top-2 right-2 opacity-0 group-hover:opacity-100 text-[var(--color-muted)] hover:text-[var(--color-danger)] transition-all p-1"
                aria-label="刪除"
              >
                <svg width="12" height="12" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                  <path d="M11 1.75V3h2.25a.75.75 0 0 1 0 1.5H2.75a.75.75 0 0 1 0-1.5H5V1.75C5 .784 5.784 0 6.75 0h2.5C10.216 0 11 .784 11 1.75ZM4.496 6.675l.66 6.6a.25.25 0 0 0 .249.225h5.19a.25.25 0 0 0 .249-.225l.66-6.6a.75.75 0 0 1 1.492.149l-.66 6.6A1.748 1.748 0 0 1 10.595 15h-5.19a1.75 1.75 0 0 1-1.741-1.575l-.66-6.6a.75.75 0 1 1 1.492-.15Z"/>
                </svg>
              </button>
            </div>
          }
        </div>
      } @else {
        <div class="divide-y divide-[var(--color-border)] border border-[var(--color-border)] rounded overflow-hidden">
          @for (note of notes; track note.noteId) {
            <div class="flex items-center gap-3 px-4 py-3 hover:bg-[var(--color-surface-2)] transition-colors group">
              <svg class="text-[var(--color-muted)] shrink-0" width="14" height="14" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                <path d="M2 1.75A1.75 1.75 0 0 1 3.75 0h6.586c.464 0 .909.184 1.237.513l2.914 2.914c.329.328.513.773.513 1.237v9.586A1.75 1.75 0 0 1 13.25 16h-9.5A1.75 1.75 0 0 1 2 14.25Z"/>
              </svg>
              <a [routerLink]="['/notes', note.noteId]" class="flex-1 min-w-0">
                <span class="text-sm text-[var(--color-text)] truncate block">{{ note.title || '（無標題）' }}</span>
              </a>
              <span class="text-xs text-[var(--color-muted)] shrink-0">{{ formatDate(note.updatedAt) }}</span>
              <button
                (click)="confirmDelete(note)"
                class="opacity-0 group-hover:opacity-100 text-[var(--color-muted)] hover:text-[var(--color-danger)] transition-all p-1 shrink-0"
                aria-label="刪除"
              >
                <svg width="12" height="12" viewBox="0 0 16 16" fill="currentColor" aria-hidden="true">
                  <path d="M11 1.75V3h2.25a.75.75 0 0 1 0 1.5H2.75a.75.75 0 0 1 0-1.5H5V1.75C5 .784 5.784 0 6.75 0h2.5C10.216 0 11 .784 11 1.75ZM4.496 6.675l.66 6.6a.25.25 0 0 0 .249.225h5.19a.25.25 0 0 0 .249-.225l.66-6.6a.75.75 0 0 1 1.492.149l-.66 6.6A1.748 1.748 0 0 1 10.595 15h-5.19a1.75 1.75 0 0 1-1.741-1.575l-.66-6.6a.75.75 0 1 1 1.492-.15Z"/>
                </svg>
              </button>
            </div>
          }
        </div>
      }
    </ng-template>

    <!-- Delete note dialog -->
    <app-dialog
      [open]="deleteDialogOpen()"
      title="刪除筆記"
      [message]="'確定要刪除「' + (noteToDelete()?.title || '無標題') + '」？此操作無法復原。'"
      confirmLabel="刪除"
      confirmVariant="danger"
      (confirm)="deleteNote()"
      (cancel)="deleteDialogOpen.set(false)"
    />

    <!-- Delete category dialog -->
    <app-dialog
      [open]="deleteCategoryDialogOpen()"
      title="刪除分類"
      [message]="'確定要刪除「' + (categoryToDelete()?.name ?? '') + '」？該分類下的筆記將變為未分類。'"
      confirmLabel="刪除"
      confirmVariant="danger"
      (confirm)="executeDeleteCategory()"
      (cancel)="deleteCategoryDialogOpen.set(false)"
    />
  `,
})
export class NoteListComponent implements OnInit {
  private readonly noteService = inject(NoteService);
  private readonly categoryService = inject(CategoryService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  readonly auth = inject(AuthService);

  readonly sidebar = createResizable({ defaultWidth: 240, min: 160, max: 480, direction: 'right' });
  readonly notes = signal<NoteSummary[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly loading = signal(true);
  readonly creating = signal(false);
  readonly selectedCategory = signal<string | null>(null);
  readonly viewMode = signal<'grid' | 'list'>('grid');
  readonly searchQuery = signal('');
  readonly showNewCategoryInput = signal(false);
  readonly newCategoryName = signal('');
  readonly deleteDialogOpen = signal(false);
  readonly noteToDelete = signal<NoteSummary | null>(null);
  readonly renamingCategoryId = signal<string | null>(null);
  renameCategoryName = '';
  readonly deleteCategoryDialogOpen = signal(false);
  readonly categoryToDelete = signal<Category | null>(null);

  @ViewChild('renameInput') renameInputRef?: ElementRef<HTMLInputElement>;

  readonly categoryCounts = computed(() => {
    const counts = new Map<string, number>();
    for (const note of this.notes()) {
      if (note.categoryId) {
        counts.set(note.categoryId, (counts.get(note.categoryId) ?? 0) + 1);
      }
    }
    return counts;
  });

  readonly uncategorizedCount = computed(() => this.notes().filter(n => !n.categoryId).length);

  readonly uncategorizedNotes = computed(() =>
    this.notes()
      .filter(n => !n.categoryId)
      .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
  );

  readonly displayedNotes = computed(() => {
    const cat = this.selectedCategory();
    const query = this.searchQuery().toLowerCase();
    let notes = this.notes();

    if (cat === '__none') {
      notes = notes.filter(n => !n.categoryId);
    } else if (cat) {
      notes = notes.filter(n => n.categoryId === cat);
    }

    if (query) {
      notes = notes.filter(n => n.title.toLowerCase().includes(query));
    }

    return notes.sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime());
  });

  readonly pageTitle = computed(() => {
    const cat = this.selectedCategory();
    if (cat === null) return 'My workspace';
    if (cat === '__none') return 'Untagged';
    return this.categories().find(c => c.id === cat)?.name ?? 'My workspace';
  });

  readonly userInitial = computed(() =>
    this.auth.currentUser()?.username?.slice(0, 1)?.toUpperCase() ?? '?'
  );

  async ngOnInit() {
    const cat = this.route.snapshot.queryParamMap.get('cat');
    if (cat) this.selectedCategory.set(cat);

    const [notes, categories] = await Promise.all([
      this.noteService.list(),
      this.categoryService.list(),
    ]);
    this.notes.set(notes);
    this.categories.set(categories);
    this.loading.set(false);
  }

  isSelected(id: string | null): boolean {
    return this.selectedCategory() === id && !this.searchQuery();
  }

  selectCategory(id: string | null) {
    this.selectedCategory.set(id);
    this.searchQuery.set('');
    this.router.navigate([], {
      queryParams: { cat: id ?? null },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  async createNote() {
    this.creating.set(true);
    try {
      const { noteId } = await this.noteService.create();
      this.router.navigate(['/notes', noteId]);
    } catch {
      this.toast.error('建立失敗');
      this.creating.set(false);
    }
  }

  async createNoteInCategory() {
    const categoryId = this.selectedCategory();
    if (!categoryId || categoryId === '__none') return;
    this.creating.set(true);
    try {
      const { noteId } = await this.noteService.create();
      await this.noteService.update(noteId, { categoryId });
      this.router.navigate(['/notes', noteId]);
    } catch {
      this.toast.error('建立失敗');
      this.creating.set(false);
    }
  }

  async onAvatarChange(e: Event) {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;
    try {
      await this.auth.updateAvatar(file);
      this.toast.success('頭貼已更新');
    } catch {
      this.toast.error('頭貼上傳失敗');
    }
    (e.target as HTMLInputElement).value = '';
  }

  async createCategory() {
    const name = this.newCategoryName().trim();
    if (!name) return;
    try {
      const cat = await this.categoryService.create(name);
      this.categories.update(cats => [...cats, cat]);
      this.newCategoryName.set('');
      this.showNewCategoryInput.set(false);
      this.selectCategory(cat.id);
      this.toast.success('分類已建立');
    } catch {
      this.toast.error('建立分類失敗');
    }
  }

  cancelNewCategory() {
    this.newCategoryName.set('');
    this.showNewCategoryInput.set(false);
  }

  startRename(cat: Category) {
    this.renamingCategoryId.set(cat.id);
    this.renameCategoryName = cat.name;
    setTimeout(() => this.renameInputRef?.nativeElement?.focus(), 0);
  }

  async confirmRename(id: string) {
    const name = this.renameCategoryName.trim();
    if (!name) { this.cancelRename(); return; }
    try {
      await this.categoryService.update(id, name);
      this.categories.update(cats => cats.map(c => c.id === id ? { ...c, name } : c));
      this.toast.success('分類已重新命名');
    } catch {
      this.toast.error('重新命名失敗');
    }
    this.renamingCategoryId.set(null);
  }

  cancelRename() {
    this.renamingCategoryId.set(null);
    this.renameCategoryName = '';
  }

  confirmDeleteCategory(cat: Category) {
    this.categoryToDelete.set(cat);
    this.deleteCategoryDialogOpen.set(true);
  }

  async executeDeleteCategory() {
    const cat = this.categoryToDelete();
    if (!cat) return;
    try {
      await this.categoryService.delete(cat.id);
      this.categories.update(cs => cs.filter(c => c.id !== cat.id));
      this.notes.update(ns => ns.map(n => n.categoryId === cat.id ? { ...n, categoryId: undefined } : n));
      if (this.selectedCategory() === cat.id) this.selectedCategory.set(null);
      this.toast.success('分類已刪除');
    } catch (err: unknown) {
      const status = (err as { status?: number })?.status;
      if (status === 409) {
        this.toast.error('分類下還有筆記，請先將筆記移到其他分類或移除分類');
      } else {
        this.toast.error('刪除失敗');
      }
    } finally {
      this.deleteCategoryDialogOpen.set(false);
      this.categoryToDelete.set(null);
    }
  }

  confirmDelete(note: NoteSummary) {
    this.noteToDelete.set(note);
    this.deleteDialogOpen.set(true);
  }

  async deleteNote() {
    const note = this.noteToDelete();
    if (!note) return;
    try {
      await this.noteService.delete(note.noteId);
      this.notes.update(n => n.filter(x => x.noteId !== note.noteId));
      this.toast.success('已刪除');
    } catch {
      this.toast.error('刪除失敗');
    } finally {
      this.deleteDialogOpen.set(false);
      this.noteToDelete.set(null);
    }
  }

  formatDate(iso: string): string {
    const now = new Date();
    const d = new Date(iso);
    const diffMs = now.getTime() - d.getTime();
    const diffDays = Math.floor(diffMs / 86400000);
    if (diffDays === 0) {
      const diffHours = Math.floor(diffMs / 3600000);
      if (diffHours === 0) return 'just now';
      return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    }
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
}

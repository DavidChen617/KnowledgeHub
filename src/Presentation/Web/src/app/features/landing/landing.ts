import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';

const FEATURES = [
  { icon: '◈', title: 'Markdown 筆記', desc: 'CodeMirror 6 編輯器，syntax highlight，所見即所得' },
  { icon: '◉', title: '知識圖譜', desc: '筆記之間的連結關係，視覺化呈現知識結構' },
  { icon: '◎', title: 'AI 結構化', desc: '一鍵讓 AI 整理你的筆記' },
  { icon: '◌', title: '分享與留言', desc: '產生公開連結，讓他人檢視筆記並留言互動' },
];

@Component({
  selector: 'landing',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `
    <div class="min-h-screen bg-[var(--color-bg)] relative overflow-hidden">

      <!-- Background grid -->
      <div class="absolute inset-0 opacity-[0.03]"
        style="background-image: linear-gradient(var(--color-text) 1px, transparent 1px), linear-gradient(90deg, var(--color-text) 1px, transparent 1px); background-size: 40px 40px;">
      </div>

      <!-- Accent glow -->
      <div class="absolute top-0 left-1/2 -translate-x-1/2 w-[600px] h-[300px] opacity-10"
        style="background: radial-gradient(ellipse at center, var(--color-accent) 0%, transparent 70%);">
      </div>

      <div class="relative z-10 max-w-4xl mx-auto px-6 py-24">

        <!-- Nav -->
        <nav class="flex items-center justify-between mb-28 page-enter">
          <span class="font-display text-sm font-bold tracking-widest text-[var(--color-accent)] uppercase">
            KnowledgeHub
          </span>
          <a
            routerLink="/login"
            class="font-mono text-xs text-[var(--color-muted)] border border-[var(--color-border)] px-3 py-1.5 hover:border-[var(--color-muted)] hover:text-[var(--color-text)] transition-all duration-150"
          >
            登入 →
          </a>
        </nav>

        <!-- Hero -->
        <div class="mb-24 page-enter-stagger">
          <div class="text-[var(--color-muted)] font-mono text-xs tracking-widest uppercase mb-4">
            Personal Knowledge Management
          </div>
          <h1 class="font-display text-5xl md:text-7xl font-extrabold text-[var(--color-text)] leading-[0.95] tracking-tight mb-6">
            <span class="text-[var(--color-accent)]">An AI-Driven</span><br/>
            Smart Notes Platform
          </h1>
          <p class="font-mono text-[var(--color-muted)] text-sm max-w-md leading-relaxed mb-10">
            寫下筆記，建立連結，讓 AI 整理結構。<br/>
            知識不再是孤立的片段，而是有脈絡的網絡。
          </p>
          <a
            routerLink="/login"
            class="inline-flex items-center gap-2 bg-[var(--color-accent)] text-[var(--color-bg)] font-mono text-sm px-6 py-3 hover:bg-[var(--color-accent-dim)] transition-colors duration-150 font-semibold"
          >
            開始使用
            <span aria-hidden="true">→</span>
          </a>
        </div>

        <!-- Divider -->
        <div class="border-t border-[var(--color-border)] mb-16"></div>

        <!-- Features -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-px bg-[var(--color-border)] page-enter-stagger">
          @for (f of features; track f.title) {
            <div class="bg-[var(--color-bg)] p-8 hover:bg-[var(--color-surface)] transition-colors duration-200">
              <div class="text-[var(--color-accent)] text-xl mb-3 font-mono">{{ f.icon }}</div>
              <h3 class="font-display text-sm font-semibold text-[var(--color-text)] mb-2 uppercase tracking-wide">
                {{ f.title }}
              </h3>
              <p class="font-mono text-xs text-[var(--color-muted)] leading-relaxed">{{ f.desc }}</p>
            </div>
          }
        </div>

        <!-- Footer -->
        <div class="mt-24 flex items-center justify-between text-[var(--color-muted)] font-mono text-xs">
          <span>KnowledgeHub</span>
          <span>Build in public</span>
        </div>

      </div>
    </div>
  `,
})
export class LandingComponent {
  readonly features = FEATURES;
}

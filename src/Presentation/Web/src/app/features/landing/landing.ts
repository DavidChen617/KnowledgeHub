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
    <div class="min-h-screen bg-[var(--color-bg)] relative overflow-hidden" (mousemove)="onMouseMove($event)">

      <!-- Mouse spotlight -->
      <div class="absolute inset-0 pointer-events-none" aria-hidden="true"
        style="background: radial-gradient(600px circle at var(--mouse-x, -200px) var(--mouse-y, -200px), var(--mouse-color, rgba(83,155,245,0.06)), transparent 60%);">
      </div>

      <!-- Floating orbs -->
      <div class="absolute top-[-150px] left-[-100px] w-[500px] h-[500px] rounded-full pointer-events-none orb-1"
        style="background: radial-gradient(circle, rgba(83,155,245,0.13) 0%, transparent 70%);" aria-hidden="true">
      </div>
      <div class="absolute top-[300px] right-[-200px] w-[600px] h-[600px] rounded-full pointer-events-none orb-2"
        style="background: radial-gradient(circle, rgba(129,140,248,0.09) 0%, transparent 70%);" aria-hidden="true">
      </div>
      <div class="absolute bottom-0 left-[20%] w-[400px] h-[400px] rounded-full pointer-events-none orb-3"
        style="background: radial-gradient(circle, rgba(52,211,153,0.07) 0%, transparent 70%);" aria-hidden="true">
      </div>

      <!-- Grid -->
      <div class="absolute inset-0 opacity-[0.025] pointer-events-none" aria-hidden="true"
        style="background-image: linear-gradient(var(--color-text) 1px, transparent 1px), linear-gradient(90deg, var(--color-text) 1px, transparent 1px); background-size: 40px 40px;">
      </div>

      <div class="relative z-10 max-w-5xl mx-auto px-6 py-20">

        <!-- Nav -->
        <nav class="flex items-center justify-between mb-24 page-enter" aria-label="主導覽">
          <span class="font-display text-sm font-bold tracking-widest text-[var(--color-accent)] uppercase">
            KnowledgeHub
          </span>
          <a
            routerLink="/login"
            class="font-mono text-xs text-[var(--color-muted)] border border-[var(--color-border)] px-3 py-1.5 hover:border-[var(--color-muted)] hover:text-[var(--color-text)] transition-all duration-150"
          >登入 →</a>
        </nav>

        <!-- Hero -->
        <section class="mb-28 page-enter-stagger" aria-labelledby="hero-heading">

          <!-- Headline -->
          <h1 id="hero-heading" class="font-display text-5xl md:text-7xl font-black text-[var(--color-text)] leading-[0.95] tracking-tight mb-6">
            <span class="gradient-text-animated">An AI-Driven</span><br/>
            Smart Notes Platform
          </h1>

          <!-- Description -->
          <p class="font-mono text-[var(--color-muted)] text-sm max-w-lg leading-relaxed mb-10">
            整合 Markdown 編輯、AI 筆記整理、知識圖譜視覺化與即時分享，<br/>
            讓知識不再是孤立的片段，而是有脈絡的網絡。
          </p>

          <!-- CTA -->
          <a
            routerLink="/login"
            class="inline-flex items-center gap-2 bg-[var(--color-accent)] text-[var(--color-bg)] font-mono text-sm px-7 py-3.5 hover:bg-[var(--color-accent-dim)] transition-colors duration-150 font-semibold"
          >
            開始體驗
            <span aria-hidden="true">→</span>
          </a>
        </section>

        <!-- Features -->
        <section class="mb-16 page-enter-stagger" aria-labelledby="features-heading">
          <h2 id="features-heading" class="font-mono text-xs text-[var(--color-muted)] uppercase tracking-widest mb-6">核心功能</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-px bg-[var(--color-border)]">
            @for (f of features; track f.title) {
              <div class="bg-[var(--color-bg)] p-8 hover:bg-[var(--color-surface)] transition-colors duration-200 group">
                <div class="font-mono text-[var(--color-accent)] text-2xl mb-4" aria-hidden="true">{{ f.icon }}</div>
                <h3 class="font-display text-sm font-bold text-[var(--color-text)] mb-2 uppercase tracking-wide group-hover:text-[var(--color-accent)] transition-colors duration-150">
                  {{ f.title }}
                </h3>
                <p class="font-mono text-xs text-[var(--color-muted)] leading-relaxed">{{ f.desc }}</p>
              </div>
            }
          </div>
        </section>

        <!-- Footer -->
        <footer class="flex items-center justify-between text-[var(--color-muted)] font-mono text-xs border-t border-[var(--color-border)] pt-6">
          <span>KnowledgeHub</span>

        </footer>

      </div>
    </div>
  `,
})
export class LandingComponent {
  readonly features = FEATURES;

  onMouseMove(e: MouseEvent) {
    const el = e.currentTarget as HTMLElement;
    const xRatio = e.clientX / window.innerWidth;
    const yRatio = e.clientY / window.innerHeight;
    const hue = Math.round(210 + xRatio * 110);       // 210 藍 → 320 粉紫
    const lightness = Math.round(55 + yRatio * 15);   // 上方亮 → 下方稍暗
    el.style.setProperty('--mouse-x', `${e.clientX}px`);
    el.style.setProperty('--mouse-y', `${e.clientY}px`);
    el.style.setProperty('--mouse-color', `hsla(${hue}, 80%, ${lightness}%, 0.08)`);
  }
}

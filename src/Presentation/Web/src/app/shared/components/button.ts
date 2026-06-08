import { Component, input, ChangeDetectionStrategy } from '@angular/core';

export type ButtonVariant = 'primary' | 'ghost' | 'danger';

@Component({
  selector: 'app-button',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'contents' },
  template: `
    <button
      [type]="type()"
      [disabled]="disabled() || loading()"
      [class]="buttonClass()"
    >
      @if (loading()) {
        <span class="inline-block w-3 h-3 border border-current border-t-transparent rounded-full animate-spin"></span>
      }
      <ng-content />
    </button>
  `,
})
export class ButtonComponent {
  readonly variant = input<ButtonVariant>('primary');
  readonly type = input<'button' | 'submit' | 'reset'>('button');
  readonly disabled = input(false);
  readonly loading = input(false);
  readonly size = input<'sm' | 'md'>('md');

  buttonClass() {
    const base = 'inline-flex items-center gap-2 font-mono text-sm transition-all duration-150 cursor-pointer border disabled:opacity-40 disabled:cursor-not-allowed focus-visible:outline focus-visible:outline-1 focus-visible:outline-[var(--color-accent)]';
    const sizes: Record<string, string> = {
      sm: 'px-3 py-1.5 text-xs',
      md: 'px-4 py-2',
    };
    const variants: Record<ButtonVariant, string> = {
      primary: 'bg-[var(--color-accent)] text-[var(--color-bg)] border-transparent hover:bg-[var(--color-accent-dim)]',
      ghost: 'bg-transparent text-[var(--color-text)] border-[var(--color-border)] hover:border-[var(--color-muted)] hover:text-white',
      danger: 'bg-transparent text-[var(--color-danger)] border-[var(--color-danger)] hover:bg-[var(--color-danger)] hover:text-[var(--color-bg)]',
    };
    return `${base} ${sizes[this.size()]} ${variants[this.variant()]}`;
  }
}

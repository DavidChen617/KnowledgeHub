import { Component, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { Overlay, OverlayRef } from '@angular/cdk/overlay';
import { ComponentPortal } from '@angular/cdk/portal';

export type ToastType = 'success' | 'error' | 'info';

export interface Toast {
  message: string;
  type: ToastType;
}

@Component({
  selector: 'toast',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="fixed bottom-6 right-6 z-50 flex flex-col gap-2 pointer-events-none">
      @for (toast of toasts(); track $index) {
        <div
          role="alert"
          class="panel-enter flex items-center gap-3 px-4 py-3 border font-mono text-sm pointer-events-auto"
          [class]="toastClass(toast.type)"
        >
          <span>{{ toastIcon(toast.type) }}</span>
          <span>{{ toast.message }}</span>
        </div>
      }
    </div>
  `,
})
export class ToastComponent {
  readonly toasts = signal<Toast[]>([]);

  add(toast: Toast, duration = 3500) {
    this.toasts.update(t => [...t, toast]);
    setTimeout(() => {
      this.toasts.update(t => t.filter(x => x !== toast));
    }, duration);
  }

  toastClass(type: ToastType) {
    const map: Record<ToastType, string> = {
      success: 'bg-[var(--color-surface)] border-[var(--color-accent)] text-[var(--color-accent)]',
      error: 'bg-[var(--color-surface)] border-[var(--color-danger)] text-[var(--color-danger)]',
      info: 'bg-[var(--color-surface)] border-[var(--color-border)] text-[var(--color-text)]',
    };
    return map[type];
  }

  toastIcon(type: ToastType) {
    return { success: '✓', error: '✕', info: '·' }[type];
  }
}

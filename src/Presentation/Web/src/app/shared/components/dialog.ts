import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';
import { ButtonComponent } from './button';

@Component({
  selector: 'app-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ButtonComponent],
  template: `
    @if (open()) {
      <div
        role="dialog"
        [attr.aria-modal]="true"
        [attr.aria-labelledby]="'dialog-title'"
        class="fixed inset-0 z-50 flex items-center justify-center"
      >
        <div
          class="absolute inset-0 bg-black/60 overlay-enter"
          (click)="cancel.emit()"
        ></div>
        <div class="relative bg-[var(--color-surface)] border border-[var(--color-border)] p-6 w-full max-w-sm panel-enter">
          <h2 id="dialog-title" class="font-display text-base font-semibold text-[var(--color-text)] mb-2">
            {{ title() }}
          </h2>
          <p class="text-[var(--color-muted)] text-sm mb-6">{{ message() }}</p>
          <div class="flex gap-3 justify-end">
            <app-button variant="ghost" size="sm" (click)="cancel.emit()">取消</app-button>
            <app-button [variant]="confirmVariant()" size="sm" (click)="confirm.emit()">
              {{ confirmLabel() }}
            </app-button>
          </div>
        </div>
      </div>
    }
  `,
})
export class DialogComponent {
  readonly open = input(false);
  readonly title = input('確認');
  readonly message = input('');
  readonly confirmLabel = input('確認');
  readonly confirmVariant = input<'primary' | 'danger'>('primary');
  readonly confirm = output<void>();
  readonly cancel = output<void>();
}

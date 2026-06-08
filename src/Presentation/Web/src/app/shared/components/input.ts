import { Component, input, output, model, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-input',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule],
  host: { class: 'contents' },
  template: `
    <input
      [type]="type()"
      [placeholder]="placeholder()"
      [disabled]="disabled()"
      [(ngModel)]="value"
      (ngModelChange)="valueChange.emit($event)"
      class="w-full bg-[var(--color-surface)] border border-[var(--color-border)] text-[var(--color-text)] font-mono text-sm px-3 py-2 outline-none transition-colors duration-150 focus:border-[var(--color-accent)] placeholder:text-[var(--color-muted)] disabled:opacity-40"
    />
  `,
})
export class InputComponent {
  readonly type = input<string>('text');
  readonly placeholder = input('');
  readonly disabled = input(false);
  readonly value = model('');
  readonly valueChange = output<string>();
}

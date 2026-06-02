import { Component, input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'spinner',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span
      role="status"
      [attr.aria-label]="label()"
      [class]="'inline-block border-2 border-current border-t-transparent rounded-full animate-spin ' + sizeClass()"
    ></span>
  `,
})
export class SpinnerComponent {
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly label = input('Loading...');

  sizeClass() {
    return { sm: 'w-3 h-3', md: 'w-5 h-5', lg: 'w-8 h-8' }[this.size()];
  }
}

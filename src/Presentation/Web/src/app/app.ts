import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SearchOverlayComponent } from './features/search/search-overlay';

@Component({
  selector: 'app-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, SearchOverlayComponent],
  template: `
    <router-outlet />
    <search-overlay />
  `,
})
export class App {}

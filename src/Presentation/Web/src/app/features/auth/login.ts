import { Component, OnInit, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { SpinnerComponent } from '../../shared/components/spinner';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'login',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [SpinnerComponent],
  template: `
    <div class="min-h-screen bg-[var(--color-bg)] flex items-center justify-center page-enter">
      <div class="w-full max-w-sm px-6">
        <div class="mb-10 text-center">
          <h1 class="font-display text-2xl font-bold text-[var(--color-text)] tracking-tight mb-1">
            KnowledgeHub
          </h1>
          <p class="text-[var(--color-muted)] text-sm">你的知識，有連結地存在</p>
        </div>

        @if (loading()) {
          <div class="flex justify-center">
            <spinner size="lg" />
          </div>
        } @else if (error()) {
          <div class="border border-[var(--color-danger)] text-[var(--color-danger)] text-sm p-3 mb-4 font-mono">
            {{ error() }}
          </div>
        }

        <button
          (click)="loginWithGoogle()"
          [disabled]="loading()"
          class="w-full flex items-center justify-center gap-3 bg-[var(--color-surface)] border border-[var(--color-border)] text-[var(--color-text)] font-mono text-sm px-4 py-3 transition-all duration-150 hover:border-[var(--color-muted)] disabled:opacity-40 disabled:cursor-not-allowed focus-visible:outline focus-visible:outline-1 focus-visible:outline-[var(--color-accent)]"
        >
          <svg width="18" height="18" viewBox="0 0 18 18" aria-hidden="true">
            <path fill="#4285F4" d="M16.51 8H8.98v3h4.3c-.18 1-.74 1.48-1.6 2.04v2.01h2.6a7.8 7.8 0 002.38-5.88c0-.57-.05-.66-.15-1.18z"/>
            <path fill="#34A853" d="M8.98 17c2.16 0 3.97-.72 5.3-1.94l-2.6-2a4.8 4.8 0 01-7.18-2.54H1.83v2.07A8 8 0 008.98 17z"/>
            <path fill="#FBBC05" d="M4.5 10.52a4.8 4.8 0 010-3.04V5.41H1.83a8 8 0 000 7.18l2.67-2.07z"/>
            <path fill="#EA4335" d="M8.98 4.18c1.17 0 2.23.4 3.06 1.2l2.3-2.3A8 8 0 001.83 5.4L4.5 7.49a4.77 4.77 0 014.48-3.3z"/>
          </svg>
          使用 Google 帳號登入
        </button>
      </div>
    </div>
  `,
})
export class LoginComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    // Google returns id_token in URL fragment after redirect
    const hash = window.location.hash.slice(1);
    if (hash) {
      const params = new URLSearchParams(hash);
      const idToken = params.get('id_token');
      if (idToken) {
        window.history.replaceState(null, '', new URL('login', document.baseURI).pathname);
        this.loading.set(true);
        this.auth.exchangeGoogleToken(idToken)
          .then(() => this.router.navigate(['/notes']))
          .catch(() => {
            this.error.set('登入失敗，請再試一次');
            this.loading.set(false);
          });
      }
    }
  }

  loginWithGoogle(): void {
    const nonce = crypto.randomUUID();
    sessionStorage.setItem('google_nonce', nonce);
    const params = new URLSearchParams({
      client_id: environment.googleClientId,
      redirect_uri: new URL('login', document.baseURI).href,
      response_type: 'id_token',
      scope: 'openid email profile',
      nonce,
    });
    window.location.href = `https://accounts.google.com/o/oauth2/v2/auth?${params}`;
  }
}

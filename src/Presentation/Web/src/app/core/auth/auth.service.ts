import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface CurrentUser {
  id: string;
  username: string;
  email: string;
  avatarUrl?: string;
}

const REFRESH_TOKEN_KEY = 'kh_refresh_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _accessToken = signal<string | null>(null);
  private readonly _user = signal<CurrentUser | null>(null);

  readonly isLoggedIn = computed(() => this._accessToken() !== null);
  readonly currentUser = this._user.asReadonly();
  readonly accessToken = this._accessToken.asReadonly();

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  async exchangeGoogleToken(idToken: string): Promise<void> {
    const baseUrl = window.location.origin;
    const res = await firstValueFrom(
      this.http.post<{ data: TokenResponse }>(`${environment.apiUrl}/oauth/google/token`, { idToken, baseUrl })
    );
    this.setTokens(res.data);
    await this.loadCurrentUser();
  }

  async refresh(): Promise<boolean> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) return false;
    try {
      const res = await firstValueFrom(
        this.http.post<{ data: TokenResponse }>(`${environment.apiUrl}/oauth/refresh`, { refreshToken })
      );
      this.setTokens(res.data);
      return true;
    } catch {
      this.logout();
      return false;
    }
  }

  async loadCurrentUser(): Promise<void> {
    try {
      const res = await firstValueFrom(
        this.http.get<{ data: CurrentUser }>(`${environment.apiUrl}/api/users/me`)
      );
      this._user.set(res.data);
    } catch {
      this.logout();
    }
  }

  async updateAvatar(file: File): Promise<string> {
    const form = new FormData();
    form.append('file', file);
    const res = await firstValueFrom(
      this.http.patch<{ data: { avatarUrl: string } }>(`${environment.apiUrl}/api/users/me/avatar`, form)
    );
    const url = res.data.avatarUrl;
    this._user.update(u => u ? { ...u, avatarUrl: url } : u);
    return url;
  }

  logout(): void {
    this._accessToken.set(null);
    this._user.set(null);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    this.router.navigate(['/']);
  }

  private setTokens(tokens: TokenResponse): void {
    this._accessToken.set(tokens.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, tokens.refreshToken);
  }
}

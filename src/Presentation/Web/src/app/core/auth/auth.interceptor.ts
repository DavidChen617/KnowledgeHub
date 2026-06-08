import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const auth = inject(AuthService);
  const token = auth.accessToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 403 && auth.getRefreshToken()) {
        return from(auth.refresh()).pipe(
          switchMap(success => {
            if (!success) return throwError(() => err);
            const retryReq = req.clone({
              setHeaders: { Authorization: `Bearer ${auth.accessToken()}` },
            });
            return next(retryReq);
          })
        );
      }
      return throwError(() => err);
    })
  );
};

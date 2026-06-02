import { Injectable, ApplicationRef, createComponent, EnvironmentInjector, signal } from '@angular/core';
import { ToastComponent, ToastType } from '../components/toast';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private component?: ToastComponent;

  constructor(
    private appRef: ApplicationRef,
    private injector: EnvironmentInjector,
  ) {
    const ref = createComponent(ToastComponent, { environmentInjector: this.injector });
    this.appRef.attachView(ref.hostView);
    document.body.appendChild(ref.location.nativeElement);
    this.component = ref.instance;
  }

  success(message: string) {
    this.component?.add({ message, type: 'success' });
  }

  error(message: string) {
    this.component?.add({ message, type: 'error' });
  }

  info(message: string) {
    this.component?.add({ message, type: 'info' });
  }
}

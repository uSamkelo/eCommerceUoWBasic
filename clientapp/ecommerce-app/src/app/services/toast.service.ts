import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ToastMessage {
  id: number;
  kind: 'error' | 'success';
  message: string;
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private readonly toastSubject = new BehaviorSubject<ToastMessage | null>(null);
  readonly toast$ = this.toastSubject.asObservable();
  private timeoutId: ReturnType<typeof setTimeout> | undefined;
  private nextId = 0;

  error(message: string, duration = 6000): void {
    this.show('error', message, duration);
  }

  success(message: string, duration = 6000): void {
    this.show('success', message, duration);
  }

  dismiss(): void {
    if (this.timeoutId !== undefined) {
      clearTimeout(this.timeoutId);
      this.timeoutId = undefined;
    }

    this.toastSubject.next(null);
  }

  private show(kind: ToastMessage['kind'], message: string, duration: number): void {
    this.dismiss();
    this.toastSubject.next({ id: ++this.nextId, kind, message });
    this.timeoutId = setTimeout(() => this.dismiss(), duration);
  }
}

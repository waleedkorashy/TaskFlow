import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly toastsSignal = signal<Toast[]>([]);
  readonly toasts = this.toastsSignal.asReadonly();

  private nextId = 1;

  show(message: string, type: Toast['type'] = 'success', duration = 4000): void {
    const toast: Toast = { id: this.nextId++, message, type };
    this.toastsSignal.update(list => [...list, toast]);

    if (duration > 0) {
      setTimeout(() => this.dismiss(toast.id), duration);
    }
  }

  success(message: string, duration = 4000): void {
    this.show(message, 'success', duration);
  }

  error(message: string, duration = 6000): void {
    this.show(message, 'error', duration);
  }

  warning(message: string, duration = 5000): void {
    this.show(message, 'warning', duration);
  }

  info(message: string, duration = 4000): void {
    this.show(message, 'info', duration);
  }

  dismiss(id: number): void {
    this.toastsSignal.update(list => list.filter(t => t.id !== id));
  }
}
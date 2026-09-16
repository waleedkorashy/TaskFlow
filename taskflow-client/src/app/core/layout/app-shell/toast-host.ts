import { Component } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'tf-toast-host',
  imports: [MatIcon],
  standalone: true,
  host: { class: 'tf-toast-host' },
  template: `
    @for (toast of toastService.toasts(); track toast.id) {
      <div class="tf-toast tf-toast-{{ toast.type }}" role="status">
        <mat-icon class="tf-toast-icon" aria-hidden="true">{{ iconFor(toast.type) }}</mat-icon>
        <span class="tf-toast-msg">{{ toast.message }}</span>
        <button
          type="button"
          class="tf-icon-btn tf-toast-close"
          (click)="toastService.dismiss(toast.id)"
          aria-label="Dismiss notification"
        >
          <mat-icon aria-hidden="true">close</mat-icon>
        </button>
      </div>
    }
  `,
})
export class ToastHost {
  constructor(readonly toastService: ToastService) {}

  protected iconFor(type: string): string {
    switch (type) {
      case 'success':
        return 'check_circle';
      case 'error':
        return 'error';
      case 'warning':
        return 'warning';
      default:
        return 'info';
    }
  }
}
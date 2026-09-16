import { Component, input, output } from '@angular/core';

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  danger?: boolean;
}

@Component({
  selector: 'tf-confirm-dialog',
  imports: [],
  host: {
    class: 'tf-dialog-backdrop',
    tabindex: '-1',
    '(click)': 'onBackdropClick($event)',
    '(keydown)': 'onKeydown($event)',
  },
  template: `
    <div
      class="tf-dialog"
      role="alertdialog"
      aria-modal="true"
      [attr.aria-label]="title()"
    >
      <div class="tf-dialog-title">{{ title() }}</div>
      <p class="tf-dialog-message">{{ message() }}</p>
      <div class="tf-dialog-actions">
        <button type="button" class="tf-btn tf-btn-secondary" (click)="close(false)">
          {{ cancelText() }}
        </button>
        <button
          type="button"
          class="tf-btn"
          [class.tf-btn-danger]="danger()"
          [class.tf-btn-primary]="!danger()"
          (click)="close(true)"
        >
          {{ confirmText() }}
        </button>
      </div>
    </div>
  `,
})
export class ConfirmDialog {
  readonly title = input('Are you sure?');
  readonly message = input('');
  readonly confirmText = input('Confirm');
  readonly cancelText = input('Cancel');
  readonly danger = input(false);

  readonly result = output<boolean>();

  protected onBackdropClick(event: Event): void {
    if ((event.target as HTMLElement).classList.contains('tf-dialog-backdrop')) {
      this.close(false);
    }
  }

  protected onKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      event.preventDefault();
      this.close(false);
    }
  }

  protected close(result: boolean): void {
    this.result.emit(result);
  }
}
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { form, FormField, required, email } from '@angular/forms/signals';
import { MatIcon } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';
import { AuthLayout } from '../../../core/layout/auth-layout/auth-layout';

@Component({
  selector: 'app-forgot-password',
  imports: [FormField, MatIcon, RouterLink, AuthLayout],
  templateUrl: './forgot-password.html',
})
export class ForgotPassword {
  private readonly auth = inject(AuthService);

  protected readonly model = signal({
    email: '',
  });

  protected readonly forgotForm = form(this.model, (path) => {
    required(path.email, { message: 'Email is required' });
    email(path.email, { message: 'Enter a valid email address' });
  });

  protected errorMessage = signal<string | null>(null);
  protected successMessage = signal<string | null>(null);
  protected isSubmitting = signal(false);

  protected onSubmit(event: Event): void {
    event.preventDefault();

    if (this.forgotForm().invalid()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.auth.forgotPassword(this.model()).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.successMessage.set(res.message);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Something went wrong. Please try again.');
      },
    });
  }
}
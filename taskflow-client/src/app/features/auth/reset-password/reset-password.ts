import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { form, FormField, required, email, minLength, maxLength } from '@angular/forms/signals';
import { MatIcon } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthLayout } from '../../../core/layout/auth-layout/auth-layout';

@Component({
  selector: 'app-reset-password',
  imports: [FormField, MatIcon, RouterLink, AuthLayout],
  templateUrl: './reset-password.html',
})
export class ResetPassword implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  protected readonly model = signal({
    email: '',
    code: '',
    newPassword: '',
  });

  protected readonly resetForm = form(this.model, (path) => {
    required(path.email, { message: 'Email is required' });
    email(path.email, { message: 'Enter a valid email address' });
    required(path.code, { message: 'Enter the code from your email' });
    minLength(path.code, 6, { message: 'Code must be 6 digits' });
    maxLength(path.code, 6, { message: 'Code must be 6 digits' });
    required(path.newPassword, { message: 'A new password is required' });
    minLength(path.newPassword, 8, { message: 'Password must be at least 8 characters' });
  });

  protected errorMessage = signal<string | null>(null);
  protected isSubmitting = signal(false);

  ngOnInit(): void {
    const emailFromQuery = this.route.snapshot.queryParamMap.get('email');
    if (emailFromQuery) {
      this.model.update((m) => ({ ...m, email: emailFromQuery }));
    }
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();

    if (this.resetForm().invalid()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.auth.resetPassword(this.model()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.toast.success('Password reset successful. You can now log in.');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Invalid or expired code.');
      },
    });
  }
}
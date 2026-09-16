import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { form, FormField, required, email, minLength } from '@angular/forms/signals';
import { MatIcon } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';
import { AuthLayout } from '../../../core/layout/auth-layout/auth-layout';

@Component({
  selector: 'app-register',
  imports: [FormField, MatIcon, RouterLink, AuthLayout],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly model = signal({
    fullName: '',
    email: '',
    password: '',
  });

  protected readonly registerForm = form(this.model, (path) => {
    required(path.fullName, { message: 'Full name is required' });
    required(path.email, { message: 'Email is required' });
    email(path.email, { message: 'Enter a valid email address' });
    required(path.password, { message: 'Password is required' });
    minLength(path.password, 8, { message: 'Password must be at least 8 characters' });
  });

  protected errorMessage = signal<string | null>(null);
  protected isSubmitting = signal(false);

  protected onSubmit(event: Event): void {
    event.preventDefault();

    if (this.registerForm().invalid()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.auth.register(this.model()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/verify-otp'], {
          queryParams: { email: this.model().email },
        });
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Registration failed. Please try again.');
      },
    });
  }
}
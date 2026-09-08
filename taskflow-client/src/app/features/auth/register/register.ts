import { Component, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { form, FormField, required, email, minLength } from '@angular/forms/signals';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormField, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  protected readonly model = signal({
    email: '',
    password: '',
    fullName: ''
  });

  protected readonly registerForm = form(this.model, (path) => {
    required(path.email, { message: 'Email is required' });
    email(path.email, { message: 'Enter a valid email address' });
    required(path.password, { message: 'Password is required' });
    minLength(path.password, 6, { message: 'Password must be at least 6 characters' });
    required(path.fullName, { message: 'Full name is required' });
  });

  protected errorMessage = signal<string | null>(null);
  protected isSubmitting = signal(false);

  constructor(private authService: AuthService, private router: Router) {}

  protected onSubmit(event: Event): void {
    event.preventDefault();

    if (this.registerForm().invalid()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.authService.register(this.model()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/verify-otp'], {
          queryParams: { email: this.model().email }
        });
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Registration failed. Please try again.');
      }
    });
  }
}
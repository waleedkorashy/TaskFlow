import { Component, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { form, FormField, required, email } from '@angular/forms/signals';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormField, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  protected readonly model = signal({
    email: '',
    password: ''
  });

  protected readonly loginForm = form(this.model, (path) => {
    required(path.email, { message: 'Email is required' });
    email(path.email, { message: 'Enter a valid email address' });
    required(path.password, { message: 'Password is required' });
  });

  protected errorMessage = signal<string | null>(null);
  protected isSubmitting = signal(false);

  constructor(private authService: AuthService, private router: Router) {}

  protected onSubmit(event: Event): void {
     event.preventDefault();
     
    if (this.loginForm().invalid()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.model()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/projects']);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        if (err.status === 403) {
          this.errorMessage.set('Please verify your email before logging in.');
        } else {
          this.errorMessage.set(err?.error?.message ?? 'Invalid email or password.');
        }
      }
    });
  }
}
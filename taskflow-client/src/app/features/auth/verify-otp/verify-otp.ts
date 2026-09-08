import { Component, signal, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { form, FormField, required, minLength, maxLength } from '@angular/forms/signals';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-verify-otp',
  standalone: true,
  imports: [FormField],
  templateUrl: './verify-otp.html',
  styleUrl: './verify-otp.scss'
})
export class VerifyOtp implements OnInit {
  protected readonly model = signal({
    email: '',
    code: ''
  });

  protected readonly otpForm = form(this.model, (path) => {
  required(path.code, { message: 'Enter the code from your email' });
  minLength(path.code, 6, { message: 'Code must be 6 digits' });
  maxLength(path.code, 6, { message: 'Code must be 6 digits' });
});

  protected errorMessage = signal<string | null>(null);
  protected infoMessage = signal<string | null>(null);
  protected isSubmitting = signal(false);
  protected isResending = signal(false);

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const emailFromQuery = this.route.snapshot.queryParamMap.get('email');
    if (emailFromQuery) {
      this.model.update(m => ({ ...m, email: emailFromQuery }));
    }
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();
    
    if (this.otpForm().invalid()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.authService.verifyOtp(this.model()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/projects']);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Invalid or expired code.');
      }
    });
  }

  protected onResend(): void {
    this.isResending.set(true);
    this.errorMessage.set(null);
    this.infoMessage.set(null);

    this.authService.resendOtp({ email: this.model().email }).subscribe({
      next: (res) => {
        this.isResending.set(false);
        this.infoMessage.set(res.message);
      },
      error: (err) => {
        this.isResending.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Could not resend code.');
      }
    });
  }
}
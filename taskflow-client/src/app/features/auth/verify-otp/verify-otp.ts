import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { form, FormField, required, minLength, maxLength } from '@angular/forms/signals';
import { MatIcon } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';
import { AuthLayout } from '../../../core/layout/auth-layout/auth-layout';

@Component({
  selector: 'app-verify-otp',
  imports: [FormField, MatIcon, AuthLayout],
  templateUrl: './verify-otp.html',
  styleUrl: './verify-otp.scss',
})
export class VerifyOtp implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  private cooldownTimer: ReturnType<typeof setInterval> | null = null;

  protected readonly model = signal({
    email: '',
    code: '',
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
  protected readonly resendCooldown = signal(0);

  ngOnInit(): void {
    const emailFromQuery = this.route.snapshot.queryParamMap.get('email');
    if (emailFromQuery) {
      this.model.update((m) => ({ ...m, email: emailFromQuery }));
    }
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();

    if (this.otpForm().invalid() || !this.model().email) {
      if (!this.model().email) {
        this.errorMessage.set('Your email address is missing. Please return to login.');
      }
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.auth.verifyOtp(this.model()).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/projects']);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Invalid or expired code.');
      },
    });
  }

  protected onResend(): void {
    if (this.resendCooldown() > 0 || this.isResending()) {
      return;
    }

    if (!this.model().email) {
      this.errorMessage.set('Your email address is missing. Please return to login.');
      return;
    }

    this.isResending.set(true);
    this.errorMessage.set(null);
    this.infoMessage.set(null);

    this.auth.resendOtp({ email: this.model().email }).subscribe({
      next: (res) => {
        this.isResending.set(false);
        this.infoMessage.set(res.message);
        this.startCooldown();
      },
      error: (err) => {
        this.isResending.set(false);
        this.errorMessage.set(err?.error?.message ?? 'Could not resend code.');
      },
    });
  }

  private startCooldown(): void {
    this.resendCooldown.set(60);

    if (this.cooldownTimer) {
      window.clearInterval(this.cooldownTimer);
    }

    this.cooldownTimer = window.setInterval(() => {
      this.resendCooldown.update((value) => {
        if (value <= 1) {
          if (this.cooldownTimer) {
            window.clearInterval(this.cooldownTimer);
          }
          return 0;
        }
        return value - 1;
      });
    }, 1000);

    this.destroyRef.onDestroy(() => {
      if (this.cooldownTimer) {
        window.clearInterval(this.cooldownTimer);
      }
    });
  }
}
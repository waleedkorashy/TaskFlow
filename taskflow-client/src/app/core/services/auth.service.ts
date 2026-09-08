import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  RegisterRequest, LoginRequest, VerifyOtpRequest, ResendOtpRequest,
  ForgotPasswordRequest, ResetPasswordRequest, AuthResponse, MessageResponse
} from '../models/auth.models';

const TOKEN_KEY = 'taskflow_token';
const USER_KEY = 'taskflow_user';

interface StoredUser {
  userId: string;
  email: string;
  fullName: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;

  private tokenSignal = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private userSignal = signal<StoredUser | null>(this.readStoredUser());

  isLoggedIn = computed(() => !!this.tokenSignal());
  currentUser = computed(() => this.userSignal());

  constructor(private http: HttpClient, private router: Router) {}

  private readStoredUser(): StoredUser | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  }

  private storeSession(response: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, response.token);
    const user: StoredUser = { userId: response.userId, email: response.email, fullName: response.fullName };
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this.tokenSignal.set(response.token);
    this.userSignal.set(user);
  }

  getToken(): string | null {
    return this.tokenSignal();
  }

  register(request: RegisterRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/register`, request);
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => this.storeSession(response))
    );
  }

  verifyOtp(request: VerifyOtpRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/verify-otp`, request).pipe(
      tap(response => this.storeSession(response))
    );
  }

  resendOtp(request: ResendOtpRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/resend-otp`, request);
  }

  forgotPassword(request: ForgotPasswordRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/forgot-password`, request);
  }

  resetPassword(request: ResetPasswordRequest): Observable<MessageResponse> {
    return this.http.post<MessageResponse>(`${this.apiUrl}/reset-password`, request);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.tokenSignal.set(null);
    this.userSignal.set(null);
    this.router.navigate(['/login']);
  }
}
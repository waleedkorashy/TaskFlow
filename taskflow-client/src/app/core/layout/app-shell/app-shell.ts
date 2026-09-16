import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { NgOptimizedImage } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { SignalRService } from '../../services/signalr.service';
import { ToastHost } from './toast-host';

@Component({
  selector: 'tf-app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatIcon, NgOptimizedImage, ToastHost],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.scss',
})
export class AppShell {
  protected readonly auth = inject(AuthService);
  protected readonly signalr = inject(SignalRService);

  protected readonly isAuthed = this.auth.isLoggedIn;
  protected readonly user = this.auth.currentUser;
  protected readonly connectionState = this.signalr.connectionState;

  protected readonly mobileNavOpen = signal(false);

  protected readonly initials = computed(() => {
    const name = this.user()?.fullName?.trim();
    if (!name) return '?';
    const parts = name.split(/\s+/);
    return parts.slice(0, 2).map(p => p.charAt(0).toUpperCase()).join('') || '?';
  });

  protected readonly connectionLabel = computed(() => {
    switch (this.connectionState()) {
      case 'connected':
        return 'Connected';
      case 'connecting':
        return 'Connecting…';
      case 'reconnecting':
        return 'Reconnecting…';
      default:
        return 'Offline';
    }
  });

  protected readonly connectionClass = computed(() =>
    this.connectionState() === 'connected' ? 'connected' : '',
  );

  protected toggleNav(): void {
    this.mobileNavOpen.update(open => !open);
  }

  protected closeNav(): void {
    this.mobileNavOpen.set(false);
  }

  protected logout(): void {
    this.auth.logout();
  }
}
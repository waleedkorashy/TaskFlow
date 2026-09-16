import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatIcon } from '@angular/material/icon';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { ProjectSummary } from '../../core/models/dashboard.models';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, DatePipe, MatIcon],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected stats = signal<{
    projectCount: number;
    boardCount: number;
    taskCount: number;
    recentProjects: ProjectSummary[];
  } | null>(null);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected readonly greeting = computed(() => {
    const name = this.authService.currentUser()?.fullName?.split(/\s+/)[0];
    const hour = new Date().getHours();
    const part = hour < 5 || hour >= 18 ? 'Good evening' : hour < 12 ? 'Good morning' : 'Good afternoon';
    return `${part}, ${name ?? 'there'}`;
  });

  ngOnInit(): void {
    this.dashboardService.getStats().subscribe({
      next: (stats) => {
        this.stats.set(stats);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load your dashboard.');
        this.isLoading.set(false);
      },
    });
  }

  protected onOpenProject(project: ProjectSummary): void {
    this.router.navigate(['/projects', project.id]);
  }

  protected initial(name: string): string {
    return (name.trim().charAt(0) || '?').toUpperCase();
  }

  protected plural(n: number, word: string): string {
    return `${n} ${word}${n === 1 ? '' : 's'}`;
  }
}
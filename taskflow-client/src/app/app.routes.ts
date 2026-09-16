import { Routes } from '@angular/router';
import { AppShell } from './core/layout/app-shell/app-shell';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  {
    path: '',
    component: AppShell,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

      // Public routes
      {
        path: 'login',
        title: 'Log In — TaskFlow',
        canActivate: [guestGuard],
        loadComponent: () => import('./features/auth/login/login').then(m => m.Login),
      },
      {
        path: 'register',
        title: 'Create Account — TaskFlow',
        canActivate: [guestGuard],
        loadComponent: () => import('./features/auth/register/register').then(m => m.Register),
      },
      {
        path: 'verify-otp',
        title: 'Verify Email — TaskFlow',
        canActivate: [guestGuard],
        loadComponent: () => import('./features/auth/verify-otp/verify-otp').then(m => m.VerifyOtp),
      },
      {
        path: 'forgot-password',
        title: 'Forgot Password — TaskFlow',
        canActivate: [guestGuard],
        loadComponent: () =>
          import('./features/auth/forgot-password/forgot-password').then(m => m.ForgotPassword),
      },
      {
        path: 'reset-password',
        title: 'Reset Password — TaskFlow',
        canActivate: [guestGuard],
        loadComponent: () =>
          import('./features/auth/reset-password/reset-password').then(m => m.ResetPassword),
      },
      {
        path: 'invitations/:token',
        title: 'Invitation — TaskFlow',
        loadComponent: () =>
          import('./features/invitations/accept-invitation/accept-invitation').then(m => m.AcceptInvitation),
      },

      // Protected routes
      {
        path: 'dashboard',
        title: 'Dashboard — TaskFlow',
        canActivate: [authGuard],
        loadComponent: () => import('./features/dashboard/dashboard').then(m => m.Dashboard),
      },
      {
        path: 'projects',
        title: 'Your Projects — TaskFlow',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/projects/project-list/project-list').then(m => m.ProjectList),
      },
      {
        path: 'projects/:id',
        title: 'Project — TaskFlow',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./features/projects/project-detail/project-detail').then(m => m.ProjectDetail),
      },
      {
        path: 'boards/:id',
        title: 'Board — TaskFlow',
        canActivate: [authGuard],
        loadComponent: () => import('./features/boards/board-view/board-view').then(m => m.BoardView),
      },
      {
        path: 'tasks/:id',
        title: 'Task — TaskFlow',
        canActivate: [authGuard],
        loadComponent: () => import('./features/tasks/task-detail/task-detail').then(m => m.TaskDetail),
      },

      { path: '**', title: 'Page Not Found — TaskFlow', loadComponent: () => import('./features/not-found/not-found').then(m => m.NotFound) },
    ],
  },
];
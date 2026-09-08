import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'register', pathMatch: 'full' },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then(m => m.Register)
  },
  {
    path: 'verify-otp',
    loadComponent: () => import('./features/auth/verify-otp/verify-otp').then(m => m.VerifyOtp)
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then(m => m.Login)
  },
  {
    path: 'projects',
    canActivate: [authGuard],
    loadComponent: () => import('./features/projects/project-list/project-list').then(m => m.ProjectList)
  },
  
  {
  path: 'projects/:id',
  canActivate: [authGuard],
  loadComponent: () => import('./features/projects/project-detail/project-detail').then(m => m.ProjectDetail)
},
{
  path: 'boards/:id',
  canActivate: [authGuard],
  loadComponent: () => import('./features/boards/board-view/board-view').then(m => m.BoardView)
},
{
  path: 'tasks/:id',
  canActivate: [authGuard],
  loadComponent: () => import('./features/tasks/task-detail/task-detail').then(m => m.TaskDetail)
},
{
  path: 'invitations/:token',
  loadComponent: () => import('./features/invitations/accept-invitation/accept-invitation').then(m => m.AcceptInvitation)
},



{ path: 'register', title: 'Register — TaskFlow', loadComponent: () => import('./features/auth/register/register').then(m => m.Register) },
{ path: 'verify-otp', title: 'Verify Email — TaskFlow', loadComponent: () => import('./features/auth/verify-otp/verify-otp').then(m => m.VerifyOtp) },
{ path: 'login', title: 'Log In — TaskFlow', loadComponent: () => import('./features/auth/login/login').then(m => m.Login) },
{ path: 'projects', title: 'Your Projects — TaskFlow', canActivate: [authGuard], loadComponent: () => import('./features/projects/project-list/project-list').then(m => m.ProjectList) },
{ path: 'projects/:id', title: 'Project — TaskFlow', canActivate: [authGuard], loadComponent: () => import('./features/projects/project-detail/project-detail').then(m => m.ProjectDetail) },
{ path: 'boards/:id', title: 'Board — TaskFlow', canActivate: [authGuard], loadComponent: () => import('./features/boards/board-view/board-view').then(m => m.BoardView) },
{ path: 'tasks/:id', title: 'Task — TaskFlow', canActivate: [authGuard], loadComponent: () => import('./features/tasks/task-detail/task-detail').then(m => m.TaskDetail) },
{ path: 'invitations/:token', title: 'Invitation — TaskFlow', loadComponent: () => import('./features/invitations/accept-invitation/accept-invitation').then(m => m.AcceptInvitation) },


{ path: '**', title: 'Page Not Found — TaskFlow', loadComponent: () => import('./features/not-found/not-found').then(m => m.NotFound) }
];
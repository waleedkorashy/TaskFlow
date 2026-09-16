import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { form, FormField, required } from '@angular/forms/signals';
import { DatePipe } from '@angular/common';
import { MatIcon } from '@angular/material/icon';
import { ProjectsService } from '../../../core/services/projects.service';
import { ToastService } from '../../../core/services/toast.service';
import { Project } from '../../../core/models/project.models';

@Component({
  selector: 'app-project-list',
  imports: [FormField, DatePipe, MatIcon],
  templateUrl: './project-list.html',
  styleUrl: './project-list.scss',
})
export class ProjectList implements OnInit {
  private readonly projectsService = inject(ProjectsService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected projects = signal<Project[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);
  protected showCreateForm = signal(false);

  protected readonly createModel = signal({ name: '', description: '' });
  protected readonly createForm = form(this.createModel, (path) => {
    required(path.name, { message: 'Project name is required' });
  });
  protected isCreating = signal(false);

  ngOnInit(): void {
    this.loadProjects();
  }

  private loadProjects(): void {
    this.isLoading.set(true);
    this.projectsService.getAll().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load projects.');
        this.isLoading.set(false);
      },
    });
  }

  protected onCreateSubmit(event: Event): void {
    event.preventDefault();
    if (this.createForm().invalid()) {
      return;
    }

    this.isCreating.set(true);
    const value = this.createModel();

    this.projectsService
      .create({ name: value.name, description: value.description || null })
      .subscribe({
        next: () => {
          this.isCreating.set(false);
          this.showCreateForm.set(false);
          this.createModel.set({ name: '', description: '' });
          this.loadProjects();
          this.toast.success('Project created.');
        },
        error: () => {
          this.isCreating.set(false);
          this.toast.error('Could not create project.');
        },
      });
  }

  protected onOpenProject(project: Project): void {
    this.router.navigate(['/projects', project.id]);
  }

  protected projectInitial(name: string): string {
    return (name.trim().charAt(0) || '?').toUpperCase();
  }
}
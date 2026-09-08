import { Component, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { form, FormField, required } from '@angular/forms/signals';
import { ProjectsService } from '../../../core/services/projects.service';
import { AuthService } from '../../../core/services/auth.service';
import { Project } from '../../../core/models/project.models';

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [FormField, RouterLink],
  templateUrl: './project-list.html',
  styleUrl: './project-list.scss'
})
export class ProjectList implements OnInit {
  protected projects = signal<Project[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);
  protected showCreateForm = signal(false);

  protected readonly createModel = signal({ name: '', description: '' });
  protected readonly createForm = form(this.createModel, (path) => {
    required(path.name, { message: 'Project name is required' });
  });
  protected isCreating = signal(false);

  constructor(
    private projectsService: ProjectsService,
    protected authService: AuthService,
    private router: Router
  ) {}

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
      }
    });
  }

  protected onCreateSubmit(event: Event): void {
    event.preventDefault();
    if (this.createForm().invalid()) {
      return;
    }

    this.isCreating.set(true);
    const value = this.createModel();

    this.projectsService.create({
      name: value.name,
      description: value.description || null
    }).subscribe({
      next: () => {
        this.isCreating.set(false);
        this.showCreateForm.set(false);
        this.createModel.set({ name: '', description: '' });
        this.loadProjects();
      },
      error: () => {
        this.isCreating.set(false);
        this.errorMessage.set('Could not create project.');
      }
    });
  }

  protected onOpenProject(project: Project): void {
    this.router.navigate(['/projects', project.id]);
  }

  protected onLogout(): void {
    this.authService.logout();
  }
}
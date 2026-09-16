import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { form, FormField, required } from '@angular/forms/signals';
import { DatePipe } from '@angular/common';
import { MatIcon } from '@angular/material/icon';
import { ProjectsService } from '../../../core/services/projects.service';
import { BoardsService } from '../../../core/services/boards.service';
import { InvitationsService } from '../../../core/services/invitations.service';
import { AuthService } from '../../../core/services/auth.service';
import { ConfirmService } from '../../../core/services/confirm.service';
import { ToastService } from '../../../core/services/toast.service';
import { Project, ProjectMember } from '../../../core/models/project.models';
import { Board } from '../../../core/models/board.models';
import { Invitation } from '../../../core/models/invitation.models';

@Component({
  selector: 'app-project-detail',
  imports: [FormField, RouterLink, DatePipe, MatIcon],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.scss',
})
export class ProjectDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly projectsService = inject(ProjectsService);
  private readonly boardsService = inject(BoardsService);
  private readonly invitationsService = inject(InvitationsService);
  private readonly authService = inject(AuthService);
  private readonly confirmService = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected project = signal<Project | null>(null);
  protected boards = signal<Board[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);
  protected showCreateForm = signal(false);
  protected isCreating = signal(false);
  protected isEditingName = signal(false);
  protected editNameValue = signal('');
  protected members = signal<ProjectMember[]>([]);
  protected pendingInvitations = signal<Invitation[]>([]);
  protected showMembersPanel = signal(false);
  protected inviteEmail = signal('');
  protected isInviting = signal(false);
  protected inviteMessage = signal<string | null>(null);

  protected boardCount = computed(() => this.boards().length);

  protected readonly createModel = signal({ name: '' });
  protected readonly createForm = form(this.createModel, (path) => {
    required(path.name, { message: 'Board name is required' });
  });

  private projectId!: string;

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id')!;
    this.loadData();
  }

  private loadData(): void {
    this.isLoading.set(true);

    this.projectsService.getOne(this.projectId).subscribe({
      next: (project) => {
        this.project.set(project);
        this.loadMembers();
      },
      error: () => {
        this.errorMessage.set('Could not load project.');
        this.isLoading.set(false);
      },
    });

    this.boardsService.getByProject(this.projectId).subscribe({
      next: (boards) => {
        this.boards.set(boards);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load boards.');
        this.isLoading.set(false);
      },
    });
  }

  private loadMembers(): void {
    this.projectsService.getMembers(this.projectId).subscribe({
      next: (members) => {
        this.members.set(members);
        if (this.isOwner()) {
          this.loadPendingInvitations();
        }
      },
    });
  }

  private loadPendingInvitations(): void {
    this.invitationsService.getPending(this.projectId).subscribe({
      next: (invitations) => this.pendingInvitations.set(invitations),
      error: () => this.toast.error('Could not load invitations.'),
    });
  }

  protected onCreateSubmit(event: Event): void {
    event.preventDefault();
    if (this.createForm().invalid()) {
      return;
    }

    this.isCreating.set(true);
    this.boardsService.create(this.projectId, { name: this.createModel().name }).subscribe({
      next: () => {
        this.isCreating.set(false);
        this.showCreateForm.set(false);
        this.createModel.set({ name: '' });
        this.loadData();
        this.toast.success('Board created.');
      },
      error: () => {
        this.isCreating.set(false);
        this.toast.error('Could not create board.');
      },
    });
  }

  protected startEditName(): void {
    const p = this.project();
    if (!p) return;
    this.editNameValue.set(p.name);
    this.isEditingName.set(true);
  }

  protected saveName(): void {
    const p = this.project();
    if (!p) return;

    const newName = this.editNameValue().trim();
    if (!newName) {
      this.isEditingName.set(false);
      return;
    }

    this.projectsService.update(p.id, { name: newName, description: p.description }).subscribe({
      next: (updated) => {
        this.project.set(updated);
        this.isEditingName.set(false);
        this.toast.success('Project renamed.');
      },
      error: () => {
        this.toast.error('Could not rename project.');
        this.isEditingName.set(false);
      },
    });
  }

  protected async onDeleteProject(): Promise<void> {
    const p = this.project();
    if (!p) return;

    const ok = await this.confirmService.confirm({
      title: 'Delete project?',
      message: `Delete "${p.name}" and everything in it? This cannot be undone.`,
      confirmText: 'Delete project',
      danger: true,
    });
    if (!ok) return;

    this.projectsService.delete(p.id).subscribe({
      next: () => {
        this.toast.success('Project deleted.');
        this.router.navigate(['/projects']);
      },
      error: () => this.toast.error('Could not delete project.'),
    });
  }

  protected onOpenBoard(board: Board): void {
    this.router.navigate(['/boards', board.id]);
  }

  protected isOwner(): boolean {
    const p = this.project();
    const user = this.authService.currentUser();
    return !!p && !!user && p.ownerId === user.userId;
  }

  protected onInvite(event: Event): void {
    event.preventDefault();
    const email = this.inviteEmail().trim();
    if (!email) return;

    this.isInviting.set(true);
    this.inviteMessage.set(null);

    this.invitationsService.invite(this.projectId, { email }).subscribe({
      next: () => {
        this.isInviting.set(false);
        this.inviteEmail.set('');
        this.inviteMessage.set(`Invitation sent to ${email}.`);
        this.loadPendingInvitations();
      },
      error: (err) => {
        this.isInviting.set(false);
        this.inviteMessage.set(err?.error?.message ?? 'Could not send invitation.');
      },
    });
  }

  protected onRevokeInvitation(invitationId: string): void {
    this.invitationsService.revoke(invitationId).subscribe({
      next: () => {
        this.pendingInvitations.update((inv) => inv.filter((i) => i.id !== invitationId));
        this.toast.success('Invitation revoked.');
      },
      error: () => this.toast.error('Could not revoke invitation.'),
    });
  }

  protected async onRemoveMember(member: ProjectMember): Promise<void> {
    const ok = await this.confirmService.confirm({
      title: 'Remove member?',
      message: `Remove ${member.fullName} from this project? They will lose access to its boards and tasks.`,
      confirmText: 'Remove',
      danger: true,
    });
    if (!ok) return;

    this.projectsService.removeMember(this.projectId, member.userId).subscribe({
      next: () => {
        this.members.update((m) => m.filter((x) => x.userId !== member.userId));
        this.toast.success('Member removed.');
      },
      error: () => this.toast.error('Could not remove member.'),
    });
  }

  protected async onLeaveProject(): Promise<void> {
    const ok = await this.confirmService.confirm({
      title: 'Leave project?',
      message: 'Leave this project? You will lose access to its boards and tasks.',
      confirmText: 'Leave project',
      danger: true,
    });
    if (!ok) return;

    this.projectsService.leaveProject(this.projectId).subscribe({
      next: () => {
        this.toast.success('You left the project.');
        this.router.navigate(['/projects']);
      },
      error: (err) => this.toast.error(err?.error?.message ?? 'Could not leave project.'),
    });
  }

  protected initials(name: string): string {
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((p) => p.charAt(0).toUpperCase())
      .join('');
  }
}
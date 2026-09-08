import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { form, FormField, required } from '@angular/forms/signals';
import { ProjectsService } from '../../../core/services/projects.service';
import { BoardsService } from '../../../core/services/boards.service';
import { Project } from '../../../core/models/project.models';
import { Board } from '../../../core/models/board.models';
import { InvitationsService } from '../../../core/services/invitations.service';
import { AuthService } from '../../../core/services/auth.service';
import { ProjectMember } from '../../../core/models/project.models';
import { Invitation } from '../../../core/models/invitation.models';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [FormField, RouterLink],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.scss'
})
export class ProjectDetail implements OnInit {
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

  private projectId!: string;

  protected readonly createModel = signal({ name: '' });
  protected readonly createForm = form(this.createModel, (path) => {
    required(path.name, { message: 'Board name is required' });
  });

constructor(
  private route: ActivatedRoute,
  private router: Router,
  private projectsService: ProjectsService,
  private boardsService: BoardsService,
  private invitationsService: InvitationsService,
  protected authService: AuthService
) {}

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
    error: () => this.errorMessage.set('Could not load project.')
  });

  this.boardsService.getByProject(this.projectId).subscribe({
    next: (boards) => {
      this.boards.set(boards);
      this.isLoading.set(false);
    },
    error: () => {
      this.errorMessage.set('Could not load boards.');
      this.isLoading.set(false);
    }
  });
}

private loadMembers(): void {
  this.projectsService.getMembers(this.projectId).subscribe({
    next: (members) => {
      this.members.set(members);
      if (this.isOwner()) {
        this.loadPendingInvitations();
      }
    }
  });
}

private loadPendingInvitations(): void {
  this.invitationsService.getPending(this.projectId).subscribe({
    next: (invitations) => this.pendingInvitations.set(invitations)
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
      },
      error: () => {
        this.isCreating.set(false);
        this.errorMessage.set('Could not create board.');
      }
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
      },
      error: () => {
        this.errorMessage.set('Could not rename project.');
        this.isEditingName.set(false);
      }
    });
  }

  protected onDeleteProject(): void {
    const p = this.project();
    if (!p) return;

    if (!confirm(`Delete project "${p.name}"? This will delete all its boards and tasks. This cannot be undone.`)) return;

    this.projectsService.delete(p.id).subscribe({
      next: () => this.router.navigate(['/projects']),
      error: () => this.errorMessage.set('Could not delete project.')
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
    }
  });
}

protected onRevokeInvitation(invitationId: string): void {
  this.invitationsService.revoke(invitationId).subscribe({
    next: () => this.pendingInvitations.update(inv => inv.filter(i => i.id !== invitationId)),
    error: () => this.errorMessage.set('Could not revoke invitation.')
  });
}

protected onRemoveMember(memberUserId: string): void {
  if (!confirm('Remove this member from the project?')) return;

  this.projectsService.removeMember(this.projectId, memberUserId).subscribe({
    next: () => this.members.update(m => m.filter(x => x.userId !== memberUserId)),
    error: () => this.errorMessage.set('Could not remove member.')
  });
}

protected onLeaveProject(): void {
  if (!confirm('Leave this project? You will lose access to its boards and tasks.')) return;

  this.projectsService.leaveProject(this.projectId).subscribe({
    next: () => this.router.navigate(['/projects']),
    error: (err) => this.errorMessage.set(err?.error?.message ?? 'Could not leave project.')
  });
}

}
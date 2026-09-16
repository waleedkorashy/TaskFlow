import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { MatIcon } from '@angular/material/icon';
import { TasksService } from '../../../core/services/tasks.service';
import { CommentsService } from '../../../core/services/comments.service';
import { LabelsService } from '../../../core/services/labels.service';
import { ProjectsService } from '../../../core/services/projects.service';
import { AuthService } from '../../../core/services/auth.service';
import { ConfirmService } from '../../../core/services/confirm.service';
import { ToastService } from '../../../core/services/toast.service';
import { TaskItem } from '../../../core/models/task.models';
import { Comment } from '../../../core/models/comment.models';
import { Label } from '../../../core/models/label.models';
import { ProjectMember } from '../../../core/models/project.models';

@Component({
  selector: 'app-task-detail',
  imports: [DatePipe, MatIcon],
  templateUrl: './task-detail.html',
  styleUrl: './task-detail.scss',
})
export class TaskDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly tasksService = inject(TasksService);
  private readonly commentsService = inject(CommentsService);
  private readonly labelsService = inject(LabelsService);
  private readonly projectsService = inject(ProjectsService);
  private readonly authService = inject(AuthService);
  private readonly confirmService = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected task = signal<TaskItem | null>(null);
  protected boardId = signal<string | null>(null);
  protected projectId = signal<string | null>(null);

  protected comments = signal<Comment[]>([]);
  protected allLabels = signal<Label[]>([]);
  protected taskLabelIds = signal<string[]>([]);
  protected members = signal<ProjectMember[]>([]);

  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected isEditingTitle = signal(false);
  protected editTitleValue = signal('');
  protected editDescription = signal('');
  protected editDueDate = signal('');
  protected editAssigneeId = signal('');

  protected newCommentText = signal('');
  protected isPostingComment = signal(false);
  protected editingCommentId = signal<string | null>(null);
  protected editCommentText = signal('');

  protected newLabelName = signal('');
  protected newLabelColor = signal('#c1532a');
  protected isCreatingLabel = signal(false);
  protected editingLabelId = signal<string | null>(null);
  protected editLabelName = signal('');
  protected editLabelColor = signal('#c1532a');

  private taskId!: string;

  ngOnInit(): void {
    this.taskId = this.route.snapshot.paramMap.get('id')!;
    this.loadTask();
  }

  private loadTask(): void {
    this.isLoading.set(true);

    this.tasksService.getOne(this.taskId).subscribe({
      next: (task) => {
        this.task.set(task);
        this.editDescription.set(task.description ?? '');
        this.editDueDate.set(task.dueDate ? task.dueDate.substring(0, 10) : '');
        this.editAssigneeId.set(task.assigneeId ?? '');
        this.boardId.set(task.boardId);
        this.projectId.set(task.projectId);
        this.taskLabelIds.set(task.labels.map((l) => l.id));
        this.isLoading.set(false);
        this.resolveProjectContext();
      },
      error: () => {
        this.errorMessage.set('Could not load task.');
        this.isLoading.set(false);
      },
    });

    this.commentsService.getByTask(this.taskId).subscribe({
      next: (comments) => this.comments.set(comments),
      error: () => this.toast.error('Could not load comments.'),
    });
  }

  private resolveProjectContext(): void {
    const projectId = this.projectId();
    if (!projectId) return;

    this.projectsService.getMembers(projectId).subscribe({
      next: (members) => this.members.set(members),
      error: () => this.toast.error('Could not load project members.'),
    });

    this.labelsService.getByProject(projectId).subscribe({
      next: (labels) => this.allLabels.set(labels),
      error: () => this.toast.error('Could not load labels.'),
    });
  }

  // ---- Task title/details ----

  protected startEditTitle(): void {
    const t = this.task();
    if (!t) return;
    this.editTitleValue.set(t.title);
    this.isEditingTitle.set(true);
  }

  protected saveTitle(): void {
    const newTitle = this.editTitleValue().trim();
    this.isEditingTitle.set(false);
    if (!newTitle) return;
    this.saveTaskDetails({ title: newTitle });
  }

  protected saveDetails(): void {
    this.saveTaskDetails({});
  }

  private saveTaskDetails(overrides: { title?: string }): void {
    const t = this.task();
    if (!t) return;

    this.tasksService
      .update(this.taskId, {
        title: overrides.title ?? t.title,
        description: this.editDescription().trim() || null,
        dueDate: this.editDueDate() ? new Date(this.editDueDate()).toISOString() : null,
        assigneeId: this.editAssigneeId() || null,
      })
      .subscribe({
        next: (updated) => {
          this.task.set(updated);
          this.taskLabelIds.set(updated.labels.map((l) => l.id));
          this.toast.success('Task saved.');
        },
        error: () => this.toast.error('Could not save changes.'),
      });
  }

  // ---- Comments (create, edit, delete — own comments only) ----

  protected isOwnComment(comment: Comment): boolean {
    return comment.userId === this.authService.currentUser()?.userId;
  }

  protected avatarInitials(name: string): string {
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((p) => p.charAt(0).toUpperCase())
      .join('');
  }

  protected onPostComment(event: Event): void {
    event.preventDefault();
    const content = this.newCommentText().trim();
    if (!content) return;

    this.isPostingComment.set(true);
    this.commentsService.create(this.taskId, { content }).subscribe({
      next: (comment) => {
        this.comments.update((c) => [...c, comment]);
        this.newCommentText.set('');
        this.isPostingComment.set(false);
        this.toast.success('Comment posted.');
      },
      error: () => {
        this.isPostingComment.set(false);
        this.toast.error('Could not post comment.');
      },
    });
  }

  protected startEditComment(comment: Comment): void {
    this.editingCommentId.set(comment.id);
    this.editCommentText.set(comment.content);
  }

  protected saveCommentEdit(commentId: string): void {
    const newContent = this.editCommentText().trim();
    if (!newContent) {
      this.editingCommentId.set(null);
      return;
    }

    this.commentsService.update(commentId, { content: newContent }).subscribe({
      next: (updated) => {
        this.comments.update((c) => c.map((x) => (x.id === commentId ? updated : x)));
        this.editingCommentId.set(null);
        this.toast.success('Comment updated.');
      },
      error: () => {
        this.editingCommentId.set(null);
        this.toast.error('Could not update comment.');
      },
    });
  }

  protected cancelEditComment(): void {
    this.editingCommentId.set(null);
  }

  protected onDeleteComment(comment: Comment): void {
    this.commentsService.delete(comment.id).subscribe({
      next: () => {
        this.comments.update((c) => c.filter((x) => x.id !== comment.id));
        this.toast.success('Comment deleted.');
      },
      error: () => this.toast.error('Could not delete comment.'),
    });
  }

  // ---- Labels (create, rename, delete, attach/detach) ----

  protected isLabelAttached(labelId: string): boolean {
    return this.taskLabelIds().includes(labelId);
  }

  protected toggleLabel(label: Label): void {
    const attached = this.isLabelAttached(label.id);
    const action = attached
      ? this.labelsService.detachFromTask(this.taskId, label.id)
      : this.labelsService.attachToTask(this.taskId, label.id);

    action.subscribe({
      next: () => {
        this.taskLabelIds.update((ids) =>
          attached ? ids.filter((id) => id !== label.id) : [...ids, label.id],
        );
      },
      error: () => this.toast.error('Could not update label.'),
    });
  }

  protected onCreateLabel(event: Event): void {
    event.preventDefault();
    const name = this.newLabelName().trim();
    const projectId = this.projectId();
    if (!name || !projectId) return;

    this.isCreatingLabel.set(true);
    this.labelsService.create(projectId, { name, colorHex: this.newLabelColor() }).subscribe({
      next: (label) => {
        this.allLabels.update((l) => [...l, label]);
        this.newLabelName.set('');
        this.isCreatingLabel.set(false);
        this.toast.success('Label created.');
      },
      error: () => {
        this.isCreatingLabel.set(false);
        this.toast.error('Could not create label.');
      },
    });
  }

  protected startEditLabel(label: Label): void {
    this.editingLabelId.set(label.id);
    this.editLabelName.set(label.name);
    this.editLabelColor.set(label.colorHex);
  }

  protected saveLabelEdit(labelId: string): void {
    const newName = this.editLabelName().trim();
    if (!newName) {
      this.editingLabelId.set(null);
      return;
    }

    this.labelsService.update(labelId, { name: newName, colorHex: this.editLabelColor() }).subscribe({
      next: (updated) => {
        this.allLabels.update((l) => l.map((x) => (x.id === labelId ? updated : x)));
        this.editingLabelId.set(null);
        this.toast.success('Label updated.');
      },
      error: () => {
        this.editingLabelId.set(null);
        this.toast.error('Could not update label.');
      },
    });
  }

  protected cancelEditLabel(): void {
    this.editingLabelId.set(null);
  }

  protected async onDeleteLabel(label: Label): Promise<void> {
    const ok = await this.confirmService.confirm({
      title: 'Delete label?',
      message: `Delete "${label.name}"? It will be removed from all tasks.`,
      confirmText: 'Delete label',
      danger: true,
    });
    if (!ok) return;

    this.labelsService.delete(label.id).subscribe({
      next: () => {
        this.allLabels.update((l) => l.filter((x) => x.id !== label.id));
        this.taskLabelIds.update((ids) => ids.filter((id) => id !== label.id));
        this.toast.success('Label deleted.');
      },
      error: () => this.toast.error('Could not delete label.'),
    });
  }

  protected goBackToBoard(): void {
    const bId = this.boardId();
    if (bId) {
      this.router.navigate(['/boards', bId]);
    } else {
      this.router.navigate(['/projects']);
    }
  }
}
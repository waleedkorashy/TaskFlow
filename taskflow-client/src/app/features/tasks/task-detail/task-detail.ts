import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormField } from '@angular/forms/signals';
import { TasksService } from '../../../core/services/tasks.service';
import { CommentsService } from '../../../core/services/comments.service';
import { LabelsService } from '../../../core/services/labels.service';
import { ProjectsService } from '../../../core/services/projects.service';
import { AuthService } from '../../../core/services/auth.service';
import { TaskItem } from '../../../core/models/task.models';
import { Comment } from '../../../core/models/comment.models';
import { Label } from '../../../core/models/label.models';
import { ProjectMember } from '../../../core/models/project.models';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [FormField, RouterLink],
  templateUrl: './task-detail.html',
  styleUrl: './task-detail.scss'
})
export class TaskDetail implements OnInit {
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
  protected newLabelColor = signal('#3f51b5');
  protected isCreatingLabel = signal(false);
  protected editingLabelId = signal<string | null>(null);
  protected editLabelName = signal('');
  protected editLabelColor = signal('#3f51b5');

  private taskId!: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private tasksService: TasksService,
    private commentsService: CommentsService,
    private labelsService: LabelsService,
    private projectsService: ProjectsService,
    protected authService: AuthService
  ) {}

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
        this.resolveProjectContext();
      },
      error: () => {
        this.errorMessage.set('Could not load task.');
        this.isLoading.set(false);
      }
    });

    this.commentsService.getByTask(this.taskId).subscribe({
      next: (comments) => this.comments.set(comments)
    });
  }

  private resolveProjectContext(): void {
    const projectId = history.state?.projectId as string | undefined;
    const boardId = history.state?.boardId as string | undefined;

    if (!projectId || !boardId) {
      this.isLoading.set(false);
      return;
    }

    this.boardId.set(boardId);
    this.projectId.set(projectId);

    this.projectsService.getMembers(projectId).subscribe({
      next: (members) => this.members.set(members)
    });

    this.labelsService.getByProject(projectId).subscribe({
      next: (labels) => {
        this.allLabels.set(labels);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
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

    this.tasksService.update(this.taskId, {
      title: overrides.title ?? t.title,
      description: this.editDescription().trim() || null,
      dueDate: this.editDueDate() ? new Date(this.editDueDate()).toISOString() : null,
      assigneeId: this.editAssigneeId() || null
    }).subscribe({
      next: (updated) => this.task.set(updated),
      error: () => this.errorMessage.set('Could not save changes.')
    });
  }

  // ---- Comments (create, edit, delete — own comments only) ----

  protected isOwnComment(comment: Comment): boolean {
    return comment.userId === this.authService.currentUser()?.userId;
  }

  protected onPostComment(event: Event): void {
    event.preventDefault();
    const content = this.newCommentText().trim();
    if (!content) return;

    this.isPostingComment.set(true);
    this.commentsService.create(this.taskId, { content }).subscribe({
      next: (comment) => {
        this.comments.update(c => [...c, comment]);
        this.newCommentText.set('');
        this.isPostingComment.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not post comment.');
        this.isPostingComment.set(false);
      }
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
        this.comments.update(c => c.map(x => x.id === commentId ? updated : x));
        this.editingCommentId.set(null);
      },
      error: () => {
        this.errorMessage.set('Could not update comment.');
        this.editingCommentId.set(null);
      }
    });
  }

  protected cancelEditComment(): void {
    this.editingCommentId.set(null);
  }

  protected onDeleteComment(commentId: string): void {
    this.commentsService.delete(commentId).subscribe({
      next: () => this.comments.update(c => c.filter(x => x.id !== commentId)),
      error: () => this.errorMessage.set('Could not delete comment.')
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
        this.taskLabelIds.update(ids =>
          attached ? ids.filter(id => id !== label.id) : [...ids, label.id]
        );
      },
      error: () => this.errorMessage.set('Could not update label.')
    });
  }

  protected onCreateLabel(event: Event): void {
    event.preventDefault();
    const name = this.newLabelName().trim();
    if (!name) return;

    const projectId = this.projectId();
    if (!projectId) return;

    this.isCreatingLabel.set(true);
    this.labelsService.create(projectId, { name, colorHex: this.newLabelColor() }).subscribe({
      next: (label) => {
        this.allLabels.update(l => [...l, label]);
        this.newLabelName.set('');
        this.isCreatingLabel.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not create label.');
        this.isCreatingLabel.set(false);
      }
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
        this.allLabels.update(l => l.map(x => x.id === labelId ? updated : x));
        this.editingLabelId.set(null);
      },
      error: () => {
        this.errorMessage.set('Could not update label.');
        this.editingLabelId.set(null);
      }
    });
  }

  protected cancelEditLabel(): void {
    this.editingLabelId.set(null);
  }

  protected onDeleteLabel(labelId: string): void {
    if (!confirm('Delete this label? It will be removed from all tasks.')) return;

    this.labelsService.delete(labelId).subscribe({
      next: () => {
        this.allLabels.update(l => l.filter(x => x.id !== labelId));
        this.taskLabelIds.update(ids => ids.filter(id => id !== labelId));
      },
      error: () => this.errorMessage.set('Could not delete label.')
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
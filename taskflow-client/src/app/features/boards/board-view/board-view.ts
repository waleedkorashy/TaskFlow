import { Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { form, FormField, required } from '@angular/forms/signals';
import { CdkDropList, CdkDropListGroup, CdkDrag, CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { DatePipe } from '@angular/common';
import { MatIcon } from '@angular/material/icon';
import { BoardsService } from '../../../core/services/boards.service';
import { TasksService } from '../../../core/services/tasks.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { ConfirmService } from '../../../core/services/confirm.service';
import { ToastService } from '../../../core/services/toast.service';
import { Board, BoardColumn } from '../../../core/models/board.models';
import { TaskItem } from '../../../core/models/task.models';

interface ColumnWithTasks extends BoardColumn {
  tasks: TaskItem[];
  showAddTask: boolean;
}

@Component({
  selector: 'app-board-view',
  imports: [FormField, RouterLink, CdkDropList, CdkDropListGroup, CdkDrag, DatePipe, MatIcon],
  templateUrl: './board-view.html',
  styleUrl: './board-view.scss',
})
export class BoardView implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly boardsService = inject(BoardsService);
  private readonly tasksService = inject(TasksService);
  private readonly signalRService = inject(SignalRService);
  private readonly confirmService = inject(ConfirmService);
  private readonly toast = inject(ToastService);

  protected board = signal<Board | null>(null);
  protected columns = signal<ColumnWithTasks[]>([]);
  protected isLoading = signal(true);
  protected errorMessage = signal<string | null>(null);

  protected searchQuery = signal('');
  protected hasFilter = computed(() => this.searchQuery().trim().length > 0);

  protected isEditingBoardName = signal(false);
  protected editBoardNameValue = signal('');

  protected showAddColumn = signal(false);
  protected readonly columnModel = signal({ name: '' });
  protected readonly columnForm = form(this.columnModel, (path) => {
    required(path.name, { message: 'Column name is required' });
  });
  protected isAddingColumn = signal(false);

  protected editingColumnId = signal<string | null>(null);
  protected editColumnName = signal('');

  protected taskInputValues = signal<Record<string, string>>({});
  protected isAddingTaskFor = signal<Record<string, boolean>>({});

  private boardId!: string;
  private reloadDebounceTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.boardId = this.route.snapshot.paramMap.get('id')!;
    this.loadBoard(true);
    this.setupRealtime();
  }

  ngOnDestroy(): void {
    this.signalRService.leaveBoard(this.boardId);
    this.signalRService.clearHandlers();
    if (this.reloadDebounceTimer) clearTimeout(this.reloadDebounceTimer);
  }

  private setupRealtime(): void {
    this.signalRService.onBoardChanged(() => this.scheduleReload());
    this.signalRService.joinBoard(this.boardId);
  }

  private scheduleReload(): void {
    if (this.reloadDebounceTimer) clearTimeout(this.reloadDebounceTimer);
    this.reloadDebounceTimer = setTimeout(() => this.loadBoard(false), 300);
  }

  private loadBoard(showLoading: boolean): void {
    if (showLoading) this.isLoading.set(true);

    this.boardsService.getOne(this.boardId).subscribe({
      next: (board) => {
        this.board.set(board);
        this.loadTasksForAllColumns(board.columns, showLoading);
      },
      error: () => {
        this.errorMessage.set('Could not load board.');
        this.isLoading.set(false);
      },
    });
  }

  private loadTasksForAllColumns(columns: BoardColumn[], showLoading: boolean): void {
    const columnsWithTasks: ColumnWithTasks[] = columns
      .sort((a, b) => a.sortOrder - b.sortOrder)
      .map((col) => ({ ...col, tasks: [], showAddTask: false }));

    this.columns.set(columnsWithTasks);

    if (columns.length === 0) {
      if (showLoading) this.isLoading.set(false);
      return;
    }

    let loadedCount = 0;
    columns.forEach((col) => {
      this.tasksService.getByColumn(col.id).subscribe({
        next: (tasks) => {
          this.columns.update((cols) => cols.map((c) => (c.id === col.id ? { ...c, tasks } : c)));
          loadedCount++;
          if (loadedCount === columns.length && showLoading) this.isLoading.set(false);
        },
        error: () => {
          loadedCount++;
          if (loadedCount === columns.length && showLoading) this.isLoading.set(false);
        },
      });
    });
  }

  // ---- Filter ----

  protected visibleTasks(column: ColumnWithTasks): TaskItem[] {
    const query = this.searchQuery().trim().toLowerCase();
    if (!query) return column.tasks;
    return column.tasks.filter((t) => {
      const title = t.title.toLowerCase().includes(query);
      const assignee = t.assigneeName?.toLowerCase().includes(query) ?? false;
      const labels = t.labels.some((l) => l.name.toLowerCase().includes(query));
      return title || assignee || labels;
    });
  }

  protected isOverdue(dueDate: string): boolean {
    return new Date(dueDate).getTime() < Date.now();
  }

  protected initials(name: string): string {
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((p) => p.charAt(0).toUpperCase())
      .join('');
  }

  // ---- Board rename/delete ----

  protected startEditBoardName(): void {
    const b = this.board();
    if (!b) return;
    this.editBoardNameValue.set(b.name);
    this.isEditingBoardName.set(true);
  }

  protected saveBoardName(): void {
    const b = this.board();
    if (!b) return;

    const newName = this.editBoardNameValue().trim();
    if (!newName) {
      this.isEditingBoardName.set(false);
      return;
    }

    this.boardsService.update(b.id, { name: newName }).subscribe({
      next: (updated) => {
        this.board.set({ ...b, name: updated.name });
        this.isEditingBoardName.set(false);
        this.toast.success('Board renamed.');
      },
      error: () => {
        this.toast.error('Could not rename board.');
        this.isEditingBoardName.set(false);
      },
    });
  }

  protected async onDeleteBoard(): Promise<void> {
    const b = this.board();
    if (!b) return;

    const ok = await this.confirmService.confirm({
      title: 'Delete board?',
      message: `Delete "${b.name}" and all of its columns and tasks? This cannot be undone.`,
      confirmText: 'Delete board',
      danger: true,
    });
    if (!ok) return;

    this.boardsService.delete(b.id).subscribe({
      next: () => {
        this.toast.success('Board deleted.');
        this.router.navigate(['/projects', b.projectId]);
      },
      error: () => this.toast.error('Could not delete board.'),
    });
  }

  // ---- Column add/rename/delete ----

  protected onAddColumn(event: Event): void {
    event.preventDefault();
    if (this.columnForm().invalid()) return;

    this.isAddingColumn.set(true);
    this.boardsService.createColumn(this.boardId, { name: this.columnModel().name }).subscribe({
      next: () => {
        this.isAddingColumn.set(false);
        this.showAddColumn.set(false);
        this.columnModel.set({ name: '' });
        this.loadBoard(false);
      },
      error: () => {
        this.isAddingColumn.set(false);
        this.toast.error('Could not create column.');
      },
    });
  }

  protected startEditColumn(column: ColumnWithTasks): void {
    this.editingColumnId.set(column.id);
    this.editColumnName.set(column.name);
  }

  protected saveColumnName(columnId: string): void {
    const newName = this.editColumnName().trim();
    if (!newName) {
      this.editingColumnId.set(null);
      return;
    }

    this.boardsService.renameColumn(columnId, { name: newName }).subscribe({
      next: () => {
        this.editingColumnId.set(null);
        this.loadBoard(false);
      },
      error: () => {
        this.toast.error('Could not rename column.');
        this.editingColumnId.set(null);
      },
    });
  }

  protected async onDeleteColumn(columnId: string): Promise<void> {
    const ok = await this.confirmService.confirm({
      title: 'Delete column?',
      message: 'Delete this column and all of its tasks? This cannot be undone.',
      confirmText: 'Delete column',
      danger: true,
    });
    if (!ok) return;

    this.boardsService.deleteColumn(columnId).subscribe({
      next: () => {
        this.toast.success('Column deleted.');
        this.loadBoard(false);
      },
      error: () => this.toast.error('Could not delete column.'),
    });
  }

  // ---- Task add/delete ----

  protected toggleAddTask(columnId: string): void {
    this.columns.update((cols) =>
      cols.map((c) => (c.id === columnId ? { ...c, showAddTask: !c.showAddTask } : c)),
    );
    this.setTaskInputValue(columnId, '');
  }

  protected getTaskInputValue(columnId: string): string {
    return this.taskInputValues()[columnId] ?? '';
  }

  protected setTaskInputValue(columnId: string, value: string): void {
    this.taskInputValues.update((v) => ({ ...v, [columnId]: value }));
  }

  protected onAddTask(event: Event, columnId: string): void {
    event.preventDefault();
    const title = this.getTaskInputValue(columnId).trim();
    if (!title) return;

    this.isAddingTaskFor.update((v) => ({ ...v, [columnId]: true }));

    this.tasksService
      .create(columnId, { title, description: null, dueDate: null, assigneeId: null })
      .subscribe({
        next: () => {
          this.isAddingTaskFor.update((v) => ({ ...v, [columnId]: false }));
          this.setTaskInputValue(columnId, '');
          this.columns.update((cols) =>
            cols.map((c) => (c.id === columnId ? { ...c, showAddTask: false } : c)),
          );
          this.loadBoard(false);
        },
        error: () => {
          this.isAddingTaskFor.update((v) => ({ ...v, [columnId]: false }));
          this.toast.error('Could not create task.');
        },
      });
  }

  protected async onDeleteTask(task: TaskItem, columnId: string): Promise<void> {
    const ok = await this.confirmService.confirm({
      title: 'Delete task?',
      message: `Delete "${task.title}"? This cannot be undone.`,
      confirmText: 'Delete task',
      danger: true,
    });
    if (!ok) return;

    this.tasksService.delete(task.id).subscribe({
      next: () => {
        this.toast.success('Task deleted.');
        this.loadBoard(false);
      },
      error: () => this.toast.error('Could not delete task.'),
    });
  }

  // ---- Drag and drop ----

  protected onTaskDropped(event: CdkDragDrop<TaskItem[]>): void {
    const previousColumnId = event.previousContainer.id;
    const targetColumnId = event.container.id;

    if (previousColumnId === targetColumnId) {
      const column = this.columns().find((c) => c.id === targetColumnId);
      if (!column) return;

      const reordered = [...column.tasks];
      moveItemInArray(reordered, event.previousIndex, event.currentIndex);

      this.columns.update((cols) =>
        cols.map((c) => (c.id === targetColumnId ? { ...c, tasks: reordered } : c)),
      );

      const movedTask = reordered[event.currentIndex];
      this.tasksService
        .move(movedTask.id, { targetColumnId, newSortOrder: event.currentIndex })
        .subscribe({
          error: () => {
            this.toast.error('Could not save new order.');
            this.loadBoard(false);
          },
        });
      return;
    }

    const sourceColumn = this.columns().find((c) => c.id === previousColumnId);
    if (!sourceColumn) return;

    const movedTask = sourceColumn.tasks[event.previousIndex];

    const updatedColumns = this.columns().map((c) => {
      if (c.id === previousColumnId) {
        const newTasks = [...c.tasks];
        newTasks.splice(event.previousIndex, 1);
        return { ...c, tasks: newTasks };
      }
      if (c.id === targetColumnId) {
        const newTasks = [...c.tasks];
        newTasks.splice(event.currentIndex, 0, movedTask);
        return { ...c, tasks: newTasks };
      }
      return c;
    });

    this.columns.set(updatedColumns);

    this.tasksService
      .move(movedTask.id, { targetColumnId, newSortOrder: event.currentIndex })
      .subscribe({
        error: () => {
          this.toast.error('Could not move task.');
          this.loadBoard(false);
        },
      });
  }

  protected getColumnIds(): string[] {
    return this.columns().map((c) => c.id);
  }

  protected onOpenTask(task: TaskItem): void {
    this.router.navigate(['/tasks', task.id]);
  }
}
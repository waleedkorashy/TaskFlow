export interface TaskItem {
  id: string;
  boardColumnId: string;
  title: string;
  description: string | null;
  sortOrder: number;
  dueDate: string | null;
  assigneeId: string | null;
  assigneeName: string | null;
  createdAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string | null;
  dueDate: string | null;
  assigneeId: string | null;
}

export interface MoveTaskRequest {
  targetColumnId: string;
  newSortOrder: number;
}
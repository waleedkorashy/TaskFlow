import { Label } from './label.models';

export interface CreateTaskRequest {
  title: string;
  description?: string | null;
  dueDate?: string | null;
  assigneeId?: string | null;
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string | null;
  dueDate?: string | null;
  assigneeId?: string | null;
  boardColumnId?: string;
  sortOrder?: number;
}

export interface MoveTaskRequest {
  targetColumnId: string;
  newSortOrder?: number;
}

export interface TaskItem {
  id: string;
  boardColumnId: string;
  boardId: string;
  projectId: string;
  title: string;
  description: string | null;
  sortOrder: number;
  dueDate: string | null;
  assigneeId: string | null;
  assigneeName: string | null;
  createdAt: string;
  labels: Label[];
}
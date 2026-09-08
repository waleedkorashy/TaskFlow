import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TaskItem, CreateTaskRequest, MoveTaskRequest } from '../models/task.models';

@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getByColumn(columnId: string): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(`${this.apiUrl}/columns/${columnId}/tasks`);
  }

  create(columnId: string, request: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(`${this.apiUrl}/columns/${columnId}/tasks`, request);
  }

  move(taskId: string, request: MoveTaskRequest): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${this.apiUrl}/tasks/${taskId}/move`, request);
  }

  delete(taskId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/tasks/${taskId}`);
  }

  getOne(taskId: string): Observable<TaskItem> {
  return this.http.get<TaskItem>(`${this.apiUrl}/tasks/${taskId}`);
}

update(taskId: string, request: CreateTaskRequest): Observable<TaskItem> {
  return this.http.put<TaskItem>(`${this.apiUrl}/tasks/${taskId}`, request);
}
}
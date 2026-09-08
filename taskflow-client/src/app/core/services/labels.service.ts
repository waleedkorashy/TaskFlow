import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Label, CreateLabelRequest } from '../models/label.models';

@Injectable({ providedIn: 'root' })
export class LabelsService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getByProject(projectId: string): Observable<Label[]> {
    return this.http.get<Label[]>(`${this.apiUrl}/projects/${projectId}/labels`);
  }

  create(projectId: string, request: CreateLabelRequest): Observable<Label> {
    return this.http.post<Label>(`${this.apiUrl}/projects/${projectId}/labels`, request);
  }

  update(labelId: string, request: CreateLabelRequest): Observable<Label> {
    return this.http.put<Label>(`${this.apiUrl}/labels/${labelId}`, request);
  }

  delete(labelId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/labels/${labelId}`);
  }

  attachToTask(taskId: string, labelId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/tasks/${taskId}/labels/${labelId}`, {});
  }

  detachFromTask(taskId: string, labelId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/tasks/${taskId}/labels/${labelId}`);
  }
}
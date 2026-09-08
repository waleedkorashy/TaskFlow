import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Project, CreateProjectRequest, ProjectMember } from '../models/project.models';

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  private readonly apiUrl = `${environment.apiUrl}/projects`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Project[]> {
    return this.http.get<Project[]>(this.apiUrl);
  }

  getOne(id: string): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateProjectRequest): Observable<Project> {
    return this.http.post<Project>(this.apiUrl, request);
  }

  update(id: string, request: CreateProjectRequest): Observable<Project> {
    return this.http.put<Project>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getMembers(projectId: string): Observable<ProjectMember[]> {
  return this.http.get<ProjectMember[]>(`${this.apiUrl}/${projectId}/members`);
  }

  removeMember(projectId: string, memberUserId: string): Observable<void> {
  return this.http.delete<void>(`${this.apiUrl}/${projectId}/members/${memberUserId}`);
  }

  leaveProject(projectId: string): Observable<void> {
  return this.http.post<void>(`${this.apiUrl}/${projectId}/leave`, {});
}
}
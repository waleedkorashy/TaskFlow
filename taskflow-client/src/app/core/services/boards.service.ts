import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Board, BoardColumn, CreateBoardRequest, CreateColumnRequest } from '../models/board.models';

@Injectable({ providedIn: 'root' })
export class BoardsService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getByProject(projectId: string): Observable<Board[]> {
    return this.http.get<Board[]>(`${this.apiUrl}/projects/${projectId}/boards`);
  }

  getOne(boardId: string): Observable<Board> {
    return this.http.get<Board>(`${this.apiUrl}/boards/${boardId}`);
  }

  create(projectId: string, request: CreateBoardRequest): Observable<Board> {
    return this.http.post<Board>(`${this.apiUrl}/projects/${projectId}/boards`, request);
  }

  createColumn(boardId: string, request: CreateColumnRequest): Observable<BoardColumn> {
    return this.http.post<BoardColumn>(`${this.apiUrl}/boards/${boardId}/columns`, request);
  }
  renameColumn(columnId: string, request: CreateColumnRequest): Observable<BoardColumn> {
  return this.http.put<BoardColumn>(`${this.apiUrl}/columns/${columnId}`, request);
}

deleteColumn(columnId: string): Observable<void> {
  return this.http.delete<void>(`${this.apiUrl}/columns/${columnId}`);
}

update(boardId: string, request: CreateBoardRequest): Observable<Board> {
  return this.http.put<Board>(`${this.apiUrl}/boards/${boardId}`, request);
}

delete(boardId: string): Observable<void> {
  return this.http.delete<void>(`${this.apiUrl}/boards/${boardId}`);
}
}
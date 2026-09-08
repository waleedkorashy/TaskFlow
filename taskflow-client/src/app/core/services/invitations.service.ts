import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Invitation, CreateInvitationRequest, InvitationPreview } from '../models/invitation.models';

@Injectable({ providedIn: 'root' })
export class InvitationsService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  invite(projectId: string, request: CreateInvitationRequest): Observable<Invitation> {
    return this.http.post<Invitation>(`${this.apiUrl}/projects/${projectId}/invitations`, request);
  }

  getPending(projectId: string): Observable<Invitation[]> {
    return this.http.get<Invitation[]>(`${this.apiUrl}/projects/${projectId}/invitations`);
  }

  revoke(invitationId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/invitations/${invitationId}`);
  }

  preview(token: string): Observable<InvitationPreview> {
    return this.http.get<InvitationPreview>(`${this.apiUrl}/invitations/${token}`);
  }

  accept(token: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/invitations/${token}/accept`, {});
  }
}
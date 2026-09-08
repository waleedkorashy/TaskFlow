import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

type VoidHandler = () => void;

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private currentBoardId: string | null = null;

  connectionState = signal<'disconnected' | 'connecting' | 'connected'>('disconnected');

  private handlers: VoidHandler[] = [];

  constructor(private authService: AuthService) {}

  private getHubUrl(): string {
    return `${environment.apiUrl.replace(/\/api$/, '')}/hubs/board`;
  }

  private ensureConnection(): signalR.HubConnection {
    if (this.hubConnection) return this.hubConnection;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.getHubUrl(), {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    const events = ['ColumnCreated', 'ColumnRenamed', 'ColumnDeleted', 'TaskCreated', 'TaskUpdated', 'TaskMoved', 'TaskDeleted'];
    events.forEach(eventName => {
      this.hubConnection!.on(eventName, () => {
        this.handlers.forEach(h => h());
      });
    });

    this.hubConnection.onreconnecting(() => this.connectionState.set('connecting'));
    this.hubConnection.onreconnected(() => {
      this.connectionState.set('connected');
      if (this.currentBoardId) {
        this.hubConnection!.invoke('JoinBoard', this.currentBoardId);
      }
    });
    this.hubConnection.onclose(() => this.connectionState.set('disconnected'));

    return this.hubConnection;
  }

  async joinBoard(boardId: string): Promise<void> {
    const connection = this.ensureConnection();

    if (connection.state === signalR.HubConnectionState.Disconnected) {
      this.connectionState.set('connecting');
      try {
        await connection.start();
        this.connectionState.set('connected');
      } catch (err) {
        console.error('SignalR connection failed:', err);
        this.connectionState.set('disconnected');
        return;
      }
    }

    this.currentBoardId = boardId;
    try {
      await connection.invoke('JoinBoard', boardId);
    } catch (err) {
      console.error('Could not join board group:', err);
    }
  }

  async leaveBoard(boardId: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) return;
    try {
      await this.hubConnection.invoke('LeaveBoard', boardId);
    } catch {
      // Connection may already be closing during navigation — safe to ignore.
    }
    this.currentBoardId = null;
  }

  onBoardChanged(handler: VoidHandler): void {
    this.handlers.push(handler);
  }

  clearHandlers(): void {
    this.handlers = [];
  }
}
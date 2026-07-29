import { Injectable, OnDestroy } from '@angular/core';
import { Subject, Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { Run } from '../models/run.model';
import { environment } from '../../environments/environment';

/**
 * Manages the SignalR connection to the RunHub.
 * Exposes run change events as an Observable and handles reconnection.
 */
@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {
  private connection: signalR.HubConnection;
  private readonly runChangedSubject = new Subject<Run>();

  readonly runChanged$: Observable<Run> = this.runChangedSubject.asObservable();

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalRHubUrl)
      .withAutomaticReconnect()
      .build();

    this.connection.on('RunChanged', (run: Run) => {
      this.runChangedSubject.next(run);
    });

    this.startConnection();
  }

  ngOnDestroy(): void {
    this.connection.stop();
  }

  private async startConnection(): Promise<void> {
    try {
      await this.connection.start();
    } catch {
      setTimeout(() => this.startConnection(), 5000);
    }
  }
}

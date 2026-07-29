import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';

interface HealthStatus {
  status: string;
}

@Component({
  selector: 'app-support',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatChipsModule],
  templateUrl: './support.component.html',
  styleUrl: './support.component.scss',
})
export class SupportComponent {
  readonly healthStatus = signal<string | null>(null);
  readonly checking = signal(false);

  constructor(private readonly http: HttpClient) {}

  checkHealth(): void {
    this.checking.set(true);
    this.http.get<HealthStatus>('/health').subscribe({
      next: (res) => {
        this.healthStatus.set(res.status);
        this.checking.set(false);
      },
      error: () => {
        this.healthStatus.set('Unhealthy');
        this.checking.set(false);
      },
    });
  }
}

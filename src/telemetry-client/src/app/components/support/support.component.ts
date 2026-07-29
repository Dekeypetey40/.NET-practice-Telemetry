import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-support',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatChipsModule],
  templateUrl: './support.component.html',
  styleUrl: './support.component.scss',
})
export class SupportComponent {
  readonly healthStatus = signal<string | null>(null);
  readonly lastError = signal<string | null>(null);
  readonly checking = signal(false);

  constructor(private readonly http: HttpClient) {}

  checkHealth(): void {
    this.checking.set(true);
    this.lastError.set(null);
    this.http.get('/api/health', { responseType: 'text' }).subscribe({
      next: (text) => {
        this.healthStatus.set(text.trim());
        this.checking.set(false);
      },
      error: (err) => {
        this.healthStatus.set('Unhealthy');
        this.lastError.set(err instanceof Error ? err.message : 'Request failed');
        this.checking.set(false);
      },
    });
  }
}

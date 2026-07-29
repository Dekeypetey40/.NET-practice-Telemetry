import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Run } from '../../models/run.model';
import { RunService } from '../../services/run.service';
import { SignalRService } from '../../services/signalr.service';
import { StateBadgePipe } from '../../pipes/state-badge.pipe';

@Component({
  selector: 'app-run-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatTableModule, MatButtonModule, MatIconModule, MatChipsModule, MatCardModule, MatProgressSpinnerModule,
    StateBadgePipe,
  ],
  templateUrl: './run-list.component.html',
  styleUrl: './run-list.component.scss',
})
export class RunListComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  readonly runs = signal<Run[]>([]);
  readonly loading = signal(true);
  readonly displayedColumns = ['sampleId', 'currentState', 'createdAt', 'actions'];

  readonly runCount = computed(() => this.runs().length);

  constructor(
    private readonly runService: RunService,
    private readonly signalR: SignalRService,
  ) {}

  ngOnInit(): void {
    this.loadRuns();

    this.signalR.runChanged$.pipe(takeUntil(this.destroy$)).subscribe((changed) => {
      this.runs.update((runs) => {
        const idx = runs.findIndex((r) => r.id === changed.id);
        if (idx >= 0) {
          const updated = [...runs];
          updated[idx] = changed;
          return updated;
        }
        return [changed, ...runs];
      });
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadRuns(): void {
    this.loading.set(true);
    this.runService.list().subscribe({
      next: (runs) => {
        this.runs.set(runs);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}

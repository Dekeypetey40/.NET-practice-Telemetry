import { Component, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Run, RunEvent, STATE_TRANSITIONS, RunState } from '../../models/run.model';
import { RunService } from '../../services/run.service';
import { SignalRService } from '../../services/signalr.service';
import { NotificationService } from '../../services/notification.service';
import { StateBadgePipe } from '../../pipes/state-badge.pipe';

@Component({
  selector: 'app-run-detail',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatCardModule, MatButtonModule, MatIconModule, MatChipsModule,
    MatListModule, MatDividerModule, MatProgressSpinnerModule, MatTooltipModule,
    StateBadgePipe,
  ],
  templateUrl: './run-detail.component.html',
  styleUrl: './run-detail.component.scss',
})
export class RunDetailComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private runId = '';

  readonly run = signal<Run | null>(null);
  readonly events = signal<RunEvent[]>([]);
  readonly loading = signal(true);
  readonly transitioning = signal(false);

  readonly availableActions = computed(() => {
    const r = this.run();
    if (!r) return [];
    return STATE_TRANSITIONS[r.currentState] ?? [];
  });

  constructor(
    private readonly route: ActivatedRoute,
    private readonly runService: RunService,
    private readonly signalR: SignalRService,
    private readonly notification: NotificationService,
  ) {}

  ngOnInit(): void {
    this.runId = this.route.snapshot.paramMap.get('id')!;
    this.loadRun();
    this.loadTimeline();

    this.signalR.runChanged$.pipe(takeUntil(this.destroy$)).subscribe((changed) => {
      if (changed.id === this.runId) {
        this.run.set(changed);
        this.loadTimeline();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  performTransition(action: string): void {
    if (this.transitioning()) return;
    this.transitioning.set(true);

    const call = this.getTransitionCall(action);
    call.subscribe({
      next: (updated) => {
        this.run.set(updated);
        this.notification.success(`Run ${action}${action.endsWith('e') ? 'd' : 'ed'} successfully`);
        this.loadTimeline();
        this.transitioning.set(false);
      },
      error: () => {
        this.loadRun();
        this.transitioning.set(false);
      },
    });
  }

  downloadBundle(): void {
    this.runService.downloadSupportBundle(this.runId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `support-bundle-${this.runId}.zip`;
        a.click();
        URL.revokeObjectURL(url);
      },
    });
  }

  getActionIcon(action: string): string {
    const icons: Record<string, string> = {
      queue: 'queue',
      start: 'play_arrow',
      complete: 'check_circle',
      fail: 'error',
      cancel: 'cancel',
    };
    return icons[action] ?? 'arrow_forward';
  }

  getActionColor(action: string): string {
    if (action === 'fail' || action === 'cancel') return 'warn';
    return 'primary';
  }

  private loadRun(): void {
    this.runService.getById(this.runId).subscribe({
      next: (run) => {
        this.run.set(run);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private loadTimeline(): void {
    this.runService.getTimeline(this.runId).subscribe({
      next: (timeline) => this.events.set(timeline.events),
    });
  }

  private getTransitionCall(action: string) {
    switch (action) {
      case 'queue': return this.runService.queue(this.runId);
      case 'start': return this.runService.start(this.runId);
      case 'complete': return this.runService.complete(this.runId);
      case 'fail': return this.runService.fail(this.runId);
      case 'cancel': return this.runService.cancel(this.runId);
      default: throw new Error(`Unknown action: ${action}`);
    }
  }
}

import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Instrument } from '../../models/run.model';
import { InstrumentService } from '../../services/instrument.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-instrument-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, ReactiveFormsModule,
    MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule,
    MatTableModule, MatProgressSpinnerModule,
  ],
  templateUrl: './instrument-list.component.html',
  styleUrl: './instrument-list.component.scss',
})
export class InstrumentListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly instrumentService = inject(InstrumentService);
  private readonly notification = inject(NotificationService);
  private readonly router = inject(Router);

  readonly instruments = signal<Instrument[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly displayedColumns = ['name', 'type', 'serialNumber', 'status', 'actions'];

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(1)]],
    type: ['', [Validators.required, Validators.minLength(1)]],
    serialNumber: [''],
  });

  ngOnInit(): void {
    this.loadInstruments();
  }

  loadInstruments(): void {
    this.loading.set(true);
    this.instrumentService.list().subscribe({
      next: (items) => {
        this.instruments.set(items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  onCreate(): void {
    if (this.form.invalid || this.submitting()) return;
    this.submitting.set(true);

    this.instrumentService.create(this.form.getRawValue()).subscribe({
      next: (created) => {
        this.notification.success(`Instrument created (${created.name})`);
        this.form.reset({ name: '', type: '', serialNumber: '' });
        this.submitting.set(false);
        this.loadInstruments();
      },
      error: () => this.submitting.set(false),
    });
  }

  useForNewRun(instrument: Instrument): void {
    this.router.navigate(['/runs/new'], { queryParams: { instrumentId: instrument.id } });
  }
}

import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RunService } from '../../services/run.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-run-create',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule,
  ],
  templateUrl: './run-create.component.html',
  styleUrl: './run-create.component.scss',
})
export class RunCreateComponent {
  private readonly fb = inject(FormBuilder);
  private readonly runService = inject(RunService);
  private readonly notification = inject(NotificationService);
  private readonly router = inject(Router);

  readonly submitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    instrumentId: ['', [Validators.required]],
    sampleId: ['', [Validators.required, Validators.minLength(1)]],
    methodName: [''],
    methodVersion: [''],
  });

  onSubmit(): void {
    if (this.form.invalid || this.submitting()) return;
    this.submitting.set(true);

    this.runService.create(this.form.getRawValue()).subscribe({
      next: (run) => {
        this.notification.success(`Run created (${run.sampleId})`);
        this.router.navigate(['/runs', run.id]);
      },
      error: () => this.submitting.set(false),
    });
  }
}

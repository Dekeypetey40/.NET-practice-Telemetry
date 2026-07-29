import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  constructor(private readonly snackBar: MatSnackBar) {}

  success(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: 'snackbar-success',
    });
  }

  error(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 6000,
      panelClass: 'snackbar-error',
    });
  }

  conflict(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 8000,
      panelClass: 'snackbar-conflict',
    });
  }
}

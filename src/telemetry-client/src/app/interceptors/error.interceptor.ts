import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

/**
 * Global HTTP error interceptor.
 * - 409 Conflict: shows the server's state-machine violation message via snackbar.
 * - Other errors: shows a generic error message.
 * Always re-throws so component-level handlers can also react (e.g. re-fetch state).
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notification = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 409) {
        const message = extractMessage(error) || 'State conflict: the run may have changed.';
        notification.conflict(message);
      } else if (error.status === 404) {
        notification.error('Resource not found.');
      } else if (error.status >= 500) {
        notification.error('Server error. Please try again later.');
      } else if (error.status >= 400) {
        const message = extractMessage(error) || 'Invalid request.';
        notification.error(message);
      }
      return throwError(() => error);
    }),
  );
};

function extractMessage(error: HttpErrorResponse): string | null {
  if (typeof error.error === 'string') return error.error;
  // API ExceptionHandlingMiddleware returns { error: "..." }
  if (error.error?.error && typeof error.error.error === 'string') return error.error.error;
  if (error.error?.message) return error.error.message;
  if (error.error?.title) return error.error.title;
  return null;
}

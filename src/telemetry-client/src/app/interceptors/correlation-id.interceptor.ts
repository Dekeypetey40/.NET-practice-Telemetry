import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Attaches a unique X-Correlation-Id header to every outgoing HTTP request.
 * Uses crypto.randomUUID() for a standards-compliant UUID v4.
 */
export const correlationIdInterceptor: HttpInterceptorFn = (req, next) => {
  const cloned = req.clone({
    setHeaders: { 'X-Correlation-Id': crypto.randomUUID() },
  });
  return next(cloned);
};

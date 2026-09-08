import { ErrorHandler, Injectable } from '@angular/core';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  handleError(error: unknown): void {
    console.error('Unexpected application error:', error);
    // In a real production app, this is where you'd send the error to a logging
    // service (e.g. Sentry). For now, logging to console keeps the app alive
    // instead of silently failing, and gives you something to check if a user reports a bug.
  }
}
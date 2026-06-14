import { HttpErrorResponse } from '@angular/common/http';

export function getApiErrorMessage(error: unknown, fallback = 'An error occurred'): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  const body = error.error;
  if (typeof body === 'string' && body.trim()) {
    return body;
  }

  if (body && typeof body === 'object') {
    if (typeof body.detail === 'string' && body.detail.trim()) {
      return body.detail;
    }
    if (typeof body.title === 'string' && body.title.trim()) {
      return body.title;
    }
    if (body.errors && typeof body.errors === 'object') {
      const messages = Object.values(body.errors)
        .flat()
        .filter((m): m is string => typeof m === 'string' && m.trim().length > 0);
      if (messages.length > 0) {
        return messages.join(', ');
      }
    }
  }

  return fallback;
}

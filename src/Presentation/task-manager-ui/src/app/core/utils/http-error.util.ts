import { HttpErrorResponse } from '@angular/common/http';

export function extractErrorMessage(err: HttpErrorResponse, fallback: string): string {
  if (err.status === 0) {
    return 'Could not reach the server. Is the API running?';
  }
  return err.error?.message ?? fallback;
}

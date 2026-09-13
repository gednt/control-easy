import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpEvent, HttpParams, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * Discriminator for the entity a photo is attached to.
 * Mirrors the categories enforced by the entry-log / consent policy.
 *
 * Note: as of Phase 12 the Photos API has no entity-binding column, so the
 * frontend keeps an in-memory + localStorage cache keyed by `${type}:${id}`.
 * The enum exists so future backend binding (Phase 13+) can be plugged in
 * without changing component contracts.
 */
export type PhotoEntityType = 'resident' | 'visitor' | 'vehicle' | 'service-provider';

export interface PhotoResponse {
  id: string;
  tenantId: string;
  filePath: string;
  thumbnailPath: string | null;
  mimeType: string;
  sizeBytes: number;
  capturedAtUtc: string | null;
  createdAtUtc: string;
  deletedAtUtc: string | null;
}

/**
 * Thin handwritten service for the Photos module (Phase 11 endpoints).
 *
 * Endpoints used:
 *   POST   /api/v1/photos        — multipart upload (field "file")
 *   GET    /api/v1/photos/{id}   — source blob (photos.read)
 *   DELETE /api/v1/photos/{id}   — soft-delete (photos.delete)
 *
 * Not yet provided by the backend (Phase 12 deviation):
 *   GET    /api/v1/photos/{id}/thumbnail  — display uses the source blob
 *                                            with `object-fit: cover` instead
 *   GET    /api/v1/photos?entityType=…    — list endpoint; binding is
 *                                            client-side via PhotoBindingCache
 */
@Injectable({ providedIn: 'root' })
export class PhotosApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/photos';

  upload(blob: Blob, fileName = 'photo.jpg'): Observable<PhotoResponse> {
    const formData = new FormData();
    formData.append('file', blob, fileName);
    return this.http.post<PhotoResponse>(this.baseUrl, formData);
  }

  /** Upload with progress events (used by the capture modal progress strip). */
  uploadWithProgress(blob: Blob, fileName = 'photo.jpg'): Observable<HttpEvent<PhotoResponse>> {
    const formData = new FormData();
    formData.append('file', blob, fileName);
    const req = new HttpRequest('POST', this.baseUrl, formData, { reportProgress: true });
    return this.http.request<PhotoResponse>(req);
  }

  get(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}`, { responseType: 'blob' });
  }

  /** Direct URL for use in <img src> — avoids blob-URL lifecycle management. */
  getSourceUrl(id: string): string {
    return `${this.baseUrl}/${id}`;
  }

  /**
   * Returns a Promise that resolves with the raw Blob. Convenience wrapper for
   * callers that need an `await` instead of subscribing.
   */
  getAsBlob(id: string): Promise<Blob> {
    return new Promise<Blob>((resolve, reject) => {
      this.get(id).subscribe({ next: resolve, error: reject });
    });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /**
   * Returns a paginated list of photos. The backend does not currently expose
   * this endpoint; callers should fall back to PhotoBindingCache. Kept as a
   * forward-compatible stub so adding the backend list later is non-breaking.
   */
  list(_skip = 0, _take = 50, _filter?: { entityType?: PhotoEntityType; entityId?: string }): Observable<PhotoResponse[]> {
    let params = new HttpParams().set('skip', _skip.toString()).set('take', _take.toString());
    if (_filter?.entityType) params = params.set('entityType', _filter.entityType);
    if (_filter?.entityId) params = params.set('entityId', _filter.entityId);
    return this.http.get<PhotoResponse[]>(this.baseUrl, { params });
  }
}

import { Injectable, signal } from '@angular/core';
import type { PhotoEntityType, PhotoResponse } from './photos-api.service';

const STORAGE_KEY = 'ce.photoBindings.v1';

interface BindingEntry {
  entityType: PhotoEntityType;
  entityId: string;
  photos: PhotoResponse[];
  updatedAt: string;
}

type BindingMap = Record<string, BindingEntry>;

function keyFor(entityType: PhotoEntityType, entityId: string): string {
  return `${entityType}:${entityId}`;
}

/**
 * Phase 12 deviation: the backend Photos API does not expose an entity-binding
 * column or a list endpoint. This service persists a frontend-only binding map
 * (keyed by `${entityType}:${entityId}`) in localStorage so the UI can show
 * photos attached to a given entity across page reloads.
 *
 * When the backend ships entity binding + a list endpoint, this cache can
 * become a write-through proxy to the API.
 */
@Injectable({ providedIn: 'root' })
export class PhotoBindingCacheService {
  private readonly bindings = signal<BindingMap>(this.load());

  list(entityType: PhotoEntityType, entityId: string): PhotoResponse[] {
    return this.bindings()[keyFor(entityType, entityId)]?.photos ?? [];
  }

  setAll(entityType: PhotoEntityType, entityId: string, photos: PhotoResponse[]): void {
    const k = keyFor(entityType, entityId);
    const updated: BindingMap = {
      ...this.bindings(),
      [k]: { entityType, entityId, photos, updatedAt: new Date().toISOString() },
    };
    this.bindings.set(updated);
    this.persist(updated);
  }

  add(entityType: PhotoEntityType, entityId: string, photo: PhotoResponse): PhotoResponse[] {
    const existing = this.list(entityType, entityId);
    const next = [photo, ...existing.filter((p) => p.id !== photo.id)];
    this.setAll(entityType, entityId, next);
    return next;
  }

  remove(entityType: PhotoEntityType, entityId: string, photoId: string): PhotoResponse[] {
    const next = this.list(entityType, entityId).filter((p) => p.id !== photoId);
    this.setAll(entityType, entityId, next);
    return next;
  }

  clear(entityType: PhotoEntityType, entityId: string): void {
    const k = keyFor(entityType, entityId);
    const next = { ...this.bindings() };
    delete next[k];
    this.bindings.set(next);
    this.persist(next);
  }

  private load(): BindingMap {
    if (typeof localStorage === 'undefined') return {};
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return {};
      const parsed = JSON.parse(raw) as BindingMap;
      return parsed && typeof parsed === 'object' ? parsed : {};
    } catch {
      return {};
    }
  }

  private persist(map: BindingMap): void {
    if (typeof localStorage === 'undefined') return;
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(map));
    } catch {
      // localStorage may be unavailable (private mode); swallow.
    }
  }
}

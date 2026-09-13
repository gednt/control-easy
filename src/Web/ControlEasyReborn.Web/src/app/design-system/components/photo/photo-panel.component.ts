import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  CeButtonComponent,
  CeIconComponent,
  CeModalComponent,
  CePhotoCaptureComponent,
  CePhotoGalleryComponent,
  ToastService,
} from '../..';
import { PhotosApiService, type PhotoEntityType, type PhotoResponse } from '../../../features/photos/photos-api.service';
import { PhotoBindingCacheService } from '../../../features/photos/photo-binding-cache.service';
import { getApiErrorMessage } from '../../../core/utils/api-error.util';

export interface PhotoPanelEntity {
  id: string;
  displayName: string;
}

/**
 * Reusable photo panel that can be embedded into any entity detail modal.
 * Wraps ce-photo-gallery + ce-photo-capture + the Phase-12 frontend binding
 * cache so each entity type (resident/visitor/vehicle/service-provider) can
 * show its photos with a single component.
 *
 * Phase 12 deviation: the backend Photos API has no entity binding column
 * nor a list endpoint, so photos are persisted in localStorage via
 * PhotoBindingCacheService. When the backend grows the binding this
 * component becomes a thin wrapper over photosApi.list() / entity upload.
 */
@Component({
  selector: 'ce-photo-panel',
  standalone: true,
  imports: [
    CeButtonComponent,
    CeIconComponent,
    CeModalComponent,
    CePhotoCaptureComponent,
    CePhotoGalleryComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="photo-panel">
      <ce-photo-gallery
        [photos]="photos()"
        [canAdd]="canAdd()"
        [canDelete]="canDelete()"
        (addRequested)="openCapture()"
        (photoDeleted)="onPhotoDeleted($event)"
      />
    </div>

    @if (showCapture() && entity()) {
      <ce-photo-capture
        [entityType]="entityType()"
        [entityId]="entity()!.id"
        mode="camera"
        [open]="showCapture()"
        (closed)="showCapture.set(false)"
        (photoUploaded)="onPhotoUploaded($event)"
      />
    }
  `,
  styles: [
    `
      .photo-panel {
        min-height: 96px;
      }
    `,
  ],
})
export class CePhotoPanelComponent {
  private readonly photosApi = inject(PhotosApiService);
  private readonly bindings = inject(PhotoBindingCacheService);
  private readonly toast = inject(ToastService);

  entityType = input.required<PhotoEntityType>();
  entity = input<PhotoPanelEntity | null>(null);
  canAdd = input<boolean>(true);
  canDelete = input<boolean>(true);

  photosChanged = output<PhotoResponse[]>();

  showCapture = signal(false);

  photos = computed<PhotoResponse[]>(() => {
    const e = this.entity();
    if (!e) return [];
    return this.bindings.list(this.entityType(), e.id);
  });

  openCapture(): void {
    if (!this.canAdd() || !this.entity()) return;
    this.showCapture.set(true);
  }

  onPhotoUploaded(photo: PhotoResponse): void {
    const e = this.entity();
    if (!e) return;
    const next = this.bindings.add(this.entityType(), e.id, photo);
    this.toast.success('Photo uploaded');
    this.photosChanged.emit(next);
  }

  async onPhotoDeleted(photoId: string): Promise<void> {
    const e = this.entity();
    if (!e) return;
    try {
      await firstValueFrom(this.photosApi.delete(photoId));
      const next = this.bindings.remove(this.entityType(), e.id, photoId);
      this.toast.success('Photo deleted');
      this.photosChanged.emit(next);
    } catch {
      this.toast.error(getApiErrorMessage(undefined, 'Failed to delete photo'));
    }
  }
}

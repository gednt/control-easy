import {
  ChangeDetectionStrategy,
  Component,
  effect,
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
 * Wraps ce-photo-gallery + ce-photo-capture for each supported entity. The
 * local cache only provides an offline fallback; the API is authoritative.
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

  photos = signal<PhotoResponse[]>([]);

  constructor() {
    effect(() => {
      const entity = this.entity();
      const type = this.entityType();
      if (!entity) {
        this.photos.set([]);
        return;
      }
      this.photos.set(this.bindings.list(type, entity.id));
      this.photosApi.list(0, 50, { entityType: type, entityId: entity.id }).subscribe({
        next: (photos) => {
          if (this.entity()?.id !== entity.id || this.entityType() !== type) return;
          this.bindings.setAll(type, entity.id, photos);
          this.photos.set(photos);
          this.photosChanged.emit(photos);
        },
      });
    });
  }

  openCapture(): void {
    if (!this.canAdd() || !this.entity()) return;
    this.showCapture.set(true);
  }

  onPhotoUploaded(photo: PhotoResponse): void {
    const e = this.entity();
    if (!e) return;
    const next = this.bindings.add(this.entityType(), e.id, photo);
    this.photos.set(next);
    this.toast.success('Photo uploaded');
    this.photosChanged.emit(next);
  }

  async onPhotoDeleted(photoId: string): Promise<void> {
    const e = this.entity();
    if (!e) return;
    try {
      await firstValueFrom(this.photosApi.delete(photoId));
      const next = this.bindings.remove(this.entityType(), e.id, photoId);
      this.photos.set(next);
      this.toast.success('Photo deleted');
      this.photosChanged.emit(next);
    } catch (err) {
      this.toast.error(getApiErrorMessage(err, 'Failed to delete photo'));
    }
  }
}

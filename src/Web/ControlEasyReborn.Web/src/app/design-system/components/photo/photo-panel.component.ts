import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { firstValueFrom } from 'rxjs';
import {
  CeButtonComponent,
} from '../button/button.component';
import {
  CeIconComponent,
} from '../icon/icon.component';
import {
  CeModalComponent,
} from '../modal/modal.component';
import {
  CePhotoCaptureComponent,
} from './photo-capture.component';
import {
  CePhotoGalleryComponent,
} from './photo-gallery.component';
import {
  ToastService,
} from '../toast/toast.component';
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
  private readonly destroyRef = inject(DestroyRef);

  entityType = input.required<PhotoEntityType>();
  entity = input<PhotoPanelEntity | null>(null);
  canAdd = input<boolean>(true);
  canDelete = input<boolean>(true);

  photosChanged = output<PhotoResponse[]>();

  showCapture = signal(false);

  photos = signal<PhotoResponse[]>([]);

  constructor() {
    effect(
      () => {
        const entity = this.entity();
        const type = this.entityType();
        if (!entity) {
          this.photos.set([]);
          return;
        }
        // Read the cached snapshot inside untracked so this effect re-runs only
        // when entity/entityType change, not when bindings change.
        this.photos.set(untracked(() => this.bindings.list(type, entity.id)));
        // Trigger the API load; subscription is auto-cleaned on component destroy.
        this.photosApi
          .list(0, 50, { entityType: type, entityId: entity.id })
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: (photos) => {
              if (this.entity()?.id !== entity.id || this.entityType() !== type) return;
              untracked(() => this.bindings.setAll(type, entity.id, photos));
              untracked(() => this.photos.set(photos));
              untracked(() => this.photosChanged.emit(photos));
            },
            error: () => {
              // Preserve cached photos on network failure
            },
          });
      },
      { allowSignalWrites: true },
    );
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

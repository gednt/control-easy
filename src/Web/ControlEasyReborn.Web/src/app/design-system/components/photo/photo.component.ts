import { ChangeDetectionStrategy, Component, OnDestroy, computed, effect, inject, input, output, signal } from '@angular/core';
import { PhotosApiService } from '../../../features/photos/photos-api.service';

export type CePhotoSize = 'thumbnail' | 'source';

/**
 * Display a protected photo by id. Image requests cannot carry the SPA's
 * bearer token when made through a plain img URL, so content is retrieved by
 * the authenticated HTTP client and rendered with an object URL.
 */
@Component({
  selector: 'ce-photo',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (loading()) {
      <div
        class="ce-photo-skeleton"
        [style.width.px]="displaySize()"
        [style.height.px]="displaySize()"
        role="img"
        aria-label="Loading photo"
      ></div>
    } @else if (error()) {
      <div
        class="ce-photo-error"
        [style.width.px]="displaySize()"
        [style.height.px]="displaySize()"
        role="img"
        aria-label="Photo unavailable"
      >?</div>
    } @else {
      <img
        [src]="sourceUrl()!"
        [width]="displaySize()"
        [height]="displaySize()"
        loading="lazy"
        alt="Photo"
        [class.clickable]="clickable()"
        (click)="onClick()"
        (error)="onError()"
        (load)="onLoad()"
      />
    }
  `,
  styles: [
    `
      :host {
        display: inline-block;
        line-height: 0;
      }
      img {
        border-radius: var(--radius-md, 4px);
        object-fit: cover;
        display: block;
      }
      img.clickable {
        cursor: pointer;
      }
      .ce-photo-skeleton {
        background: var(--color-surface-alt, #eee);
        border-radius: var(--radius-md, 4px);
        animation: ce-photo-pulse 1.5s ease-in-out infinite;
      }
      .ce-photo-error {
        display: flex;
        align-items: center;
        justify-content: center;
        background: var(--color-surface-alt, #eee);
        color: var(--color-text-muted, #888);
        opacity: 0.6;
        border-radius: var(--radius-md, 4px);
        font-size: 1.5rem;
      }
      @keyframes ce-photo-pulse {
        0%, 100% { opacity: 1; }
        50% { opacity: 0.6; }
      }
      @media (prefers-reduced-motion: reduce) {
        .ce-photo-skeleton { animation: none; }
      }
    `,
  ],
})
export class CePhotoComponent implements OnDestroy {
  private readonly photosApi = inject(PhotosApiService);

  photoId = input.required<string>();
  /** Logical size; rendered as a square. Phase 11 has only the source endpoint. */
  size = input<CePhotoSize | number>('thumbnail');
  clickable = input<boolean>(true);
  /** Optional override of the default 128/800 dimensions for custom layouts. */
  pixelSize = input<number | null>(null);

  loading = signal(true);
  error = signal(false);
  sourceUrl = signal<string | null>(null);
  private objectUrl: string | null = null;

  photoClicked = output<void>();

  displaySize = computed<number>(() => {
    const override = this.pixelSize();
    if (override && override > 0) return override;
    const s = this.size();
    return s === 'source' ? 800 : 128;
  });

  constructor() {
    effect((onCleanup) => {
      const id = this.photoId();
      this.releaseObjectUrl();
      this.loading.set(true);
      this.error.set(false);
      const subscription = this.photosApi.get(id).subscribe({
        next: (blob) => {
          this.objectUrl = URL.createObjectURL(blob);
          this.sourceUrl.set(this.objectUrl);
          this.loading.set(false);
        },
        error: () => {
          this.sourceUrl.set(null);
          this.loading.set(false);
          this.error.set(true);
        },
      });
      onCleanup(() => {
        subscription.unsubscribe();
        this.releaseObjectUrl();
      });
    }, { allowSignalWrites: true });
  }

  onLoad(): void {
    this.loading.set(false);
    this.error.set(false);
  }

  onError(): void {
    this.loading.set(false);
    this.error.set(true);
  }

  ngOnDestroy(): void {
    this.releaseObjectUrl();
  }

  onClick(): void {
    if (this.clickable()) {
      this.photoClicked.emit();
    }
  }

  private releaseObjectUrl(): void {
    if (this.objectUrl) URL.revokeObjectURL(this.objectUrl);
    this.objectUrl = null;
    this.sourceUrl.set(null);
  }
}

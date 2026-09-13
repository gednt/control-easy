import { Component, ChangeDetectionStrategy, computed, input, output, signal } from '@angular/core';

export type CePhotoSize = 'thumbnail' | 'source';

/**
 * Display a photo by id. Uses the Phase 11 source endpoint directly; the
 * `size` input is preserved as a forward-compatible affordance for when the
 * backend grows a real /thumbnail route (Phase 12 deviation).
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
        [src]="srcUrl()"
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
export class CePhotoComponent {
  photoId = input.required<string>();
  /** Logical size; rendered as a square. Phase 11 has only the source endpoint. */
  size = input<CePhotoSize | number>('thumbnail');
  clickable = input<boolean>(true);
  /** Optional override of the default 128/800 dimensions for custom layouts. */
  pixelSize = input<number | null>(null);

  loading = signal(true);
  error = signal(false);

  photoClicked = output<void>();

  displaySize = computed<number>(() => {
    const override = this.pixelSize();
    if (override && override > 0) return override;
    const s = this.size();
    return s === 'source' ? 800 : 128;
  });

  srcUrl = computed<string>(() => {
    const s = this.size();
    if (s === 'source') {
      return `/api/v1/photos/${this.photoId()}`;
    }
    // Phase 12 deviation: backend has no /thumbnail endpoint yet. Use the
    // source; the <img> applies object-fit: cover to crop to the requested
    // displaySize. The `size` input is preserved so adding a real thumbnail
    // route later is non-breaking.
    return `/api/v1/photos/${this.photoId()}`;
  });

  onLoad(): void {
    this.loading.set(false);
    this.error.set(false);
  }

  onError(): void {
    this.loading.set(false);
    this.error.set(true);
  }

  onClick(): void {
    if (this.clickable()) {
      this.photoClicked.emit();
    }
  }
}

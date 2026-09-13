import {
  Component,
  ChangeDetectionStrategy,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CeModalComponent } from '../modal/modal.component';
import { CeButtonComponent } from '../button/button.component';
import { CeIconComponent } from '../icon/icon.component';
import { CeInputComponent } from '../input/input.component';
import { CePhotoCaptureComponent } from '../photo/photo-capture.component';
import { CeOverrideReasonComponent } from '../override-reason/override-reason.component';
import { ToastService } from '../toast/toast.component';
import {
  EntryLogService,
  type CreateEntryLogRequest,
  type EntryLogResponse,
  type EntryState,
  type OverrideReason,
  type SubjectType,
} from '../../../features/entry-log/entry-log.service';
import { ConsentPolicyService } from '../../../features/consent-policy/consent-policy.service';
import { getApiErrorMessage } from '../../../core/utils/api-error.util';

type WorkflowStep = 'tiles' | 'subject-info';
type TileAction = 'register' | 'denied' | 'gatehouse' | 'override';

interface TileDescriptor {
  action: TileAction;
  icon: 'user-check' | 'x-circle' | 'package' | 'alert-triangle';
  label: string;
  sub: string;
  cssClass: string;
}

const TILES: TileDescriptor[] = [
  { action: 'register', icon: 'user-check', label: 'Register entry', sub: 'with consent', cssClass: 'tile primary' },
  { action: 'denied', icon: 'x-circle', label: 'Entry denied', sub: 'consent refused', cssClass: 'tile denied' },
  { action: 'gatehouse', icon: 'package', label: 'Gatehouse only', sub: 'package drop', cssClass: 'tile gatehouse' },
  { action: 'override', icon: 'alert-triangle', label: 'Override', sub: 'emergency / vouched', cssClass: 'tile override' },
];

/**
 * Gatehouse entry workflow. Hosts the 4-tile selection screen and the
 * subsequent subject-info form. Wires `ce-photo-capture` for the auto-camera
 * path and `ce-override-reason` for the override decision modal.
 */
@Component({
  selector: 'ce-entry-workflow',
  standalone: true,
  imports: [
    CeModalComponent,
    CeButtonComponent,
    CeIconComponent,
    CeInputComponent,
    CePhotoCaptureComponent,
    CeOverrideReasonComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ce-modal
      [open]="open()"
      title="New entry"
      size="lg"
      (closed)="close()"
    >
      @if (step() === 'tiles') {
        <div class="tile-grid">
          @for (tile of tiles; track tile.action) {
            <button
              type="button"
              class="tile-button"
              [attr.data-action]="tile.action"
              [attr.aria-label]="tile.label"
              [disabled]="loading()"
              (click)="onTileTap(tile.action)"
            >
              <span [class]="tile.cssClass">
                <ce-icon [name]="tile.icon" [size]="32" />
                <span class="label">{{ tile.label }}</span>
                <span class="sub">{{ tile.sub }}</span>
              </span>
            </button>
          }
        </div>
      } @else if (step() === 'subject-info') {
        <div class="subject-form">
          <ce-input
            label="Name (optional)"
            placeholder="Visitor name"
            [value]="subjectName()"
            (valueChange)="subjectName.set($event)"
          />
          <ce-input
            label="Document (optional)"
            placeholder="CPF or RG"
            [value]="subjectDocument()"
            (valueChange)="subjectDocument.set($event)"
          />
          <div class="actions">
            <ce-button variant="ghost" size="md" (click)="back()" [disabled]="loading()">
              Back
            </ce-button>
            <ce-button
              variant="primary"
              size="md"
              (click)="continue()"
              [loading]="loading()"
            >
              {{ loading() ? 'Logging…' : 'Continue' }}
            </ce-button>
          </div>
        </div>
      }

      @if (showCapture() && pendingState()) {
        <ce-photo-capture
          [entityType]="photoEntityType()"
          [entityId]="subjectName() || 'unknown'"
          mode="camera"
          [open]="showCapture()"
          (closed)="onCaptureClosed()"
          (photoUploaded)="onPhotoUploaded($event)"
        />
      }

      <ce-override-reason
        [open]="showOverrideReason()"
        (reasonSelected)="onOverrideReason($event)"
        (closed)="showOverrideReason.set(false)"
      />
    </ce-modal>
  `,
  styles: [
    `
      :host { display: contents; }
      .tile-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: var(--space-3, 12px);
        padding: var(--space-1, 4px) 0;
      }
      .tile-button {
        background: transparent;
        border: 0;
        padding: 0;
        cursor: pointer;
        font-family: inherit;
        min-height: 160px;
        display: block;
        transition: transform 100ms ease-out;
      }
      .tile-button:active:not(:disabled) {
        transform: scale(0.95);
      }
      .tile-button:focus-visible {
        outline: 2px solid var(--color-primary, #0066cc);
        outline-offset: 2px;
      }
      .tile-button:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
      .tile {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: var(--space-2, 8px);
        padding: var(--space-4, 16px);
        border-radius: var(--radius-lg, 8px);
        width: 100%;
        height: 100%;
      }
      .tile.primary { background: var(--color-primary, #0066cc); color: white; }
      .tile.denied { background: var(--color-danger, #991b1b); color: white; }
      .tile.gatehouse {
        background: var(--color-surface-alt, #e5e7eb);
        color: var(--color-text-primary, #111827);
      }
      .tile.override { background: var(--color-warning, #f59e0b); color: white; }
      .label {
        font-size: var(--font-size-base, 18px);
        font-weight: var(--font-weight-semibold, 600);
      }
      .sub {
        font-size: var(--font-size-xs, 13px);
        opacity: 0.85;
      }
      .subject-form {
        display: flex;
        flex-direction: column;
        gap: var(--space-3, 12px);
        padding: var(--space-1, 4px) 0;
      }
      .actions {
        display: flex;
        justify-content: space-between;
        margin-top: var(--space-2, 8px);
      }
      @media (prefers-reduced-motion: reduce) {
        .tile-button:active:not(:disabled) { transform: none; }
      }
    `,
  ],
})
export class CeEntryWorkflowComponent {
  private readonly entryLogService = inject(EntryLogService);
  private readonly consentPolicyService = inject(ConsentPolicyService);
  private readonly toast = inject(ToastService);

  open = input<boolean>(false);
  defaultCategory = input<SubjectType>('visitor');

  entryLogged = output<EntryLogResponse>();
  closed = output<void>();

  readonly tiles = TILES;

  step = signal<WorkflowStep>('tiles');
  pendingState = signal<EntryState | null>(null);
  showCapture = signal(false);
  showOverrideReason = signal(false);
  loading = signal(false);

  subjectName = signal('');
  subjectDocument = signal('');
  selectedCategory = signal<SubjectType>('visitor');
  photoId = signal<string | null>(null);
  pendingOverrideReason = signal<OverrideReason | null>(null);

  /** CePhotoCapture expects PhotoEntityType values: resident/visitor/vehicle/service-provider. */
  photoEntityType = computed<'resident' | 'visitor' | 'vehicle' | 'service-provider'>(() => {
    const cat = this.selectedCategory();
    if (cat === 'dweller') return 'resident';
    if (cat === 'service-provider') return 'service-provider';
    if (cat === 'vehicle') return 'vehicle';
    return 'visitor';
  });

  constructor() {
    // Reset the workflow when the modal opens/closes externally.
    effect(
      () => {
        const isOpen = this.open();
        if (isOpen) {
          this.reset();
        }
      },
      { allowSignalWrites: true },
    );
  }

  async onTileTap(action: TileAction): Promise<void> {
    if (action === 'register') {
      this.pendingState.set('entered_with_consent');
      this.selectedCategory.set('visitor');
      try {
        const policies = await firstValueFrom(this.consentPolicyService.getAll());
        const policy = policies.find((p) => p.subjectCategory === 'visitors');
        if (policy?.photoRequired) {
          this.showCapture.set(true);
        } else {
          this.step.set('subject-info');
        }
      } catch {
        // Backend unreachable — fall through to subject info so the user
        // isn't stranded; the create call will surface the real error.
        this.step.set('subject-info');
      }
    } else if (action === 'denied') {
      this.pendingState.set('denied');
      this.selectedCategory.set('visitor');
      this.step.set('subject-info');
    } else if (action === 'gatehouse') {
      this.pendingState.set('gatehouse_only');
      this.selectedCategory.set('service-provider');
      this.step.set('subject-info');
    } else if (action === 'override') {
      this.pendingState.set('entered_override');
      this.showOverrideReason.set(true);
    }
  }

  back(): void {
    this.step.set('tiles');
    this.subjectName.set('');
    this.subjectDocument.set('');
    this.photoId.set(null);
    this.pendingOverrideReason.set(null);
  }

  close(): void {
    this.reset();
    this.closed.emit();
  }

  onPhotoUploaded(photo: { id: string }): void {
    this.photoId.set(photo.id);
    this.showCapture.set(false);
    this.step.set('subject-info');
  }

  onCaptureClosed(): void {
    // If the user cancelled the camera without uploading, return to tiles.
    if (!this.photoId()) {
      this.showCapture.set(false);
      this.reset();
    }
  }

  onOverrideReason(reason: OverrideReason): void {
    this.pendingOverrideReason.set(reason);
    this.showOverrideReason.set(false);
    this.step.set('subject-info');
  }

  async continue(): Promise<void> {
    const state = this.pendingState();
    if (!state) return;
    this.loading.set(true);
    try {
      const request: CreateEntryLogRequest = {
        entryState: state,
        subjectType: this.selectedCategory(),
        subjectName: this.subjectName() || undefined,
        subjectDocument: this.subjectDocument() || undefined,
        photoId: this.photoId() || undefined,
        overrideReason: this.pendingOverrideReason() || undefined,
      };
      const entry = await firstValueFrom(this.entryLogService.create(request));
      this.toast.success('Entry logged');
      this.entryLogged.emit(entry);
      this.close();
    } catch (err) {
      this.toast.error(getApiErrorMessage(err, 'Failed to log entry. Retry?'));
      this.loading.set(false);
    }
  }

  private reset(): void {
    this.step.set('tiles');
    this.pendingState.set(null);
    this.showCapture.set(false);
    this.showOverrideReason.set(false);
    this.subjectName.set('');
    this.subjectDocument.set('');
    this.photoId.set(null);
    this.pendingOverrideReason.set(null);
    this.loading.set(false);
    this.selectedCategory.set(this.defaultCategory());
  }
}
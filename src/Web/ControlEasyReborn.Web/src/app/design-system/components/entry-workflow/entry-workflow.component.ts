import { Component, ChangeDetectionStrategy, computed, effect, inject, input, output, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CeModalComponent } from '../modal/modal.component';
import { CeButtonComponent } from '../button/button.component';
import { CeIconComponent } from '../icon/icon.component';
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
import { ResidentsApiService, type ResidentResponse } from '../../../features/residents/residents-api.service';
import { getApiErrorMessage } from '../../../core/utils/api-error.util';

type WorkflowStep = 'tiles' | 'subject-info';
type TileAction = 'register' | 'exit' | 'denied' | 'gatehouse' | 'override';

interface TileDescriptor {
  action: TileAction;
  icon: 'user-check' | 'log-out' | 'x-circle' | 'package' | 'alert-triangle';
  label: string;
  sub: string;
  cssClass: string;
}

const TILES: TileDescriptor[] = [
  { action: 'register', icon: 'user-check', label: 'Register entry', sub: 'with consent', cssClass: 'tile primary' },
  { action: 'exit', icon: 'log-out', label: 'Register exit', sub: 'resident / vehicle', cssClass: 'tile exit' },
  { action: 'denied', icon: 'x-circle', label: 'Entry denied', sub: 'consent refused', cssClass: 'tile denied' },
  { action: 'gatehouse', icon: 'package', label: 'Gatehouse only', sub: 'package drop', cssClass: 'tile gatehouse' },
  {
    action: 'override',
    icon: 'alert-triangle',
    label: 'Override',
    sub: 'emergency / vouched',
    cssClass: 'tile override',
  },
];

function toSubjectCategory(subjectType: SubjectType): 'dwellers' | 'visitors' | 'service-providers' | 'vehicles' {
  switch (subjectType) {
    case 'dweller':
      return 'dwellers';
    case 'visitor':
      return 'visitors';
    case 'service_provider':
      return 'service-providers';
    case 'vehicle':
      return 'vehicles';
  }
}

/**
 * Gatehouse access workflow. Hosts the entry/exit selection screen and the
 * subsequent subject-info form. Wires `ce-photo-capture` for the auto-camera
 * path and `ce-override-reason` for the override decision modal.
 */
@Component({
  selector: 'ce-entry-workflow',
  standalone: true,
  imports: [CeModalComponent, CeButtonComponent, CeIconComponent, CePhotoCaptureComponent, CeOverrideReasonComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ce-modal [open]="open()" title="Access movement" size="lg" (closed)="close()">
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
          <div class="field">
            <span class="field-label">Category</span>
            <div class="category-selector" role="radiogroup" aria-label="Subject category">
              <button
                type="button"
                class="category-chip"
                [class.active]="selectedCategory() === 'visitor'"
                [disabled]="!isCategoryAllowed('visitor')"
                (click)="onCategorySelect('visitor')"
              >
                Visitor
              </button>
              <button
                type="button"
                class="category-chip"
                [class.active]="selectedCategory() === 'dweller'"
                [disabled]="!isCategoryAllowed('dweller')"
                (click)="onCategorySelect('dweller')"
              >
                Resident
              </button>
              <button
                type="button"
                class="category-chip"
                [class.active]="selectedCategory() === 'service_provider'"
                [disabled]="!isCategoryAllowed('service_provider')"
                (click)="onCategorySelect('service_provider')"
              >
                Service Provider
              </button>
              <button
                type="button"
                class="category-chip"
                [class.active]="selectedCategory() === 'vehicle'"
                [disabled]="!isCategoryAllowed('vehicle')"
                (click)="onCategorySelect('vehicle')"
              >
                Vehicle
              </button>
            </div>
          </div>

          @if (selectedCategory() === 'dweller') {
            <section class="resident-lookup" aria-label="Resident lookup">
              <label class="field" for="resident-lookup">
                <span class="field-label">Find registered resident</span>
                <span class="field-hint">Search by CPF or resident ID (QR)</span>
              </label>
              <div class="lookup-row">
                <input
                  id="resident-lookup"
                  type="search"
                  class="ce-input"
                  placeholder="CPF or resident ID"
                  [value]="residentLookupQuery()"
                  (input)="onResidentLookupInput($any($event.target).value)"
                  (keydown.enter)="findResident()"
                />
                <ce-button variant="secondary" size="md" (click)="findResident()" [loading]="residentSearchLoading()">
                  Search
                </ce-button>
              </div>

              @if (residentSearchError()) {
                <p class="lookup-message error" role="alert">{{ residentSearchError() }}</p>
              }

              @if (residentSearchResults().length) {
                <div class="lookup-results" role="listbox" aria-label="Resident search results">
                  @for (resident of residentSearchResults(); track resident.id) {
                    <button type="button" class="lookup-result" role="option" (click)="selectResident(resident)">
                      <strong>{{ resident.name }}</strong>
                      <span>{{ resident.cpf }}</span>
                    </button>
                  }
                </div>
              }

              @if (selectedResident(); as resident) {
                <div class="selected-resident" aria-live="polite">
                  <ce-icon name="check-circle" [size]="16" />
                  <span
                    ><strong>{{ resident.name }}</strong> · {{ resident.cpf }}</span
                  >
                </div>
              }
            </section>
          } @else {
            <label class="field">
              <span class="field-label">Name (optional)</span>
              <input
                type="text"
                class="ce-input"
                [placeholder]="subjectNamePlaceholder()"
                [value]="subjectName()"
                (input)="subjectName.set($any($event.target).value)"
              />
            </label>

            <label class="field">
              <span class="field-label">Document (optional)</span>
              <input
                type="text"
                class="ce-input"
                placeholder="CPF or RG"
                [value]="subjectDocument()"
                (input)="subjectDocument.set($any($event.target).value)"
              />
            </label>
          }

          @if (
            (pendingState() === 'entered_with_consent' || pendingState() === 'entered_without_consent') && !photoId()
          ) {
            <div class="photo-prompt">
              <button type="button" class="photo-capture-trigger" (click)="openCapture()">
                <ce-icon name="camera" [size]="16" /> Capture photo
              </button>
            </div>
          } @else if (photoId()) {
            <div class="photo-status attached">
              <ce-icon name="check-circle" [size]="16" />
              <span>Photo attached</span>
            </div>
          }

          <div class="actions">
            <ce-button variant="ghost" size="md" (click)="back()" [disabled]="loading()"> Back </ce-button>
            <ce-button variant="primary" size="md" (click)="continue()" [loading]="loading()">
              {{ loading() ? 'Logging…' : 'Continue' }}
            </ce-button>
          </div>
        </div>
      }

      @if (showCapture() && pendingState()) {
        <ce-photo-capture
          [entityType]="photoEntityType()"
          [entityId]="subjectName() || 'unknown'"
          [mode]="captureMode()"
          [open]="showCapture()"
          (closed)="onCaptureClosed()"
          (modeChange)="onCaptureModeChange($event)"
          (photoUploaded)="onPhotoUploaded($event)"
        />
      }

      <ce-override-reason
        [open]="showOverrideReason()"
        (reasonSelected)="onOverrideReason($event)"
        (closed)="onOverrideClosed()"
      />
    </ce-modal>
  `,
  styles: [
    `
      :host {
        display: contents;
      }
      .tile-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 1px;
        padding: 0;
        background: var(--color-border, #c9c1b3);
        border-top: 3px solid var(--color-sidebar, #182a33);
      }
      .tile-button {
        background: transparent;
        border: 0;
        padding: 0;
        cursor: pointer;
        font-family: inherit;
        min-height: 172px;
        display: block;
        transition:
          background-color 150ms ease-out,
          transform 100ms ease-out;
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
        padding: var(--space-5, 20px);
        border-radius: 0;
        width: 100%;
        height: 100%;
      }
      .tile.primary {
        background: var(--color-primary, #a84d3d);
        color: white;
      }
      .tile.denied {
        background: var(--color-danger, #991b1b);
        color: white;
      }
      .tile.gatehouse {
        background: var(--color-surface, #fffdf7);
        color: var(--color-text-primary, #111827);
      }
      .tile.exit {
        background: var(--color-sidebar, #182a33);
        color: white;
      }
      .tile.override {
        background: #c28a2c;
        color: #1c2428;
      }
      .label {
        font-family: var(--font-family-display, Georgia, serif);
        font-size: var(--font-size-xl, 20px);
        font-weight: var(--font-weight-semibold, 600);
      }
      .sub {
        font-size: var(--font-size-xs, 13px);
        opacity: 0.78;
        font-family: var(--font-family-mono, monospace);
        letter-spacing: 0.05em;
      }
      .subject-form {
        display: flex;
        flex-direction: column;
        gap: var(--space-3, 12px);
        padding: var(--space-2, 8px) 0;
      }
      .field {
        display: flex;
        flex-direction: column;
        gap: var(--space-1, 4px);
      }
      .field-label {
        font-size: var(--font-size-sm, 14px);
        font-weight: var(--font-weight-medium, 500);
        color: var(--color-text-primary, #111827);
      }
      .field-hint {
        font-size: var(--font-size-xs, 12px);
        color: var(--color-text-secondary, #4b5563);
      }
      .category-selector {
        display: flex;
        gap: var(--space-2, 8px);
        flex-wrap: wrap;
      }
      .category-chip {
        padding: var(--space-2, 6px) var(--space-3, 12px);
        border: 1px solid var(--color-border, #e5e7eb);
        background: var(--color-surface, #fffdf7);
        color: var(--color-text-secondary, #4b5563);
        font-family: var(--font-family-mono, monospace);
        font-size: var(--font-size-xs, 12px);
        letter-spacing: 0.04em;
        text-transform: uppercase;
        cursor: pointer;
        transition: all 150ms ease;
      }
      .category-chip:hover:not(:disabled) {
        border-color: var(--color-primary, #a84d3d);
        color: var(--color-primary, #a84d3d);
      }
      .category-chip.active {
        background: var(--color-primary, #a84d3d);
        color: white;
        border-color: var(--color-primary, #a84d3d);
        font-weight: 600;
      }
      .category-chip:disabled {
        opacity: 0.4;
        cursor: not-allowed;
      }
      .ce-input {
        padding: var(--space-2, 8px) var(--space-3, 12px);
        border: 1px solid var(--color-border, #e5e7eb);
        border-radius: 0;
        font-family: inherit;
        font-size: var(--font-size-sm, 14px);
        background: var(--color-surface, #fffdf7);
        color: var(--color-text-primary, #111827);
        min-height: 2.5rem;
      }
      .ce-input:focus-visible {
        outline: 2px solid var(--color-primary, #0066cc);
        outline-offset: 1px;
        border-color: var(--color-primary, #0066cc);
      }
      .resident-lookup {
        display: grid;
        gap: var(--space-2, 8px);
        padding: var(--space-3, 12px);
        border: 1px solid var(--color-border, #e5e7eb);
        background: color-mix(in srgb, var(--color-primary, #a84d3d) 4%, var(--color-surface, #fffdf7));
      }
      .lookup-row {
        display: flex;
        gap: var(--space-2, 8px);
      }
      .lookup-row .ce-input {
        min-width: 0;
        flex: 1;
      }
      .lookup-message {
        margin: 0;
        font-size: var(--font-size-xs, 12px);
      }
      .lookup-message.error {
        color: var(--color-danger, #991b1b);
      }
      .lookup-results {
        display: grid;
        border: 1px solid var(--color-border, #e5e7eb);
      }
      .lookup-result {
        display: flex;
        align-items: baseline;
        justify-content: space-between;
        gap: var(--space-3, 12px);
        padding: var(--space-2, 8px) var(--space-3, 12px);
        border: 0;
        border-bottom: 1px solid var(--color-border, #e5e7eb);
        background: var(--color-surface, #fffdf7);
        color: var(--color-text-primary, #111827);
        cursor: pointer;
        font: inherit;
        text-align: left;
      }
      .lookup-result:last-child {
        border-bottom: 0;
      }
      .lookup-result:hover,
      .lookup-result:focus-visible {
        background: color-mix(in srgb, var(--color-primary, #a84d3d) 10%, var(--color-surface, #fffdf7));
      }
      .lookup-result span {
        font-family: var(--font-family-mono, monospace);
        font-size: var(--font-size-xs, 12px);
      }
      .selected-resident {
        display: inline-flex;
        align-items: center;
        gap: var(--space-2, 8px);
        color: var(--color-success, #16a34a);
        font-size: var(--font-size-sm, 14px);
      }
      .photo-capture-trigger {
        display: inline-flex;
        align-items: center;
        gap: var(--space-2, 8px);
        padding: var(--space-2, 8px) var(--space-3, 12px);
        border: 1px dashed var(--color-primary, #a84d3d);
        background: color-mix(in srgb, var(--color-primary, #a84d3d) 6%, transparent);
        color: var(--color-primary, #a84d3d);
        font-family: var(--font-family-mono, monospace);
        font-size: var(--font-size-xs, 12px);
        font-weight: 600;
        cursor: pointer;
        text-transform: uppercase;
      }
      .photo-capture-trigger:hover {
        background: color-mix(in srgb, var(--color-primary, #a84d3d) 12%, transparent);
      }
      .photo-status {
        display: inline-flex;
        align-items: center;
        gap: var(--space-2, 8px);
        font-size: var(--font-size-xs, 12px);
        font-weight: 600;
        color: var(--color-success, #16a34a);
      }
      .actions {
        display: flex;
        justify-content: space-between;
        margin-top: var(--space-2, 8px);
      }
      @media (prefers-reduced-motion: reduce) {
        .tile-button:active:not(:disabled) {
          transform: none;
        }
      }
    `,
  ],
})
export class CeEntryWorkflowComponent {
  private readonly entryLogService = inject(EntryLogService);
  private readonly consentPolicyService = inject(ConsentPolicyService);
  private readonly residentsApi = inject(ResidentsApiService);
  private readonly toast = inject(ToastService);

  open = input<boolean>(false);
  closeOnEntry = input(false);
  defaultCategory = input<SubjectType>('visitor');

  entryLogged = output<EntryLogResponse>();
  closed = output<void>();

  readonly tiles = TILES;

  step = signal<WorkflowStep>('tiles');
  pendingState = signal<EntryState | null>(null);
  showCapture = signal(false);
  showOverrideReason = signal(false);
  captureMode = signal<'camera' | 'upload'>('camera');
  loading = signal(false);

  subjectName = signal('');
  subjectDocument = signal('');
  selectedCategory = signal<SubjectType>('visitor');
  residentLookupQuery = signal('');
  residentSearchResults = signal<ResidentResponse[]>([]);
  residentSearchError = signal<string | null>(null);
  residentSearchLoading = signal(false);
  selectedResident = signal<ResidentResponse | null>(null);
  photoId = signal<string | null>(null);
  pendingOverrideReason = signal<OverrideReason | null>(null);

  photoEntityType = computed<'resident' | 'visitor' | 'vehicle' | 'service-provider'>(() => {
    const cat = this.selectedCategory();
    if (cat === 'dweller') return 'resident';
    if (cat === 'service_provider') return 'service-provider';
    if (cat === 'vehicle') return 'vehicle';
    return 'visitor';
  });

  subjectNamePlaceholder = computed(() => {
    switch (this.selectedCategory()) {
      case 'dweller':
        return 'Resident name';
      case 'service_provider':
        return 'Company or provider name';
      case 'vehicle':
        return 'License plate or driver name';
      default:
        return 'Visitor name';
    }
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

  isCategoryAllowed(category: SubjectType): boolean {
    const state = this.pendingState();
    if (state === 'gatehouse_only') {
      return category === 'service_provider';
    }
    if (state === 'entered_override') {
      return category === 'visitor' || category === 'dweller';
    }
    if (state === 'exited') {
      return category === 'dweller' || category === 'vehicle';
    }
    return true;
  }

  onCategorySelect(category: SubjectType): void {
    if (!this.isCategoryAllowed(category)) return;
    if (this.selectedCategory() !== category) {
      this.clearSubject();
    }
    this.selectedCategory.set(category);
  }

  onResidentLookupInput(value: string): void {
    this.residentLookupQuery.set(value);
    this.residentSearchResults.set([]);
    this.residentSearchError.set(null);
    this.selectedResident.set(null);
    this.subjectName.set('');
    this.subjectDocument.set('');
  }

  async findResident(): Promise<void> {
    const rawIdentifier = this.residentLookupQuery().trim();
    if (!rawIdentifier) {
      this.residentSearchError.set('Enter a CPF or resident ID to search.');
      return;
    }

    const isResidentId = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(
      rawIdentifier,
    );
    const normalizedIdentifier = isResidentId ? rawIdentifier : rawIdentifier.replace(/\D/g, '');
    const searchValue = normalizedIdentifier || rawIdentifier;

    this.residentSearchLoading.set(true);
    this.residentSearchError.set(null);
    this.residentSearchResults.set([]);
    this.selectedResident.set(null);

    try {
      const residents = isResidentId
        ? [await firstValueFrom(this.residentsApi.get(rawIdentifier))]
        : await firstValueFrom(this.residentsApi.list(searchValue, 0, 10));
      const activeResidents = residents.filter((resident) => resident.active);

      if (!activeResidents.length) {
        this.residentSearchError.set('No active resident was found for this CPF or ID.');
      } else if (activeResidents.length === 1) {
        const resident = activeResidents[0];
        if (resident) this.selectResident(resident);
      } else {
        this.residentSearchResults.set(activeResidents);
      }
    } catch {
      this.residentSearchError.set('No active resident was found for this CPF or ID.');
    } finally {
      this.residentSearchLoading.set(false);
    }
  }

  selectResident(resident: ResidentResponse): void {
    this.selectedResident.set(resident);
    this.residentLookupQuery.set(resident.cpf);
    this.residentSearchResults.set([]);
    this.residentSearchError.set(null);
    this.subjectName.set(resident.name);
    this.subjectDocument.set(resident.cpf);
  }

  openCapture(): void {
    this.captureMode.set('camera');
    this.showCapture.set(true);
  }

  async onTileTap(action: TileAction): Promise<void> {
    if (action === 'register') {
      this.pendingState.set('entered_with_consent');
      this.selectedCategory.set('visitor');
      const cat = toSubjectCategory(this.selectedCategory());
      try {
        const policy = await firstValueFrom(this.consentPolicyService.getByCategory(cat));
        if (policy?.photoRequired) {
          this.captureMode.set('camera');
          this.showCapture.set(true);
        } else {
          this.step.set('subject-info');
        }
      } catch {
        this.step.set('subject-info');
      }
    } else if (action === 'exit') {
      this.pendingState.set('exited');
      this.selectedCategory.set('dweller');
      this.showCapture.set(false);
      this.step.set('subject-info');
    } else if (action === 'denied') {
      this.pendingState.set('entered_without_consent');
      this.selectedCategory.set('visitor');
      const cat = toSubjectCategory(this.selectedCategory());
      try {
        const policy = await firstValueFrom(this.consentPolicyService.getByCategory(cat));
        if (policy?.photoRequired) {
          this.captureMode.set('camera');
          this.showCapture.set(true);
        } else {
          this.step.set('subject-info');
        }
      } catch {
        this.step.set('subject-info');
      }
    } else if (action === 'gatehouse') {
      this.pendingState.set('gatehouse_only');
      this.selectedCategory.set('service_provider');
      this.showCapture.set(false);
      this.step.set('subject-info');
    } else if (action === 'override') {
      this.pendingState.set('entered_override');
      this.showCapture.set(false);
      this.showOverrideReason.set(true);
    }
  }

  back(): void {
    this.step.set('tiles');
    this.clearSubject();
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
    this.showCapture.set(false);
    if (!this.photoId() && this.pendingState() === 'entered_with_consent') {
      this.reset();
    }
  }

  onCaptureModeChange(mode: 'camera' | 'upload'): void {
    this.captureMode.set(mode);
  }

  onOverrideReason(reason: OverrideReason): void {
    this.pendingOverrideReason.set(reason);
    this.showOverrideReason.set(false);
    this.step.set('subject-info');
  }

  onOverrideClosed(): void {
    this.showOverrideReason.set(false);
    if (!this.pendingOverrideReason()) {
      this.reset();
    }
  }

  async continue(): Promise<void> {
    const state = this.pendingState();
    if (!state) return;
    if (this.selectedCategory() === 'dweller' && !this.selectedResident()) {
      this.toast.error('Search and select the registered resident before logging access.');
      return;
    }
    this.loading.set(true);
    try {
      const request: CreateEntryLogRequest = {
        entryState: state,
        subjectType: this.selectedCategory(),
        subjectName: this.subjectName() || undefined,
        subjectDocument: this.subjectDocument() || undefined,
        photoId: this.photoId() || undefined,
        overrideReason: this.pendingOverrideReason() || undefined,
        apartmentId: this.selectedResident()?.apartmentId ?? undefined,
        residentId: this.selectedResident()?.id ?? undefined,
      };
      const entry = await firstValueFrom(this.entryLogService.create(request));
      this.toast.success(state === 'exited' ? 'Exit logged' : 'Entry logged');
      this.entryLogged.emit(entry);
      if (this.closeOnEntry()) {
        this.close();
      } else {
        this.reset();
      }
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
    this.captureMode.set('camera');
    this.subjectName.set('');
    this.subjectDocument.set('');
    this.residentLookupQuery.set('');
    this.residentSearchResults.set([]);
    this.residentSearchError.set(null);
    this.residentSearchLoading.set(false);
    this.selectedResident.set(null);
    this.photoId.set(null);
    this.pendingOverrideReason.set(null);
    this.loading.set(false);
    this.selectedCategory.set(this.defaultCategory());
  }

  private clearSubject(): void {
    this.subjectName.set('');
    this.subjectDocument.set('');
    this.residentLookupQuery.set('');
    this.residentSearchResults.set([]);
    this.residentSearchError.set(null);
    this.selectedResident.set(null);
  }
}

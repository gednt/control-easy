import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import {
  ApartmentsApiService,
  ApartmentResponse,
  formatApartmentLabel,
} from './apartments-api.service';
import { getApiErrorMessage } from '../../core/utils/api-error.util';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'ce-apartments-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-header">
      <div class="page-title-block">
        <h1 class="page-title">Apartments</h1>
        <p class="page-subtitle">{{ apartments().length }} apartments registered in this condominium.</p>
      </div>
      <div class="page-header-actions">
        <button class="ce-button variant-secondary size-md" (click)="refresh()">&#8635; Refresh</button>
        @if (canWrite()) {
          <button class="ce-button variant-primary size-md" (click)="openCreateModal()">&#43; Add apartment</button>
        }
      </div>
    </div>

    @if (pageError()) {
      <div class="page-error">{{ pageError() }}</div>
    }

    @if (loading()) {
      <div class="loading-state">
        <div class="ce-spinner tone-primary size-lg"></div>
        <p>Loading apartments...</p>
      </div>
    } @else if (apartments().length === 0) {
      <div class="ce-empty-state">
        <div class="ce-empty-state-icon">&#127968;</div>
        <div class="ce-empty-state-title">No apartments yet</div>
        <div class="ce-empty-state-description text-secondary">Add your first apartment to get started.</div>
        @if (canWrite()) {
          <button class="ce-button variant-primary size-md" (click)="openCreateModal()">&#43; Add apartment</button>
        }
      </div>
    } @else {
      <div class="ce-card" style="padding: 0;">
        <div class="ce-table-wrapper">
          <table class="ce-table" aria-label="Apartments table">
            <thead>
              <tr>
                <th scope="col">Block</th>
                <th scope="col">Unit</th>
                <th scope="col">Label</th>
                <th scope="col">Status</th>
                <th scope="col" style="width: 1%;"></th>
              </tr>
            </thead>
            <tbody>
              @for (apartment of apartments(); track apartment.id) {
                <tr>
                  <td>{{ apartment.block }}</td>
                  <td>{{ apartment.unit }}</td>
                  <td>{{ formatLabel(apartment) }}</td>
                  <td>
                    <span class="ce-badge tone-success size-sm" [class.tone-neutral]="!apartment.active">
                      {{ apartment.active ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td>
                    @if (canWrite()) {
                      <div class="action-cell">
                        <button class="icon-btn-sm" aria-label="Edit apartment" title="Edit" (click)="openEditModal(apartment)">&#9998;</button>
                        @if (apartment.active) {
                          <button class="icon-btn-sm action-deactivate" aria-label="Deactivate apartment" title="Deactivate" (click)="onDeactivate(apartment)">&#8855;</button>
                        }
                      </div>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    }

    @if (createModalOpen()) {
      <div class="ce-modal-backdrop" (click)="closeCreateModal()">
        <div class="ce-modal size-md" role="dialog" aria-modal="true" (click)="$event.stopPropagation()">
          <div class="ce-modal-header">
            <h3 class="ce-modal-title">Add apartment</h3>
            <button class="ce-modal-close" (click)="closeCreateModal()" aria-label="Close">&#10005;</button>
          </div>
          <div class="ce-modal-body">
            @if (createError()) {
              <div class="form-error-banner">{{ createError() }}</div>
            }
            <form class="ce-form" [formGroup]="createForm" (ngSubmit)="onCreate()">
              <div class="form-row">
                <div class="ce-input-group" style="flex: 1;">
                  <label class="ce-input-label" for="ap-block">Block</label>
                  <div class="ce-input-wrapper" [class.has-error]="createForm.get('block')?.invalid && createForm.get('block')?.touched">
                    <input id="ap-block" class="ce-input" placeholder="e.g. A" formControlName="block" />
                  </div>
                  @if (createForm.get('block')?.invalid && createForm.get('block')?.touched) {
                    <div class="ce-input-error">Block is required</div>
                  }
                </div>
                <div class="ce-input-group" style="flex: 1;">
                  <label class="ce-input-label" for="ap-unit">Unit</label>
                  <div class="ce-input-wrapper" [class.has-error]="createForm.get('unit')?.invalid && createForm.get('unit')?.touched">
                    <input id="ap-unit" class="ce-input" placeholder="e.g. 101" formControlName="unit" />
                  </div>
                  @if (createForm.get('unit')?.invalid && createForm.get('unit')?.touched) {
                    <div class="ce-input-error">Unit is required</div>
                  }
                </div>
              </div>
            </form>
          </div>
          <div class="ce-modal-footer">
            <button class="ce-button variant-ghost size-sm" (click)="closeCreateModal()">Cancel</button>
            <button class="ce-button variant-primary size-sm"
                    (click)="onCreate()"
                    [disabled]="createForm.invalid || creating()">
              @if (creating()) { <span class="ce-spinner tone-current size-sm"></span> }
              Add apartment
            </button>
          </div>
        </div>
      </div>
    }

    @if (editModalOpen()) {
      <div class="ce-modal-backdrop" (click)="closeEditModal()">
        <div class="ce-modal size-md" role="dialog" aria-modal="true" (click)="$event.stopPropagation()">
          <div class="ce-modal-header">
            <h3 class="ce-modal-title">Edit apartment</h3>
            <button class="ce-modal-close" (click)="closeEditModal()" aria-label="Close">&#10005;</button>
          </div>
          <div class="ce-modal-body">
            @if (editError()) {
              <div class="form-error-banner">{{ editError() }}</div>
            }
            <form class="ce-form" [formGroup]="editForm" (ngSubmit)="onEdit()">
              <div class="form-row">
                <div class="ce-input-group" style="flex: 1;">
                  <label class="ce-input-label" for="ep-block">Block</label>
                  <div class="ce-input-wrapper" [class.has-error]="editForm.get('block')?.invalid && editForm.get('block')?.touched">
                    <input id="ep-block" class="ce-input" formControlName="block" />
                  </div>
                </div>
                <div class="ce-input-group" style="flex: 1;">
                  <label class="ce-input-label" for="ep-unit">Unit</label>
                  <div class="ce-input-wrapper" [class.has-error]="editForm.get('unit')?.invalid && editForm.get('unit')?.touched">
                    <input id="ep-unit" class="ce-input" formControlName="unit" />
                  </div>
                </div>
              </div>
            </form>
          </div>
          <div class="ce-modal-footer">
            <button class="ce-button variant-ghost size-sm" (click)="closeEditModal()">Cancel</button>
            <button class="ce-button variant-primary size-sm"
                    (click)="onEdit()"
                    [disabled]="editForm.invalid || saving()">
              @if (saving()) { <span class="ce-spinner tone-current size-sm"></span> }
              Save changes
            </button>
          </div>
        </div>
      </div>
    }

    @if (confirmDeactivateOpen()) {
      <div class="ce-modal-backdrop" (click)="closeDeactivateConfirm()">
        <div class="ce-modal size-sm" role="dialog" aria-modal="true" (click)="$event.stopPropagation()">
          <div class="ce-modal-header">
            <h3 class="ce-modal-title">Deactivate apartment</h3>
            <button class="ce-modal-close" (click)="closeDeactivateConfirm()" aria-label="Close">&#10005;</button>
          </div>
          <div class="ce-modal-body">
            @if (deactivateError()) {
              <div class="form-error-banner">{{ deactivateError() }}</div>
            }
            <p class="text-secondary">
              Deactivate <strong>{{ apartmentToDeactivate() ? formatLabel(apartmentToDeactivate()!) : '' }}</strong>?
              Existing resident and visit records will keep their references.
            </p>
          </div>
          <div class="ce-modal-footer">
            <button class="ce-button variant-ghost size-sm" (click)="closeDeactivateConfirm()">Cancel</button>
            <button class="ce-button variant-danger size-sm"
                    (click)="onConfirmDeactivate()"
                    [disabled]="deactivating()">
              @if (deactivating()) { <span class="ce-spinner tone-current size-sm"></span> }
              Deactivate
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .page-header { display: flex; align-items: center; justify-content: space-between; gap: var(--space-4); flex-wrap: wrap; margin-bottom: var(--space-6); }
    .page-title-block { min-width: 0; }
    .page-title { font-size: var(--font-size-2xl); margin-bottom: var(--space-1); }
    .page-subtitle { color: var(--color-text-secondary); font-size: var(--font-size-sm); }
    .page-header-actions { display: flex; gap: var(--space-2); flex-wrap: wrap; }
    .page-error {
      margin-bottom: var(--space-4);
      padding: var(--space-3) var(--space-4);
      border-radius: var(--radius-lg);
      background: var(--color-danger-light);
      color: var(--color-danger);
      font-size: var(--font-size-sm);
    }
    .loading-state {
      display: flex; flex-direction: column; align-items: center; justify-content: center;
      padding: var(--space-12); gap: var(--space-4); color: var(--color-text-secondary);
    }
    .ce-button {
      display: inline-flex; align-items: center; justify-content: center; gap: var(--space-2);
      font-weight: var(--font-weight-medium); border: 1px solid transparent; border-radius: var(--radius-lg);
      cursor: pointer; font-family: inherit;
    }
    .ce-button.size-sm { height: 2rem; padding: 0 var(--space-3); font-size: var(--font-size-sm); }
    .ce-button.size-md { height: 2.5rem; padding: 0 var(--space-4); font-size: var(--font-size-sm); }
    .ce-button.variant-primary { background: var(--color-primary); color: var(--color-text-on-primary); }
    .ce-button.variant-secondary { background: var(--color-surface); color: var(--color-text-primary); border-color: var(--color-border); }
    .ce-button.variant-ghost { background: transparent; color: var(--color-text-primary); }
    .ce-button.variant-danger { background: var(--color-danger); color: var(--color-text-on-primary); }
    .ce-button:disabled { opacity: 0.6; cursor: not-allowed; }
    .ce-card { background: var(--color-surface-elevated); border: 1px solid var(--color-border); border-radius: var(--radius-xl); box-shadow: var(--shadow-card); overflow: hidden; }
    .ce-table-wrapper { overflow-x: auto; }
    .ce-table { width: 100%; border-collapse: collapse; font-size: var(--font-size-sm); }
    .ce-table thead { background: var(--color-neutral-light); }
    .ce-table th, .ce-table td { padding: var(--space-3) var(--space-4); border-bottom: 1px solid var(--color-border); text-align: left; }
    .ce-table th { font-weight: var(--font-weight-semibold); color: var(--color-text-secondary); font-size: var(--font-size-xs); text-transform: uppercase; }
    .ce-badge { display: inline-flex; align-items: center; font-weight: var(--font-weight-medium); border-radius: var(--radius-full); border: 1px solid transparent; font-size: 0.7rem; padding: var(--space-1) var(--space-2); }
    .ce-badge.tone-success { background: var(--color-success-light); color: var(--color-success); }
    .ce-badge.tone-neutral { background: var(--color-neutral-light); color: var(--color-text-secondary); }
    .action-cell { display: flex; gap: var(--space-1); }
    .icon-btn-sm {
      width: 2rem; height: 2rem; display: inline-flex; align-items: center; justify-content: center;
      background: transparent; border: 0; border-radius: var(--radius-md); cursor: pointer; color: var(--color-text-muted);
    }
    .icon-btn-sm:hover { background: var(--color-neutral-light); color: var(--color-text-primary); }
    .action-deactivate:hover { background: var(--color-danger-light); color: var(--color-danger); }
    .ce-empty-state { display: flex; flex-direction: column; align-items: center; text-align: center; padding: var(--space-12); gap: var(--space-3); }
    .ce-empty-state-icon { font-size: 1.5rem; width: 4rem; height: 4rem; display: inline-flex; align-items: center; justify-content: center; background: var(--color-neutral-light); border-radius: var(--radius-full); }
    .ce-empty-state-title { font-size: var(--font-size-lg); font-weight: var(--font-weight-semibold); }
    .ce-spinner { display: inline-block; border-radius: var(--radius-full); border: 2px solid currentColor; border-top-color: transparent; animation: spin 1.4s linear infinite; }
    .ce-spinner.size-sm { width: 1rem; height: 1rem; }
    .ce-spinner.size-lg { width: 2rem; height: 2rem; border-width: 3px; }
    .ce-spinner.tone-primary { color: var(--color-primary); }
    .ce-spinner.tone-current { color: currentColor; }
    .ce-modal-backdrop { position: fixed; inset: 0; background: rgb(0 0 0 / 0.5); display: flex; align-items: center; justify-content: center; z-index: 100; padding: var(--space-4); }
    .ce-modal { background: var(--color-surface-elevated); border-radius: var(--radius-xl); box-shadow: var(--shadow-xl); width: 100%; max-width: 32rem; }
    .ce-modal.size-sm { max-width: 24rem; }
    .ce-modal-header { display: flex; align-items: center; justify-content: space-between; padding: var(--space-5) var(--space-6); border-bottom: 1px solid var(--color-border); }
    .ce-modal-title { font-size: var(--font-size-lg); font-weight: var(--font-weight-semibold); }
    .ce-modal-close { background: transparent; border: 0; cursor: pointer; font-size: 1.25rem; color: var(--color-text-muted); }
    .ce-modal-body { padding: var(--space-6); }
    .ce-modal-footer { display: flex; justify-content: flex-end; gap: var(--space-2); padding: var(--space-4) var(--space-6); border-top: 1px solid var(--color-border); }
    .ce-form { display: flex; flex-direction: column; gap: var(--space-4); }
    .ce-input-group { display: flex; flex-direction: column; gap: var(--space-1); }
    .ce-input-label { font-size: var(--font-size-sm); font-weight: var(--font-weight-medium); }
    .ce-input-wrapper { display: flex; align-items: center; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-lg); overflow: hidden; }
    .ce-input-wrapper.has-error { border-color: var(--color-danger); }
    .ce-input { flex: 1; border: 0; background: transparent; padding: var(--space-3); font-family: inherit; font-size: var(--font-size-sm); outline: none; min-height: 2.5rem; width: 100%; }
    .ce-input-error { font-size: var(--font-size-xs); color: var(--color-danger); }
    .form-row { display: flex; gap: var(--space-3); }
    .form-error-banner {
      margin-bottom: var(--space-4); padding: var(--space-3); border-radius: var(--radius-lg);
      background: var(--color-danger-light); color: var(--color-danger); font-size: var(--font-size-sm);
    }
    .text-secondary { color: var(--color-text-secondary); }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApartmentsPage {
  private readonly api = inject(ApartmentsApiService);
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  readonly canWrite = computed(() => this.auth.hasPermission('Apartments.Write'));

  apartments = signal<ApartmentResponse[]>([]);
  loading = signal(true);
  creating = signal(false);
  saving = signal(false);
  deactivating = signal(false);
  createModalOpen = signal(false);
  editModalOpen = signal(false);
  confirmDeactivateOpen = signal(false);
  pageError = signal<string | null>(null);
  createError = signal<string | null>(null);
  editError = signal<string | null>(null);
  deactivateError = signal<string | null>(null);
  editingApartment = signal<ApartmentResponse | null>(null);
  apartmentToDeactivate = signal<ApartmentResponse | null>(null);

  readonly formatLabel = formatApartmentLabel;

  createForm = this.fb.group({
    block: ['', Validators.required],
    unit: ['', Validators.required],
  });

  editForm = this.fb.group({
    block: ['', Validators.required],
    unit: ['', Validators.required],
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.pageError.set(null);
    this.api.list(undefined, 0, 500).subscribe({
      next: (data) => {
        this.apartments.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.pageError.set(getApiErrorMessage(err, 'Failed to load apartments'));
        this.loading.set(false);
      },
    });
  }

  refresh(): void {
    this.load();
  }

  openCreateModal(): void {
    this.createForm.reset();
    this.createError.set(null);
    this.createModalOpen.set(true);
  }

  closeCreateModal(): void {
    this.createModalOpen.set(false);
  }

  onCreate(): void {
    if (this.createForm.invalid) return;
    this.creating.set(true);
    this.createError.set(null);
    const v = this.createForm.value;
    this.api.create({ block: v.block!, unit: v.unit! }).subscribe({
      next: () => {
        this.creating.set(false);
        this.closeCreateModal();
        this.load();
      },
      error: (err) => {
        this.creating.set(false);
        this.createError.set(getApiErrorMessage(err, 'Failed to create apartment'));
      },
    });
  }

  openEditModal(apartment: ApartmentResponse): void {
    this.editingApartment.set(apartment);
    this.editForm.patchValue({ block: apartment.block, unit: apartment.unit });
    this.editError.set(null);
    this.editModalOpen.set(true);
  }

  closeEditModal(): void {
    this.editModalOpen.set(false);
    this.editingApartment.set(null);
  }

  onEdit(): void {
    if (this.editForm.invalid) return;
    const apartment = this.editingApartment();
    if (!apartment) return;
    this.saving.set(true);
    this.editError.set(null);
    const v = this.editForm.value;
    this.api.update(apartment.id, {
      block: v.block!,
      unit: v.unit!,
      active: apartment.active,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeEditModal();
        this.load();
      },
      error: (err) => {
        this.saving.set(false);
        this.editError.set(getApiErrorMessage(err, 'Failed to update apartment'));
      },
    });
  }

  onDeactivate(apartment: ApartmentResponse): void {
    this.apartmentToDeactivate.set(apartment);
    this.deactivateError.set(null);
    this.confirmDeactivateOpen.set(true);
  }

  closeDeactivateConfirm(): void {
    this.confirmDeactivateOpen.set(false);
    this.apartmentToDeactivate.set(null);
  }

  onConfirmDeactivate(): void {
    const apartment = this.apartmentToDeactivate();
    if (!apartment) return;
    this.deactivating.set(true);
    this.deactivateError.set(null);
    this.api.deactivate(apartment.id, apartment.block, apartment.unit).subscribe({
      next: () => {
        this.deactivating.set(false);
        this.closeDeactivateConfirm();
        this.load();
      },
      error: (err) => {
        this.deactivating.set(false);
        this.deactivateError.set(getApiErrorMessage(err, 'Failed to deactivate apartment'));
      },
    });
  }
}

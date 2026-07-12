import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { VisitsApiService, VisitResponse } from './visits-api.service';
import { ApartmentPickerComponent } from '../../shared/apartment-picker/apartment-picker.component';
import {
  ApartmentsApiService,
  formatApartmentLabel,
} from '../apartments/apartments-api.service';
import { getApiErrorMessage } from '../../core/utils/api-error.util';

type StatusFilter = 'all' | 'Pending' | 'CheckedIn';

@Component({
  selector: 'ce-visits-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ApartmentPickerComponent],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Visits</h1>
        <p class="page-subtitle">{{ visits().length }} visit records</p>
      </div>
      <button class="ce-button variant-primary size-md" (click)="openCreate()">+ Add visit</button>
    </div>

    <div class="filter-tabs">
      <button class="filter-tab" [class.active]="statusFilter() === 'all'" (click)="setStatusFilter('all')">All</button>
      <button class="filter-tab" [class.active]="statusFilter() === 'Pending'" (click)="setStatusFilter('Pending')">Pending</button>
      <button class="filter-tab" [class.active]="statusFilter() === 'CheckedIn'" (click)="setStatusFilter('CheckedIn')">On-site</button>
    </div>

    @if (pageError()) {
      <div class="page-error">{{ pageError() }}</div>
    }

    @if (actionError()) {
      <div class="page-error">{{ actionError() }}</div>
    }

    @if (loading()) {
      <p class="text-secondary">Loading visits...</p>
    } @else {
      <div class="ce-card">
        <table class="ce-table">
          <thead>
            <tr>
              <th>Visitor</th>
              <th>Document</th>
              <th>Apartment</th>
              <th>Status</th>
              <th>Checked in</th>
              <th>Checked out</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            @for (visit of visits(); track visit.id) {
              <tr>
                <td>{{ visit.visitorName }}</td>
                <td>{{ visit.visitorDocument }}</td>
                <td>{{ getApartmentLabel(visit.apartmentId) }}</td>
                <td>
                  <span class="ce-badge" [class]="getStatusBadgeClass(visit.status)">
                    {{ getStatusLabel(visit.status) }}
                  </span>
                </td>
                <td>{{ visit.checkedInAtUtc ? (visit.checkedInAtUtc | date:'short') : '—' }}</td>
                <td>{{ visit.checkedOutAtUtc ? (visit.checkedOutAtUtc | date:'short') : '—' }}</td>
                <td>
                  @if (visit.status === 'Pending') {
                    <button
                      class="ce-button variant-primary size-sm"
                      [disabled]="actionInFlight() === visit.id"
                      (click)="onCheckIn(visit)">
                      @if (actionInFlight() === visit.id) { <span class="spinner"></span> }
                      Check in
                    </button>
                  } @else if (visit.status === 'CheckedIn') {
                    <button
                      class="ce-button variant-secondary size-sm"
                      [disabled]="actionInFlight() === visit.id"
                      (click)="onCheckOut(visit)">
                      @if (actionInFlight() === visit.id) { <span class="spinner"></span> }
                      Check out
                    </button>
                  } @else {
                    <span class="text-secondary text-xs">—</span>
                  }
                </td>
              </tr>
            } @empty {
              <tr><td colspan="7" class="text-secondary">No visits yet.</td></tr>
            }
          </tbody>
        </table>
      </div>
    }

    @if (createOpen()) {
      <div class="ce-modal-backdrop" (click)="closeCreate()">
        <div class="ce-modal" (click)="$event.stopPropagation()">
          <h3>Add visit</h3>
          @if (createError()) {
            <div class="form-error-banner">{{ createError() }}</div>
          }
          <form [formGroup]="form" (ngSubmit)="onCreate()">
            <label>Visitor name<input class="ce-input" formControlName="visitorName" /></label>
            <label>Document<input class="ce-input" formControlName="visitorDocument" /></label>
            <label>Phone<input class="ce-input" formControlName="visitorPhone" /></label>
            <ce-apartment-picker
              formControlName="apartmentId"
              label="Apartment"
              inputId="vs-apartment"
              placeholder="Select apartment..." />
            <label>Purpose<input class="ce-input" formControlName="purpose" /></label>
            <div class="actions">
              <button type="button" class="ce-button variant-ghost" (click)="closeCreate()">Cancel</button>
              <button type="submit" class="ce-button variant-primary" [disabled]="form.invalid || creating()">
                @if (creating()) { <span class="spinner"></span> }
                Create
              </button>
            </div>
          </form>
        </div>
      </div>
    }
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--space-4); }
    .page-title { font-size: var(--font-size-2xl); margin: 0; }
    .page-subtitle { color: var(--color-text-secondary); font-size: var(--font-size-sm); }
    .filter-tabs { display: flex; gap: var(--space-2); margin-bottom: var(--space-4); }
    .filter-tab {
      border: 1px solid var(--color-border); background: var(--color-surface); border-radius: var(--radius-lg);
      padding: var(--space-2) var(--space-4); cursor: pointer; font-family: inherit; font-size: var(--font-size-sm);
    }
    .filter-tab.active { background: var(--color-primary); color: var(--color-text-on-primary); border-color: var(--color-primary); }
    .page-error {
      margin-bottom: var(--space-4); padding: var(--space-3); border-radius: var(--radius-lg);
      background: var(--color-danger-light); color: var(--color-danger); font-size: var(--font-size-sm);
    }
    .ce-card { background: var(--color-surface-elevated); border: 1px solid var(--color-border); border-radius: var(--radius-xl); overflow: hidden; }
    .ce-table { width: 100%; border-collapse: collapse; font-size: var(--font-size-sm); }
    .ce-table th, .ce-table td { padding: var(--space-3) var(--space-4); border-bottom: 1px solid var(--color-border); text-align: left; vertical-align: middle; }
    .ce-table thead { background: var(--color-neutral-light); }
    .ce-badge {
      padding: var(--space-1) var(--space-2); border-radius: var(--radius-full);
      font-size: var(--font-size-xs); font-weight: var(--font-weight-medium); border: 1px solid transparent;
    }
    .badge-pending { background: var(--color-neutral-light); color: var(--color-text-secondary); }
    .badge-onsite { background: var(--color-success-light); color: var(--color-success); }
    .badge-done { background: var(--color-neutral-light); color: var(--color-text-muted); }
    .badge-cancelled { background: var(--color-danger-light); color: var(--color-danger); }
    .ce-button {
      border: 0; border-radius: var(--radius-lg); padding: 0 var(--space-3); cursor: pointer;
      font-family: inherit; display: inline-flex; align-items: center; gap: var(--space-2);
    }
    .ce-button.size-sm { height: 2rem; font-size: var(--font-size-sm); }
    .ce-button.size-md { height: 2.5rem; padding: 0 var(--space-4); }
    .variant-primary { background: var(--color-primary); color: var(--color-text-on-primary); }
    .variant-secondary { background: var(--color-surface); color: var(--color-text-primary); border: 1px solid var(--color-border); }
    .variant-ghost { background: transparent; }
    .ce-button:disabled { opacity: 0.6; cursor: not-allowed; }
    .ce-modal-backdrop { position: fixed; inset: 0; background: rgb(0 0 0 / 0.5); display: flex; align-items: center; justify-content: center; z-index: 100; }
    .ce-modal { background: var(--color-surface-elevated); padding: var(--space-6); border-radius: var(--radius-xl); width: min(28rem, 90vw); display: flex; flex-direction: column; gap: var(--space-3); }
    .ce-input { width: 100%; padding: var(--space-2) var(--space-3); border: 1px solid var(--color-border); border-radius: var(--radius-lg); margin-top: var(--space-1); font-family: inherit; }
    label { display: block; font-size: var(--font-size-sm); }
    .actions { display: flex; justify-content: flex-end; gap: var(--space-2); margin-top: var(--space-2); }
    .form-error-banner {
      padding: var(--space-3); border-radius: var(--radius-lg);
      background: var(--color-danger-light); color: var(--color-danger); font-size: var(--font-size-sm);
    }
    .spinner {
      width: 1rem; height: 1rem; border: 2px solid currentColor; border-top-color: transparent;
      border-radius: 50%; animation: spin 1s linear infinite;
    }
    .text-secondary { color: var(--color-text-secondary); }
    .text-xs { font-size: var(--font-size-xs); }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VisitsPage {
  private readonly api = inject(VisitsApiService);
  private readonly apartmentsApi = inject(ApartmentsApiService);
  private readonly fb = inject(FormBuilder);

  visits = signal<VisitResponse[]>([]);
  apartmentLabels = signal<Record<string, string>>({});
  loading = signal(true);
  creating = signal(false);
  createOpen = signal(false);
  statusFilter = signal<StatusFilter>('all');
  pageError = signal<string | null>(null);
  createError = signal<string | null>(null);
  actionError = signal<string | null>(null);
  actionInFlight = signal<string | null>(null);

  form = this.fb.group({
    visitorName: ['', Validators.required],
    visitorDocument: ['', Validators.required],
    visitorPhone: [''],
    apartmentId: [null],
    purpose: [''],
  });

  constructor() {
    this.loadApartmentLabels();
    this.load();
  }

  loadApartmentLabels(): void {
    this.apartmentsApi.list(undefined, 0, 500).subscribe({
      next: (apartments) => {
        const labels: Record<string, string> = {};
        for (const apartment of apartments) {
          labels[apartment.id] = formatApartmentLabel(apartment);
        }
        this.apartmentLabels.set(labels);
      },
    });
  }

  getApartmentLabel(apartmentId: string | null): string {
    if (!apartmentId) return '—';
    return this.apartmentLabels()[apartmentId] ?? apartmentId.slice(0, 8);
  }

  setStatusFilter(filter: StatusFilter): void {
    this.statusFilter.set(filter);
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.pageError.set(null);
    const filter = this.statusFilter();
    const status = filter === 'all' ? undefined : filter;
    this.api.list(status).subscribe({
      next: (data) => { this.visits.set(data); this.loading.set(false); },
      error: (err) => {
        this.pageError.set(getApiErrorMessage(err, 'Failed to load visits'));
        this.loading.set(false);
      },
    });
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Pending': return 'Pending';
      case 'CheckedIn': return 'On-site';
      case 'CheckedOut': return 'Checked out';
      case 'Cancelled': return 'Cancelled';
      default: return status;
    }
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Pending': return 'badge-pending';
      case 'CheckedIn': return 'badge-onsite';
      case 'CheckedOut': return 'badge-done';
      case 'Cancelled': return 'badge-cancelled';
      default: return 'badge-pending';
    }
  }

  openCreate(): void {
    this.form.reset();
    this.createError.set(null);
    this.createOpen.set(true);
  }

  closeCreate(): void { this.createOpen.set(false); }

  onCreate(): void {
    if (this.form.invalid) return;
    this.creating.set(true);
    this.createError.set(null);
    const v = this.form.value;
    this.api.create({
      visitorName: v.visitorName!,
      visitorDocument: v.visitorDocument!,
      visitorPhone: v.visitorPhone || null,
      apartmentId: v.apartmentId || null,
      purpose: v.purpose || null,
    }).subscribe({
      next: () => { this.creating.set(false); this.closeCreate(); this.load(); },
      error: (err) => {
        this.creating.set(false);
        this.createError.set(getApiErrorMessage(err, 'Failed to create visit'));
      },
    });
  }

  onCheckIn(visit: VisitResponse): void {
    this.actionInFlight.set(visit.id);
    this.actionError.set(null);
    this.api.checkIn(visit.id).subscribe({
      next: () => {
        this.actionInFlight.set(null);
        this.load();
      },
      error: (err) => {
        this.actionInFlight.set(null);
        this.actionError.set(getApiErrorMessage(err, 'Failed to check in visitor'));
      },
    });
  }

  onCheckOut(visit: VisitResponse): void {
    this.actionInFlight.set(visit.id);
    this.actionError.set(null);
    this.api.checkOut(visit.id).subscribe({
      next: () => {
        this.actionInFlight.set(null);
        this.load();
      },
      error: (err) => {
        this.actionInFlight.set(null);
        this.actionError.set(getApiErrorMessage(err, 'Failed to check out visitor'));
      },
    });
  }
}

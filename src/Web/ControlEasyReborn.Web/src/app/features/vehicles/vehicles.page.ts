import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { VehiclesApiService, VehicleResponse } from './vehicles-api.service';
import { ApartmentPickerComponent } from '../../shared/apartment-picker/apartment-picker.component';
import {
  ApartmentsApiService,
  formatApartmentLabel,
} from '../apartments/apartments-api.service';
import { getApiErrorMessage } from '../../core/utils/api-error.util';

@Component({
  selector: 'ce-vehicles-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ApartmentPickerComponent],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Vehicles</h1>
        <p class="page-subtitle">{{ vehicles().length }} registered vehicles</p>
      </div>
      <button class="ce-button variant-primary size-md" (click)="openCreate()">+ Add vehicle</button>
    </div>

    @if (pageError()) {
      <div class="page-error">{{ pageError() }}</div>
    }

    @if (loading()) {
      <p class="text-secondary">Loading vehicles...</p>
    } @else {
      <div class="ce-card">
        <table class="ce-table">
          <thead>
            <tr>
              <th>Plate</th>
              <th>Owner</th>
              <th>Apartment</th>
              <th>Brand / Model</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            @for (vehicle of vehicles(); track vehicle.id) {
              <tr>
                <td>{{ vehicle.plate }}</td>
                <td>{{ vehicle.ownerName ?? '—' }}</td>
                <td>{{ getApartmentLabel(vehicle.apartmentId) }}</td>
                <td>{{ vehicle.brand ?? '—' }} {{ vehicle.model ?? '' }}</td>
                <td><span class="ce-badge">{{ vehicle.active ? 'Active' : 'Inactive' }}</span></td>
              </tr>
            } @empty {
              <tr><td colspan="5" class="text-secondary">No vehicles yet.</td></tr>
            }
          </tbody>
        </table>
      </div>
    }

    @if (createOpen()) {
      <div class="ce-modal-backdrop" (click)="closeCreate()">
        <div class="ce-modal" (click)="$event.stopPropagation()">
          <h3>Add vehicle</h3>
          @if (createError()) {
            <div class="form-error-banner">{{ createError() }}</div>
          }
          <form [formGroup]="form" (ngSubmit)="onCreate()">
            <label>Plate<input class="ce-input" formControlName="plate" /></label>
            <label>Owner<input class="ce-input" formControlName="ownerName" /></label>
            <ce-apartment-picker
              formControlName="apartmentId"
              label="Apartment"
              inputId="vh-apartment"
              placeholder="Select apartment..." />
            <label>Brand<input class="ce-input" formControlName="brand" /></label>
            <label>Model<input class="ce-input" formControlName="model" /></label>
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
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--spacing-6); }
    .page-title { font-size: var(--font-size-2xl); margin: 0; }
    .page-subtitle { color: var(--color-text-secondary); font-size: var(--font-size-sm); }
    .page-error {
      margin-bottom: var(--spacing-4); padding: var(--spacing-3); border-radius: var(--radius-lg);
      background: var(--color-danger-light); color: var(--color-danger); font-size: var(--font-size-sm);
    }
    .ce-card { background: var(--color-surface-elevated); border: 1px solid var(--color-border); border-radius: var(--radius-xl); overflow: hidden; }
    .ce-table { width: 100%; border-collapse: collapse; font-size: var(--font-size-sm); }
    .ce-table th, .ce-table td { padding: var(--spacing-3) var(--spacing-4); border-bottom: 1px solid var(--color-border); text-align: left; }
    .ce-table thead { background: var(--color-neutral-light); }
    .ce-badge { padding: var(--spacing-1) var(--spacing-2); border-radius: var(--radius-full); background: var(--color-neutral-light); font-size: var(--font-size-xs); }
    .ce-button { border: 0; border-radius: var(--radius-lg); padding: 0 var(--spacing-4); height: 2.5rem; cursor: pointer; font-family: inherit; display: inline-flex; align-items: center; gap: var(--spacing-2); }
    .variant-primary { background: var(--color-primary); color: white; }
    .variant-ghost { background: transparent; }
    .ce-button:disabled { opacity: 0.6; cursor: not-allowed; }
    .ce-modal-backdrop { position: fixed; inset: 0; background: rgb(0 0 0 / 0.5); display: flex; align-items: center; justify-content: center; z-index: 100; }
    .ce-modal { background: var(--color-surface-elevated); padding: var(--spacing-6); border-radius: var(--radius-xl); width: min(28rem, 90vw); display: flex; flex-direction: column; gap: var(--spacing-3); }
    .ce-input { width: 100%; padding: var(--spacing-2) var(--spacing-3); border: 1px solid var(--color-border); border-radius: var(--radius-lg); margin-top: var(--spacing-1); font-family: inherit; }
    label { display: block; font-size: var(--font-size-sm); }
    .actions { display: flex; justify-content: flex-end; gap: var(--spacing-2); margin-top: var(--spacing-2); }
    .form-error-banner {
      padding: var(--spacing-3); border-radius: var(--radius-lg);
      background: var(--color-danger-light); color: var(--color-danger); font-size: var(--font-size-sm);
    }
    .spinner {
      width: 1rem; height: 1rem; border: 2px solid currentColor; border-top-color: transparent;
      border-radius: 50%; animation: spin 1s linear infinite;
    }
    .text-secondary { color: var(--color-text-secondary); }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VehiclesPage {
  private readonly api = inject(VehiclesApiService);
  private readonly apartmentsApi = inject(ApartmentsApiService);
  private readonly fb = inject(FormBuilder);

  vehicles = signal<VehicleResponse[]>([]);
  apartmentLabels = signal<Record<string, string>>({});
  loading = signal(true);
  creating = signal(false);
  createOpen = signal(false);
  pageError = signal<string | null>(null);
  createError = signal<string | null>(null);

  form = this.fb.group({
    plate: ['', Validators.required],
    ownerName: [''],
    apartmentId: [null],
    brand: [''],
    model: [''],
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

  load(): void {
    this.loading.set(true);
    this.pageError.set(null);
    this.api.list().subscribe({
      next: (data) => { this.vehicles.set(data); this.loading.set(false); },
      error: (err) => {
        this.pageError.set(getApiErrorMessage(err, 'Failed to load vehicles'));
        this.loading.set(false);
      },
    });
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
      plate: v.plate!,
      ownerName: v.ownerName || null,
      apartmentId: v.apartmentId || null,
      brand: v.brand || null,
      model: v.model || null,
    }).subscribe({
      next: () => { this.creating.set(false); this.closeCreate(); this.load(); },
      error: (err) => {
        this.creating.set(false);
        this.createError.set(getApiErrorMessage(err, 'Failed to create vehicle'));
      },
    });
  }
}

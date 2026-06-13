import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { VehiclesApiService, VehicleResponse } from './vehicles-api.service';

@Component({
  selector: 'ce-vehicles-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Vehicles</h1>
        <p class="page-subtitle">{{ vehicles().length }} registered vehicles</p>
      </div>
      <button class="ce-button variant-primary size-md" (click)="openCreate()">+ Add vehicle</button>
    </div>

    @if (loading()) {
      <p class="text-secondary">Loading vehicles...</p>
    } @else {
      <div class="ce-card">
        <table class="ce-table">
          <thead>
            <tr>
              <th>Plate</th>
              <th>Owner</th>
              <th>Brand / Model</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            @for (vehicle of vehicles(); track vehicle.id) {
              <tr>
                <td>{{ vehicle.plate }}</td>
                <td>{{ vehicle.ownerName ?? '—' }}</td>
                <td>{{ vehicle.brand ?? '—' }} {{ vehicle.model ?? '' }}</td>
                <td><span class="ce-badge">{{ vehicle.active ? 'Active' : 'Inactive' }}</span></td>
              </tr>
            } @empty {
              <tr><td colspan="4" class="text-secondary">No vehicles yet.</td></tr>
            }
          </tbody>
        </table>
      </div>
    }

    @if (createOpen()) {
      <div class="ce-modal-backdrop" (click)="closeCreate()">
        <div class="ce-modal" (click)="$event.stopPropagation()">
          <h3>Add vehicle</h3>
          <form [formGroup]="form" (ngSubmit)="onCreate()">
            <label>Plate<input class="ce-input" formControlName="plate" /></label>
            <label>Owner<input class="ce-input" formControlName="ownerName" /></label>
            <label>Brand<input class="ce-input" formControlName="brand" /></label>
            <label>Model<input class="ce-input" formControlName="model" /></label>
            <div class="actions">
              <button type="button" class="ce-button variant-ghost" (click)="closeCreate()">Cancel</button>
              <button type="submit" class="ce-button variant-primary" [disabled]="form.invalid || creating()">Create</button>
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
    .ce-card { background: var(--color-surface-elevated); border: 1px solid var(--color-border); border-radius: var(--radius-xl); overflow: hidden; }
    .ce-table { width: 100%; border-collapse: collapse; font-size: var(--font-size-sm); }
    .ce-table th, .ce-table td { padding: var(--spacing-3) var(--spacing-4); border-bottom: 1px solid var(--color-border); text-align: left; }
    .ce-table thead { background: var(--color-neutral-light); }
    .ce-badge { padding: var(--spacing-1) var(--spacing-2); border-radius: var(--radius-full); background: var(--color-neutral-light); font-size: var(--font-size-xs); }
    .ce-button { border: 0; border-radius: var(--radius-lg); padding: 0 var(--spacing-4); height: 2.5rem; cursor: pointer; font-family: inherit; }
    .variant-primary { background: var(--color-primary); color: white; }
    .variant-ghost { background: transparent; }
    .ce-modal-backdrop { position: fixed; inset: 0; background: rgb(0 0 0 / 0.5); display: flex; align-items: center; justify-content: center; z-index: 100; }
    .ce-modal { background: var(--color-surface-elevated); padding: var(--spacing-6); border-radius: var(--radius-xl); width: min(28rem, 90vw); display: flex; flex-direction: column; gap: var(--spacing-3); }
    .ce-input { width: 100%; padding: var(--spacing-2) var(--spacing-3); border: 1px solid var(--color-border); border-radius: var(--radius-lg); margin-top: var(--spacing-1); }
    label { display: block; font-size: var(--font-size-sm); }
    .actions { display: flex; justify-content: flex-end; gap: var(--spacing-2); margin-top: var(--spacing-2); }
    .text-secondary { color: var(--color-text-secondary); }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VehiclesPage {
  private readonly api = inject(VehiclesApiService);
  private readonly fb = inject(FormBuilder);

  vehicles = signal<VehicleResponse[]>([]);
  loading = signal(true);
  creating = signal(false);
  createOpen = signal(false);

  form = this.fb.group({
    plate: ['', Validators.required],
    ownerName: [''],
    brand: [''],
    model: [''],
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.list().subscribe({
      next: (data) => { this.vehicles.set(data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.form.reset(); this.createOpen.set(true); }
  closeCreate(): void { this.createOpen.set(false); }

  onCreate(): void {
    if (this.form.invalid) return;
    this.creating.set(true);
    const v = this.form.value;
    this.api.create({
      plate: v.plate!,
      ownerName: v.ownerName || null,
      brand: v.brand || null,
      model: v.model || null,
    }).subscribe({
      next: () => { this.creating.set(false); this.closeCreate(); this.load(); },
      error: () => this.creating.set(false),
    });
  }
}

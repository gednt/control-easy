import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { VisitsApiService, VisitResponse } from './visits-api.service';

@Component({
  selector: 'ce-visits-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Visits</h1>
        <p class="page-subtitle">{{ visits().length }} visit records</p>
      </div>
      <button class="ce-button variant-primary size-md" (click)="openCreate()">+ Add visit</button>
    </div>

    @if (loading()) {
      <p class="text-secondary">Loading visits...</p>
    } @else {
      <div class="ce-card">
        <table class="ce-table">
          <thead>
            <tr>
              <th>Visitor</th>
              <th>Document</th>
              <th>Status</th>
              <th>Created</th>
            </tr>
          </thead>
          <tbody>
            @for (visit of visits(); track visit.id) {
              <tr>
                <td>{{ visit.visitorName }}</td>
                <td>{{ visit.visitorDocument }}</td>
                <td><span class="ce-badge">{{ visit.status }}</span></td>
                <td>{{ visit.createdAtUtc | date:'short' }}</td>
              </tr>
            } @empty {
              <tr><td colspan="4" class="text-secondary">No visits yet.</td></tr>
            }
          </tbody>
        </table>
      </div>
    }

    @if (createOpen()) {
      <div class="ce-modal-backdrop" (click)="closeCreate()">
        <div class="ce-modal" (click)="$event.stopPropagation()">
          <h3>Add visit</h3>
          <form [formGroup]="form" (ngSubmit)="onCreate()">
            <label>Name<input class="ce-input" formControlName="visitorName" /></label>
            <label>Document<input class="ce-input" formControlName="visitorDocument" /></label>
            <label>Purpose<input class="ce-input" formControlName="purpose" /></label>
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
export class VisitsPage {
  private readonly api = inject(VisitsApiService);
  private readonly fb = inject(FormBuilder);

  visits = signal<VisitResponse[]>([]);
  loading = signal(true);
  creating = signal(false);
  createOpen = signal(false);

  form = this.fb.group({
    visitorName: ['', Validators.required],
    visitorDocument: ['', Validators.required],
    purpose: [''],
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.list().subscribe({
      next: (data) => { this.visits.set(data); this.loading.set(false); },
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
      visitorName: v.visitorName!,
      visitorDocument: v.visitorDocument!,
      purpose: v.purpose || null,
    }).subscribe({
      next: () => { this.creating.set(false); this.closeCreate(); this.load(); },
      error: () => this.creating.set(false),
    });
  }
}

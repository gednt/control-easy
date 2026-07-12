import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AdministrationApiService, AuditLogResponse, ConfigurationResponse } from './administration-api.service';

@Component({
  selector: 'ce-administration-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Administration</h1>
        <p class="page-subtitle">Audit logs and tenant configuration</p>
      </div>
      <button class="ce-button variant-primary size-md" (click)="openCreate()">+ Add configuration</button>
    </div>

    <div class="tabs">
      <button class="tab" [class.active]="tab() === 'audit'" (click)="tab.set('audit')">Audit logs</button>
      <button class="tab" [class.active]="tab() === 'config'" (click)="tab.set('config')">Configurations</button>
    </div>

    @if (loading()) {
      <p class="text-secondary">Loading...</p>
    } @else if (tab() === 'audit') {
      <div class="ce-card">
        <table class="ce-table">
          <thead>
            <tr><th>Action</th><th>Entity</th><th>By</th><th>When</th></tr>
          </thead>
          <tbody>
            @for (log of auditLogs(); track log.id) {
              <tr>
                <td>{{ log.action }}</td>
                <td>{{ log.entityType }}</td>
                <td>{{ log.performedByName ?? '—' }}</td>
                <td>{{ log.createdAtUtc | date:'short' }}</td>
              </tr>
            } @empty {
              <tr><td colspan="4" class="text-secondary">No audit logs yet.</td></tr>
            }
          </tbody>
        </table>
      </div>
    } @else {
      <div class="ce-card">
        <table class="ce-table">
          <thead>
            <tr><th>Key</th><th>Value</th><th>Description</th></tr>
          </thead>
          <tbody>
            @for (config of configurations(); track config.id) {
              <tr>
                <td>{{ config.key }}</td>
                <td>{{ config.value }}</td>
                <td>{{ config.description ?? '—' }}</td>
              </tr>
            } @empty {
              <tr><td colspan="3" class="text-secondary">No configurations yet.</td></tr>
            }
          </tbody>
        </table>
      </div>
    }

    @if (createOpen()) {
      <div class="ce-modal-backdrop" (click)="closeCreate()">
        <div class="ce-modal" (click)="$event.stopPropagation()">
          <h3>Add configuration</h3>
          <form [formGroup]="form" (ngSubmit)="onCreate()">
            <label>Key<input class="ce-input" formControlName="key" /></label>
            <label>Value<input class="ce-input" formControlName="value" /></label>
            <label>Description<input class="ce-input" formControlName="description" /></label>
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
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--space-6); }
    .page-title { font-size: var(--font-size-2xl); margin: 0; }
    .page-subtitle { color: var(--color-text-secondary); font-size: var(--font-size-sm); }
    .tabs { display: flex; gap: var(--space-2); margin-bottom: var(--space-4); }
    .tab { border: 1px solid var(--color-border); background: var(--color-surface); border-radius: var(--radius-lg); padding: var(--space-2) var(--space-4); cursor: pointer; font-family: inherit; }
    .tab.active { background: var(--color-primary); color: var(--color-text-on-primary); border-color: var(--color-primary); }
    .ce-card { background: var(--color-surface-elevated); border: 1px solid var(--color-border); border-radius: var(--radius-xl); overflow: hidden; }
    .ce-table { width: 100%; border-collapse: collapse; font-size: var(--font-size-sm); }
    .ce-table th, .ce-table td { padding: var(--space-3) var(--space-4); border-bottom: 1px solid var(--color-border); text-align: left; }
    .ce-table thead { background: var(--color-neutral-light); }
    .ce-button { border: 0; border-radius: var(--radius-lg); padding: 0 var(--space-4); height: 2.5rem; cursor: pointer; font-family: inherit; }
    .variant-primary { background: var(--color-primary); color: var(--color-text-on-primary); }
    .variant-ghost { background: transparent; }
    .ce-modal-backdrop { position: fixed; inset: 0; background: rgb(0 0 0 / 0.5); display: flex; align-items: center; justify-content: center; z-index: 100; }
    .ce-modal { background: var(--color-surface-elevated); padding: var(--space-6); border-radius: var(--radius-xl); width: min(28rem, 90vw); display: flex; flex-direction: column; gap: var(--space-3); }
    .ce-input { width: 100%; padding: var(--space-2) var(--space-3); border: 1px solid var(--color-border); border-radius: var(--radius-lg); margin-top: var(--space-1); }
    label { display: block; font-size: var(--font-size-sm); }
    .actions { display: flex; justify-content: flex-end; gap: var(--space-2); margin-top: var(--space-2); }
    .text-secondary { color: var(--color-text-secondary); }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdministrationPage {
  private readonly api = inject(AdministrationApiService);
  private readonly fb = inject(FormBuilder);

  auditLogs = signal<AuditLogResponse[]>([]);
  configurations = signal<ConfigurationResponse[]>([]);
  loading = signal(true);
  creating = signal(false);
  createOpen = signal(false);
  tab = signal<'audit' | 'config'>('audit');

  form = this.fb.group({
    key: ['', Validators.required],
    value: ['', Validators.required],
    description: [''],
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.listAuditLogs().subscribe({
      next: (logs) => {
        this.auditLogs.set(logs);
        this.api.listConfigurations().subscribe({
          next: (configs) => { this.configurations.set(configs); this.loading.set(false); },
          error: () => this.loading.set(false),
        });
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.form.reset(); this.createOpen.set(true); }
  closeCreate(): void { this.createOpen.set(false); }

  onCreate(): void {
    if (this.form.invalid) return;
    this.creating.set(true);
    const v = this.form.value;
    this.api.createConfiguration({
      key: v.key!,
      value: v.value!,
      description: v.description || null,
    }).subscribe({
      next: () => { this.creating.set(false); this.closeCreate(); this.load(); this.tab.set('config'); },
      error: () => this.creating.set(false),
    });
  }
}

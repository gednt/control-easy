import { Component, ChangeDetectionStrategy, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ServiceProvidersApiService, ServiceProviderResponse } from './service-providers-api.service';
import { CeButtonComponent, CeModalComponent, CePhotoPanelComponent } from '../../design-system';

@Component({
  selector: 'ce-service-providers-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CeButtonComponent, CeModalComponent, CePhotoPanelComponent],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Service Providers</h1>
        <p class="page-subtitle">{{ providers().length }} registered providers</p>
      </div>
      <button class="ce-button variant-primary size-md" (click)="openCreate()">+ Add provider</button>
    </div>

    @if (loading()) {
      <p class="text-secondary">Loading service providers...</p>
    } @else {
      <div class="ce-card">
        <table class="ce-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Document</th>
              <th>Service type</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            @for (provider of providers(); track provider.id) {
              <tr>
                <td>{{ provider.name }}</td>
                <td>{{ provider.document }}</td>
                <td>{{ provider.serviceType ?? '—' }}</td>
                <td><span class="ce-badge">{{ provider.active ? 'Active' : 'Inactive' }}</span></td>
                <td>
                  <button class="ce-button variant-ghost size-sm" (click)="openPhotos(provider)">
                    Photos
                  </button>
                </td>
              </tr>
            } @empty {
              <tr><td colspan="5" class="text-secondary">No service providers yet.</td></tr>
            }
          </tbody>
        </table>
      </div>
    }

    @if (createOpen()) {
      <div class="ce-modal-backdrop" (click)="closeCreate()">
        <div class="ce-modal" (click)="$event.stopPropagation()">
          <h3>Add service provider</h3>
          <form [formGroup]="form" (ngSubmit)="onCreate()">
            <label>Name<input class="ce-input" formControlName="name" /></label>
            <label>Document<input class="ce-input" formControlName="document" /></label>
            <label>Service type<input class="ce-input" formControlName="serviceType" /></label>
            <div class="actions">
              <button type="button" class="ce-button variant-ghost" (click)="closeCreate()">Cancel</button>
              <button type="submit" class="ce-button variant-primary" [disabled]="form.invalid || creating()">Create</button>
            </div>
          </form>
        </div>
      </div>
    }

    <ce-modal
      [open]="photosModalOpen()"
      [title]="photosProvider() ? photosProvider()!.name + ' — Photos' : 'Service provider photos'"
      size="lg"
      (openChange)="onPhotosModalOpenChange($event)"
    >
      <ce-photo-panel
        entityType="service-provider"
        [entity]="photosEntity()"
        [canAdd]="true"
        [canDelete]="true"
      />
      <div ce-modal-footer>
        <ce-button variant="ghost" size="sm" (click)="closePhotos()">Close</ce-button>
      </div>
    </ce-modal>
  `,
  styles: [`
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--space-6); }
    .page-title { font-size: var(--font-size-2xl); margin: 0; }
    .page-subtitle { color: var(--color-text-secondary); font-size: var(--font-size-sm); }
    .ce-card { background: var(--color-surface-elevated); border: 1px solid var(--color-border); border-radius: var(--radius-xl); overflow: hidden; }
    .ce-table { width: 100%; border-collapse: collapse; font-size: var(--font-size-sm); }
    .ce-table th, .ce-table td { padding: var(--space-3) var(--space-4); border-bottom: 1px solid var(--color-border); text-align: left; }
    .ce-table thead { background: var(--color-neutral-light); }
    .ce-badge { padding: var(--space-1) var(--space-2); border-radius: var(--radius-full); background: var(--color-neutral-light); font-size: var(--font-size-xs); }
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
export class ServiceProvidersPage {
  private readonly api = inject(ServiceProvidersApiService);
  private readonly fb = inject(FormBuilder);

  providers = signal<ServiceProviderResponse[]>([]);
  loading = signal(true);
  creating = signal(false);
  createOpen = signal(false);
  photosModalOpen = signal(false);
  photosProvider = signal<ServiceProviderResponse | null>(null);

  readonly photosEntity = computed(() => {
    const p = this.photosProvider();
    return p ? { id: p.id, displayName: p.name } : null;
  });

  form = this.fb.group({
    name: ['', Validators.required],
    document: ['', Validators.required],
    serviceType: [''],
  });

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.api.list().subscribe({
      next: (data) => { this.providers.set(data); this.loading.set(false); },
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
      name: v.name!,
      document: v.document!,
      serviceType: v.serviceType || null,
    }).subscribe({
      next: () => { this.creating.set(false); this.closeCreate(); this.load(); },
      error: () => this.creating.set(false),
    });
  }

  openPhotos(provider: ServiceProviderResponse): void {
    this.photosProvider.set(provider);
    this.photosModalOpen.set(true);
  }

  closePhotos(): void {
    this.photosModalOpen.set(false);
    this.photosProvider.set(null);
  }

  onPhotosModalOpenChange(open: boolean): void {
    if (!open) this.closePhotos();
  }
}

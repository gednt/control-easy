import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CeStatTileComponent } from '../../design-system/components/stat-tile/stat-tile.component';
import { DashboardApiService, DashboardStatsResponse, RecentVisit } from './dashboard-api.service';
import { getApiErrorMessage } from '../../core/utils/api-error.util';

@Component({
  selector: 'ce-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterLink, CeStatTileComponent],
  template: `
    <div class="page-header">
      <div class="page-title-block">
        <h1 class="page-title">Dashboard</h1>
        <p class="page-subtitle">Welcome to ControlEasy Reborn.</p>
      </div>
      <button class="ce-button variant-secondary size-md" (click)="refresh()">
        &#8635; Refresh
      </button>
    </div>

    @if (pageError()) {
      <div class="page-error">{{ pageError() }}</div>
    }

    @if (loading()) {
      <div class="loading-state">
        <div class="ce-spinner tone-primary size-lg"></div>
        <p>Loading statistics...</p>
      </div>
    } @else if (stats()) {
      <div class="stat-tiles-row">
        <a routerLink="/residents" class="stat-tile-link">
          <ce-stat-tile label="Active Residents" [value]="stats()!.activeResidents"></ce-stat-tile>
        </a>
        <a routerLink="/vehicles" class="stat-tile-link">
          <ce-stat-tile label="Active Vehicles" [value]="stats()!.activeVehicles"></ce-stat-tile>
        </a>
        <a routerLink="/apartments" class="stat-tile-link">
          <ce-stat-tile label="Occupied Apartments" [value]="stats()!.occupiedApartments"></ce-stat-tile>
        </a>
        <a routerLink="/visits" class="stat-tile-link">
          <ce-stat-tile label="Open Visits" [value]="stats()!.openVisits"></ce-stat-tile>
        </a>
      </div>

      <div class="dashboard-section">
        <div class="section-header">
          <h2 class="section-title">Today's Summary</h2>
        </div>
        <div class="summary-row">
          <div class="summary-item">
            <span class="summary-value">{{ stats()!.todayVisits }}</span>
            <span class="summary-label">Visits today</span>
          </div>
          <div class="summary-item">
            <span class="summary-value">{{ stats()!.totalResidents }}</span>
            <span class="summary-label">Total residents</span>
          </div>
          <div class="summary-item">
            <span class="summary-value">{{ stats()!.totalVehicles }}</span>
            <span class="summary-label">Total vehicles</span>
          </div>
          <div class="summary-item">
            <span class="summary-value">{{ stats()!.totalApartments }}</span>
            <span class="summary-label">Apartments</span>
          </div>
        </div>
      </div>

      <div class="dashboard-section">
        <div class="section-header">
          <h2 class="section-title">Recent Visits</h2>
          <a routerLink="/visits" class="section-link">View all</a>
        </div>
        <div class="ce-card">
          <table class="ce-table" aria-label="Recent visits table">
            <thead>
              <tr>
                <th scope="col">Visitor</th>
                <th scope="col">Apartment</th>
                <th scope="col">Purpose</th>
                <th scope="col">Status</th>
                <th scope="col">Time</th>
              </tr>
            </thead>
            <tbody>
              @for (visit of stats()!.recentVisits; track visit.id) {
                <tr>
                  <td class="font-semibold">{{ visit.visitorName }}</td>
                  <td>{{ visit.apartmentLabel ?? '\u2014' }}</td>
                  <td>{{ visit.purpose ?? '\u2014' }}</td>
                  <td>
                    <span class="ce-badge" [class]="getStatusBadgeClass(visit.status)">
                      {{ getStatusLabel(visit.status) }}
                    </span>
                  </td>
                  <td>{{ visit.createdAtUtc | date:'short' }}</td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="5">
                    <div class="ce-empty-state" style="padding: var(--space-8) var(--space-4);">
                      <div class="ce-empty-state-icon">&#128197;</div>
                      <div class="ce-empty-state-title">No visits yet</div>
                      <div class="ce-empty-state-description text-secondary">
                        Visits will appear here as they are registered.
                      </div>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    }
  `,
  styles: [`
    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
      flex-wrap: wrap;
      margin-bottom: var(--space-6);
    }
    .page-title-block { min-width: 0; }
    .page-title { font-size: var(--font-size-2xl); margin-bottom: var(--space-1); }
    .page-subtitle { color: var(--color-text-secondary); font-size: var(--font-size-sm); }
    .page-error {
      margin-bottom: var(--space-4);
      padding: var(--space-3);
      border-radius: var(--radius-lg);
      background: var(--color-danger-light);
      color: var(--color-danger);
      font-size: var(--font-size-sm);
    }

    .ce-button {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: var(--space-2);
      font-weight: var(--font-weight-medium);
      border: 1px solid transparent;
      border-radius: var(--radius-lg);
      cursor: pointer;
      user-select: none;
      white-space: nowrap;
      transition: transform var(--duration-fast) var(--ease-out), box-shadow var(--duration-fast) var(--ease-out), background-color var(--duration-fast) var(--ease-out), border-color var(--duration-fast) var(--ease-out), color var(--duration-fast) var(--ease-out), opacity var(--duration-fast) var(--ease-out);
      text-decoration: none;
      font-family: inherit;
    }
    .ce-button.variant-secondary { background: var(--color-surface); color: var(--color-text-primary); border-color: var(--color-border); }
    .ce-button.variant-secondary:hover:not(:disabled) { background: var(--color-surface-elevated); }
    .ce-button.size-md { height: 2.5rem; padding: 0 var(--space-4); font-size: var(--font-size-sm); }

    .loading-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: var(--space-12);
      gap: var(--space-4);
      color: var(--color-text-secondary);
    }
    .ce-spinner {
      display: inline-block;
      border-radius: var(--radius-full);
      border: 2px solid currentColor;
      border-top-color: transparent;
      animation: spin-slow 1.4s linear infinite;
    }
    .ce-spinner.size-lg { width: 2rem; height: 2rem; border-width: 3px; }
    .ce-spinner.tone-primary { color: var(--color-primary); }

    .stat-tiles-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr));
      gap: var(--space-4);
      margin-bottom: var(--space-6);
    }
    .stat-tile-link {
      text-decoration: none;
      color: inherit;
      display: block;
    }
    .stat-tile-link:hover ce-stat-tile div {
      border-color: var(--color-primary);
      box-shadow: var(--shadow-primary-glow);
    }

    .dashboard-section {
      margin-bottom: var(--space-6);
    }
    .section-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: var(--space-4);
    }
    .section-title { font-size: var(--font-size-lg); font-weight: var(--font-weight-semibold); }
    .section-link {
      font-size: var(--font-size-sm);
      color: var(--color-primary);
      text-decoration: none;
      font-weight: var(--font-weight-medium);
    }
    .section-link:hover { text-decoration: underline; }

    .summary-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(10rem, 1fr));
      gap: var(--space-3);
    }
    .summary-item {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: var(--space-4) var(--space-3);
      background: var(--color-surface-elevated);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-xl);
      gap: var(--space-1);
    }
    .summary-value { font-size: var(--font-size-xl); font-weight: var(--font-weight-bold); color: var(--color-text-primary); }
    .summary-label { font-size: var(--font-size-xs); color: var(--color-text-secondary); }

    .ce-card {
      background: var(--color-surface-elevated);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-card);
      overflow: hidden;
    }
    .ce-table { width: 100%; border-collapse: collapse; font-size: var(--font-size-sm); }
    .ce-table thead { background: var(--color-neutral-light); position: sticky; top: 0; }
    .ce-table th {
      text-align: left;
      padding: var(--space-3) var(--space-4);
      font-weight: var(--font-weight-semibold);
      color: var(--color-text-secondary);
      font-size: var(--font-size-xs);
      text-transform: uppercase;
      letter-spacing: 0.05em;
      border-bottom: 1px solid var(--color-border);
    }
    .ce-table td {
      padding: var(--space-3) var(--space-4);
      border-bottom: 1px solid var(--color-border);
      color: var(--color-text-primary);
    }
    .ce-table tbody tr:last-child td { border-bottom: 0; }
    .ce-table tbody tr:hover { background: color-mix(in oklch, var(--color-primary) 3%, transparent); }

    .ce-badge {
      display: inline-flex;
      align-items: center;
      gap: var(--space-1);
      font-weight: var(--font-weight-medium);
      border-radius: var(--radius-full);
      border: 1px solid transparent;
      font-size: var(--font-size-xs);
      line-height: 1;
      padding: var(--space-1) var(--space-2);
    }
    .badge-pending { background: var(--color-neutral-light); color: var(--color-text-secondary); }
    .badge-onsite { background: var(--color-success-light); color: var(--color-success); border-color: color-mix(in oklch, var(--color-success) 30%, transparent); }
    .badge-done { background: var(--color-neutral-light); color: var(--color-text-muted); }
    .badge-cancelled { background: var(--color-danger-light); color: var(--color-danger); }

    .ce-empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: var(--space-12) var(--space-6);
      gap: var(--space-3);
      color: var(--color-text-secondary);
    }
    .ce-empty-state-icon {
      width: 4rem;
      height: 4rem;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      background: var(--color-neutral-light);
      color: var(--color-text-muted);
      border-radius: var(--radius-full);
      margin-bottom: var(--space-2);
      font-size: 1.5rem;
    }
    .ce-empty-state-title { font-size: var(--font-size-lg); font-weight: var(--font-weight-semibold); color: var(--color-text-primary); }
    .ce-empty-state-description { max-width: 24rem; }

    .font-semibold { font-weight: var(--font-weight-semibold); }
    .text-secondary { color: var(--color-text-secondary); }

    @keyframes spin-slow { to { transform: rotate(360deg); } }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPage {
  private readonly api = inject(DashboardApiService);

  stats = signal<DashboardStatsResponse | null>(null);
  loading = signal(true);
  pageError = signal<string | null>(null);

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.pageError.set(null);
    this.api.getStats().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.pageError.set(getApiErrorMessage(err, 'Failed to load dashboard statistics'));
        this.loading.set(false);
      },
    });
  }

  refresh(): void {
    this.load();
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
}
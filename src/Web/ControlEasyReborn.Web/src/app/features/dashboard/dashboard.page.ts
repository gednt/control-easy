import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { ThemeService } from '../../design-system/theme/theme.service';

@Component({
  selector: 'ce-dashboard-page',
  standalone: true,
  template: `
    <div class="page-header">
      <div class="page-title-block">
        <h1 class="page-title">Dashboard</h1>
        <p class="page-subtitle">Welcome to ControlEasy Reborn.</p>
      </div>
    </div>
    <div class="ce-card accent-primary" style="padding: var(--spacing-6);">
      <h2 style="margin-bottom: var(--spacing-2);">Getting started</h2>
      <p class="text-secondary">Use the sidebar to navigate to Residents, Visits, Vehicles, and more.</p>
    </div>
  `,
  styles: [`
    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--spacing-4);
      flex-wrap: wrap;
      margin-bottom: var(--spacing-6);
    }
    .page-title-block { min-width: 0; }
    .page-title { font-size: var(--font-size-2xl); margin-bottom: var(--spacing-1); }
    .page-subtitle { color: var(--color-text-secondary); font-size: var(--font-size-sm); }
    .ce-card {
      background: var(--color-surface-elevated);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-card);
    }
    .ce-card.accent-primary { border-left: 4px solid var(--color-primary); }
    .text-secondary { color: var(--color-text-secondary); }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPage {}
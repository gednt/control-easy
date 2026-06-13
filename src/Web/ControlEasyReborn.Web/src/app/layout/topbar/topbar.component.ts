import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ThemeService } from '../../design-system/theme/theme.service';

@Component({
  selector: 'ce-topbar',
  standalone: true,
  imports: [RouterLink],
  template: `
    <header class="topbar" role="banner">
      <button class="topbar-menu-btn icon-btn" aria-label="Open menu" (click)="toggleMobileMenu()">
        &#9776;
      </button>
      <nav class="ce-breadcrumbs" aria-label="Breadcrumb">
        <ol>
          <li><a routerLink="/">Home</a></li>
        </ol>
      </nav>
      <div class="topbar-actions">
        <button class="icon-btn" aria-label="Toggle theme" (click)="themeService.toggle()">
          {{ themeService.resolvedTheme() === 'dark' ? '&#9789;' : '&#9788;' }}
        </button>
        <div class="topbar-avatar">A</div>
      </div>
    </header>
  `,
  styles: [`
    .topbar {
      position: sticky;
      top: 0;
      z-index: 40;
      display: flex;
      align-items: center;
      gap: var(--spacing-4);
      height: var(--topbar-height);
      padding: 0 var(--spacing-6);
      background: var(--color-surface);
      border-bottom: 1px solid var(--color-border);
      backdrop-filter: blur(8px);
    }
    @media (max-width: 639px) {
      .topbar { padding: 0 var(--spacing-3); }
    }
    .topbar-menu-btn {
      display: none;
      background: transparent;
      border: 0;
      width: 44px;
      height: 44px;
      align-items: center;
      justify-content: center;
      border-radius: var(--radius-md);
      cursor: pointer;
      color: var(--color-text-primary);
      font-size: 1.25rem;
    }
    @media (max-width: 639px) {
      .topbar-menu-btn { display: inline-flex; }
    }
    .topbar-menu-btn:hover { background: var(--color-neutral-light); }
    .ce-breadcrumbs ol {
      display: flex;
      align-items: center;
      gap: var(--spacing-1);
      list-style: none;
      margin: 0;
      padding: 0;
      font-size: var(--font-size-sm);
      color: var(--color-text-secondary);
    }
    .ce-breadcrumbs a { color: var(--color-text-secondary); text-decoration: none; }
    .ce-breadcrumbs a:hover { color: var(--color-primary); }
    .topbar-actions {
      display: flex;
      align-items: center;
      gap: var(--spacing-2);
      margin-left: auto;
    }
    .icon-btn {
      width: 44px;
      height: 44px;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      background: transparent;
      border: 0;
      border-radius: var(--radius-md);
      cursor: pointer;
      color: var(--color-text-primary);
      transition: background var(--duration-fast);
      font-size: 1.25rem;
    }
    .icon-btn:hover { background: var(--color-neutral-light); }
    .topbar-avatar {
      width: 2rem;
      height: 2rem;
      border-radius: var(--radius-full);
      background: linear-gradient(135deg, var(--color-primary), #ec4899);
      color: white;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-weight: var(--font-weight-semibold);
      font-size: 0.75rem;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TopbarComponent {
  readonly themeService = inject(ThemeService);

  toggleMobileMenu(): void {
  }
}
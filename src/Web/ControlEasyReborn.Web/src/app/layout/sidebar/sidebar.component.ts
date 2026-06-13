import { Component, ChangeDetectionStrategy, signal, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ThemeService } from '../../design-system/theme/theme.service';

@Component({
  selector: 'ce-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <aside class="sidebar" aria-label="Main navigation">
      <div class="sidebar-brand">
        <div class="sidebar-brand-mark">CE</div>
        <div class="sidebar-brand-text">ControlEasy</div>
      </div>
      <nav class="sidebar-nav">
        <div class="sidebar-nav-section">
          <div class="sidebar-nav-title">Overview</div>
          <a class="sidebar-nav-item"
             routerLink="/"
             routerLinkActive="active"
             [routerLinkActiveOptions]="{exact: true}">
            <span class="sidebar-nav-icon">&#9783;</span>
            <span class="sidebar-nav-text">Dashboard</span>
          </a>
        </div>
        <div class="sidebar-nav-section">
          <div class="sidebar-nav-title">Modules</div>
          <a class="sidebar-nav-item" routerLink="/residents" routerLinkActive="active">
            <span class="sidebar-nav-icon">&#9787;</span>
            <span class="sidebar-nav-text">Residents</span>
          </a>
          <a class="sidebar-nav-item" routerLink="/visits" routerLinkActive="active">
            <span class="sidebar-nav-icon">&#9788;</span>
            <span class="sidebar-nav-text">Visits</span>
          </a>
          <a class="sidebar-nav-item" routerLink="/vehicles" routerLinkActive="active">
            <span class="sidebar-nav-icon">&#9789;</span>
            <span class="sidebar-nav-text">Vehicles</span>
          </a>
          <a class="sidebar-nav-item" routerLink="/service-providers" routerLinkActive="active">
            <span class="sidebar-nav-icon">&#9790;</span>
            <span class="sidebar-nav-text">Service Providers</span>
          </a>
        </div>
        <div class="sidebar-nav-section">
          <div class="sidebar-nav-title">Settings</div>
          <a class="sidebar-nav-item" routerLink="/administration" routerLinkActive="active">
            <span class="sidebar-nav-icon">&#9881;</span>
            <span class="sidebar-nav-text">Administration</span>
          </a>
        </div>
      </nav>
      <div class="sidebar-footer">
        <div class="sidebar-footer-avatar">A</div>
        <div class="sidebar-footer-text">
          <div class="sidebar-footer-name">Admin</div>
          <div class="sidebar-footer-role">TenantAdmin</div>
        </div>
      </div>
    </aside>
  `,
  styles: [`
    .sidebar {
      background: var(--color-sidebar-bg);
      color: var(--color-sidebar-text);
      border-right: 1px solid var(--color-sidebar-border);
      display: flex;
      flex-direction: column;
      position: sticky;
      top: 0;
      height: 100vh;
      overflow: hidden;
    }
    @media (max-width: 639px) {
      .sidebar { display: none; }
    }
    .sidebar-brand {
      display: flex;
      align-items: center;
      gap: var(--spacing-3);
      padding: var(--spacing-5) var(--spacing-4);
      border-bottom: 1px solid var(--color-sidebar-border);
      min-height: var(--topbar-height);
    }
    .sidebar-brand-mark {
      width: 2rem;
      height: 2rem;
      background: var(--color-primary);
      color: white;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border-radius: var(--radius-md);
      font-weight: var(--font-weight-bold);
      font-size: 1.1rem;
      flex-shrink: 0;
    }
    .sidebar-brand-text {
      font-weight: var(--font-weight-semibold);
      color: white;
      white-space: nowrap;
    }
    @media (max-width: 1023px) {
      .sidebar-brand-text,
      .sidebar-nav-text,
      .sidebar-nav-title,
      .sidebar-footer-text { display: none; }
    }
    .sidebar-nav {
      flex: 1;
      padding: var(--spacing-3);
      overflow-y: auto;
    }
    .sidebar-nav-section { margin-top: var(--spacing-4); }
    .sidebar-nav-section:first-child { margin-top: 0; }
    .sidebar-nav-title {
      font-size: 0.65rem;
      font-weight: var(--font-weight-semibold);
      color: var(--color-sidebar-text-muted);
      text-transform: uppercase;
      letter-spacing: 0.1em;
      padding: var(--spacing-2) var(--spacing-3);
    }
    .sidebar-nav-item {
      display: flex;
      align-items: center;
      gap: var(--spacing-3);
      padding: var(--spacing-2) var(--spacing-3);
      color: var(--color-sidebar-text);
      border-radius: var(--radius-md);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-medium);
      text-decoration: none;
      margin-bottom: var(--spacing-1);
      transition: background var(--duration-fast) var(--ease-out), color var(--duration-fast) var(--ease-out);
    }
    .sidebar-nav-item:hover {
      background: var(--color-sidebar-hover-bg);
      color: white;
    }
    .sidebar-nav-item.active {
      background: var(--color-sidebar-active-bg);
      color: var(--color-sidebar-active-text);
    }
    .sidebar-nav-icon {
      width: 1.25rem;
      height: 1.25rem;
      flex-shrink: 0;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-size: 1rem;
    }
    .sidebar-footer {
      padding: var(--spacing-3);
      border-top: 1px solid var(--color-sidebar-border);
      display: flex;
      align-items: center;
      gap: var(--spacing-3);
    }
    .sidebar-footer-avatar {
      width: 2.5rem;
      height: 2.5rem;
      border-radius: var(--radius-full);
      background: linear-gradient(135deg, var(--color-primary), #ec4899);
      color: white;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-weight: var(--font-weight-semibold);
      font-size: 0.85rem;
      flex-shrink: 0;
    }
    .sidebar-footer-name {
      color: white;
      font-size: 0.85rem;
      font-weight: var(--font-weight-medium);
    }
    .sidebar-footer-role {
      color: var(--color-sidebar-text-muted);
      font-size: 0.7rem;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {}
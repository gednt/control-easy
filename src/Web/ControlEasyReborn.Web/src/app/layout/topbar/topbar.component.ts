import { Component, ChangeDetectionStrategy, ElementRef, HostListener, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ThemeService } from '../../design-system/theme/theme.service';
import { DemoInfoService } from '../../core/services/demo-info.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'ce-topbar',
  standalone: true,
  imports: [RouterLink],
  template: `
    <header class="topbar" role="banner">
      <button class="topbar-menu-btn icon-btn" type="button" aria-label="Open menu" (click)="toggleMobileMenu()">
        &#9776;
      </button>
      <nav class="ce-breadcrumbs" aria-label="Breadcrumb">
        <ol>
          <li><a routerLink="/">Home</a></li>
        </ol>
      </nav>
      <div class="topbar-actions">
        @if (demoInfo.enabled()) {
          <a class="topbar-help-link" routerLink="/help/demo">Help → Demo guide</a>
        }
        <button class="icon-btn" type="button" aria-label="Toggle theme" (click)="themeService.toggle()">
          {{ themeService.resolvedTheme() === 'dark' ? '&#9789;' : '&#9788;' }}
        </button>
        <div class="topbar-profile">
          <button class="topbar-avatar"
                  type="button"
                  aria-label="Open profile menu"
                  aria-haspopup="menu"
                  [attr.aria-expanded]="profileMenuOpen()"
                  (click)="toggleProfileMenu()">
            {{ initials() }}
          </button>

          @if (profileMenuOpen()) {
            <div class="profile-menu" role="menu" aria-label="Profile menu">
              <div class="profile-menu-header">
                <div class="profile-menu-name">{{ displayName() }}</div>
                <div class="profile-menu-role">{{ roleLabel() }}</div>
              </div>
              <button class="profile-menu-item danger"
                      type="button"
                      role="menuitem"
                      (click)="logout()">
                Sign out
              </button>
            </div>
          }
        </div>
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
    .topbar-help-link {
      font-size: var(--font-size-sm);
      color: var(--color-text-secondary);
      text-decoration: none;
      padding: var(--spacing-1) var(--spacing-2);
      border-radius: var(--radius-md);
    }
    .topbar-help-link:hover { color: var(--color-primary); background: var(--color-neutral-light); }
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
    .topbar-profile {
      position: relative;
      display: inline-flex;
    }
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
      border: 0;
      cursor: pointer;
      font-family: inherit;
      transition: transform var(--duration-fast) var(--ease-out), box-shadow var(--duration-fast) var(--ease-out);
    }
    .topbar-avatar:hover {
      transform: translateY(-1px);
      box-shadow: var(--shadow-primary-glow);
    }
    .profile-menu {
      position: absolute;
      top: calc(100% + var(--spacing-2));
      right: 0;
      width: 14rem;
      z-index: 60;
      background: var(--color-surface-elevated);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-xl);
      padding: var(--spacing-2);
    }
    .profile-menu-header {
      padding: var(--spacing-2) var(--spacing-3) var(--spacing-3);
      border-bottom: 1px solid var(--color-border);
      margin-bottom: var(--spacing-1);
    }
    .profile-menu-name {
      color: var(--color-text-primary);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
    }
    .profile-menu-role {
      color: var(--color-text-secondary);
      font-size: var(--font-size-xs);
      margin-top: var(--spacing-1);
    }
    .profile-menu-item {
      width: 100%;
      display: flex;
      align-items: center;
      border: 0;
      background: transparent;
      color: var(--color-text-primary);
      cursor: pointer;
      border-radius: var(--radius-md);
      font-family: inherit;
      font-size: var(--font-size-sm);
      padding: var(--spacing-2) var(--spacing-3);
      text-align: left;
      transition: background var(--duration-fast) var(--ease-out), color var(--duration-fast) var(--ease-out);
    }
    .profile-menu-item:hover,
    .profile-menu-item:focus-visible {
      background: var(--color-neutral-light);
    }
    .profile-menu-item.danger {
      color: var(--color-danger);
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TopbarComponent {
  readonly themeService = inject(ThemeService);
  readonly demoInfo = inject(DemoInfoService);
  private readonly authService = inject(AuthService);
  private readonly host = inject(ElementRef<HTMLElement>);

  readonly profileMenuOpen = signal(false);
  readonly displayName = computed(() => this.authService.userDisplayName() ?? this.fallbackDisplayName());
  readonly roleLabel = computed(() => this.authService.roles()[0] ?? 'Signed in');
  readonly initials = computed(() => this.displayName().trim().slice(0, 1).toUpperCase() || 'A');

  @HostListener('document:click', ['$event.target'])
  onDocumentClick(target: EventTarget | null): void {
    if (target instanceof Node && !this.host.nativeElement.contains(target)) {
      this.profileMenuOpen.set(false);
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.profileMenuOpen.set(false);
  }

  toggleMobileMenu(): void {
  }

  toggleProfileMenu(): void {
    this.profileMenuOpen.update((open) => !open);
  }

  logout(): void {
    this.profileMenuOpen.set(false);
    this.authService.logout();
  }

  private fallbackDisplayName(): string {
    const roles = this.authService.roles();
    if (roles.includes('PlatformAdmin')) return 'Platform Admin';
    if (roles.includes('TenantAdmin')) return 'Admin';
    if (roles.includes('AttendantProfile')) return 'Porteiro';
    if (roles.includes('Morador')) return 'Morador';
    return 'Account';
  }
}
import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';

@Component({
  selector: 'ce-app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, SidebarComponent, TopbarComponent],
  template: `
    <div class="app-shell">
      <ce-sidebar />
      <div class="main-area">
        <ce-topbar />
        <main id="main" class="page-content" tabindex="-1">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: [`
    .app-shell {
      display: grid;
      grid-template-columns: var(--sidebar-width) 1fr;
      min-height: 100vh;
    }
    @media (max-width: 1023px) {
      .app-shell { grid-template-columns: var(--sidebar-width-collapsed) 1fr; }
    }
    @media (max-width: 639px) {
      .app-shell { grid-template-columns: 1fr; }
    }
    .main-area {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }
    .page-content {
      flex: 1;
      padding: var(--spacing-6);
      overflow-x: auto;
    }
    @media (max-width: 639px) {
      .page-content { padding: var(--spacing-4); }
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShellComponent {}
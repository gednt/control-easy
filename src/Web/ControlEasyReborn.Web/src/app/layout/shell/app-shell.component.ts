import { Component, ChangeDetectionStrategy, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';
import { DemoBannerComponent } from '../demo-banner/demo-banner.component';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'ce-app-shell',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, TopbarComponent, DemoBannerComponent],
  template: `
    <div class="app-shell">
      <ce-sidebar />
      <div class="main-area">
        <ce-demo-banner />
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
export class AppShellComponent implements OnInit {
  private readonly authService = inject(AuthService);

  ngOnInit(): void {
    this.authService.loadSession();
  }
}
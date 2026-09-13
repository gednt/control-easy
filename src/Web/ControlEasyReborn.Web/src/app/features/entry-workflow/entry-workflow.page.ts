import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CeEntryWorkflowComponent } from '../../design-system';
import { ToastService } from '../../design-system/components/toast/toast.component';

/**
 * Dedicated page for the `/gatehouse` route. Hosts the same workflow modal
 * used by the dashboard FAB, but pinned full-screen so porteiros using a
 * tablet can navigate directly without going through the dashboard.
 */
@Component({
  selector: 'ce-entry-workflow-page',
  standalone: true,
  imports: [CeEntryWorkflowComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ce-entry-workflow
      [open]="true"
      (closed)="goBack()"
      (entryLogged)="onLogged()"
    />
  `,
})
export class EntryWorkflowPage {
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  goBack(): void {
    void this.router.navigate(['/']);
  }

  onLogged(): void {
    this.toast.info('Entry recorded. Tap the button to start another.');
    // Stay on the page so the porteiro can log the next entry without
    // re-navigating; the modal has already reset via its own effect.
  }
}
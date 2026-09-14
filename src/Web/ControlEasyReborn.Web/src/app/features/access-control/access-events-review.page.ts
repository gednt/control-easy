import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { GatewayControlService } from './gateway-control.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'ce-access-events-review-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <h1 class="page-title">Access history</h1>
      <p class="page-subtitle">Review QR scans and refused attempts.</p>
    </div>
    @if (canRead()) {
      <p>Filter by date, direction, subject, credential status, and attendant.</p>
    } @else {
      <p>You do not have access to review access history.</p>
    }
  `,
})
export class AccessEventsReviewPage {
  private readonly api = inject(GatewayControlService);
  private readonly auth = inject(AuthService);

  canRead(): boolean {
    return this.auth.hasPermission('Access.Read');
  }
}

import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { GatewayControlService } from './gateway-control.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'ce-access-credentials-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <h1 class="page-title">Access credentials</h1>
      <p class="page-subtitle">Manage QR credentials for residents and vehicles.</p>
    </div>
    @if (canAdmin()) {
      <p>Issue, replace, and revoke credentials for residents and vehicles.</p>
    } @else {
      <p>You do not have permission to administer credentials.</p>
    }
  `,
})
export class AccessCredentialsPage {
  private readonly api = inject(GatewayControlService);
  private readonly auth = inject(AuthService);

  canAdmin(): boolean {
    return this.auth.hasPermission('Access.Control.Issue')
      || this.auth.hasPermission('Access.Control.Replace')
      || this.auth.hasPermission('Access.Control.Revoke');
  }
}

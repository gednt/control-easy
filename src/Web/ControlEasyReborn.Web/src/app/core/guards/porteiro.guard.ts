import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * Gatehouse workflow guard. Allows users with the AttendantProfile role
 * (porteiros) plus TenantAdmin so admins can also register entries during
 * demo/onboarding. PlatformAdmins are routed to their landing page.
 */
export const porteiroGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    void router.navigate(['/login']);
    return false;
  }

  const roles = authService.roles();
  if (
    roles.includes('AttendantProfile') ||
    roles.includes('TenantAdmin') ||
    roles.includes('PlatformAdmin')
  ) {
    return true;
  }

  void router.navigate(['/']);
  return false;
};
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { DemoInfoService } from '../services/demo-info.service';

export const demoModeGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const demoInfo = inject(DemoInfoService);
  const router = inject(Router);

  if (!auth.isAuthenticated() || !auth.isDemoPersona() || !demoInfo.enabled()) {
    return router.parseUrl('/');
  }

  return true;
};

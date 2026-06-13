import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { DemoInfoService } from '../services/demo-info.service';

export const demoModeGuard: CanActivateFn = () => {
  const demoInfo = inject(DemoInfoService);
  const router = inject(Router);

  if (!demoInfo.loaded()) {
    return router.parseUrl('/');
  }

  return demoInfo.enabled() ? true : router.parseUrl('/');
};

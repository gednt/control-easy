import { ApplicationConfig, APP_INITIALIZER, provideZoneChangeDetection, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { DemoInfoService, demoInfoInitializer } from './core/services/demo-info.service';
import { BootstrapInfoService, bootstrapInfoInitializer } from './core/services/bootstrap-info.service';
import { CE_LUCIDE_ICONS } from './design-system/components/icon/icon.registry';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    provideAnimationsAsync(),
    importProvidersFrom(CE_LUCIDE_ICONS),
    {
      provide: APP_INITIALIZER,
      useFactory: demoInfoInitializer,
      deps: [DemoInfoService],
      multi: true,
    },
    {
      provide: APP_INITIALIZER,
      useFactory: bootstrapInfoInitializer,
      deps: [BootstrapInfoService],
      multi: true,
    },
  ],
};
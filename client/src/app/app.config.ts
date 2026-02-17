import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { tenantInterceptor } from './core/interceptors/tenant.interceptor';

import { routes } from './app.routes';

import { environment } from '../environments/environment';
import { SalesService } from './modules/sales/services/sales.service';
import { RealSalesService } from './modules/sales/services/real-sales.service';
import { MockSalesService } from './modules/sales/services/mock-sales.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([tenantInterceptor])),

    // Conditional Service Provider
    {
      provide: SalesService,
      useClass: environment.mock ? MockSalesService : RealSalesService
    }
  ]
};

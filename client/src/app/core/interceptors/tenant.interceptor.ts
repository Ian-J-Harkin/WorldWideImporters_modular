import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { TenantService } from '../services/tenant.service';
import { throwError } from 'rxjs';

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
    const tenantService = inject(TenantService);
    let tenantId = tenantService.currentTenantId();

    if (!tenantId) {
        console.warn('⚠️ No Tenant Context. Injecting Fallback WWI Tenant.');
        tenantId = '8db1620a-8640-410a-8651-f0945934188b';
    }

    const authReq = req.clone({
        setHeaders: { 'X-Tenant-Id': tenantId }
    });

    return next(authReq);
};

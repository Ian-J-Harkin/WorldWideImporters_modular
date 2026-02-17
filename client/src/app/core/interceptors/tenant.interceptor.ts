import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { TenantService } from '../services/tenant.service';
import { throwError } from 'rxjs';

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
    const tenantService = inject(TenantService);
    const tenantId = tenantService.currentTenantId();

    if (!tenantId) {
        // Enforcement of Production Guardrail 1: No tenantless requests
        console.error('Tenant ID is required for all requests.');
        return throwError(() => new Error('Tenant ID is required.'));
    }

    const authReq = req.clone({
        setHeaders: { 'X-Tenant-Id': tenantId }
    });

    return next(authReq);
};

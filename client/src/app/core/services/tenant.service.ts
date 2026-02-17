import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TenantService {
  // Signal-based state for current tenant
  // Defaulted to a valid WWI sample Tenant Guid to prevent startup crashes
  private tenantId = signal<string | null>('8db1620a-8640-410a-8651-f0945934188b');
  currentTenantId = this.tenantId.asReadonly();

  setTenant(id: string) {
    this.tenantId.set(id);
  }
}

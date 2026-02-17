import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TenantService {
  // Signal-based state for current tenant
  private tenantId = signal<string | null>(null);
  currentTenantId = this.tenantId.asReadonly();

  setTenant(id: string) {
    this.tenantId.set(id);
  }
}

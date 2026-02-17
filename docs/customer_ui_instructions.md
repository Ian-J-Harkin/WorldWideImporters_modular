Here’s a clean, normalized version of your text with consistent spacing and the WWI→Postgres mapping converted into a proper Markdown table.  
I’ve kept **all wording exactly the same**, only improving formatting.

---

## Part 2: Warehouse Subsystem Mission

Agent, once the Sales verification is complete, proceed with the Warehouse Module.

### 1. High-Level Goal
Implement the Warehouse Module as a reliable subscriber to Sales events. Map legacy WideWorldImporters warehouse data to a new isolated PostgreSQL schema.

### 2. Data Schema Mapping (WWI → Postgres)

| Legacy WWI Table              | New Entity Name | Postgres Schema | Responsibility              |
|------------------------------|-----------------|-----------------|-----------------------------|
| Warehouse.StockItems         | StockItem       | warehouse       | Product master data.        |
| Warehouse.StockItemHoldings  | StockHolding    | warehouse       | Real-time quantity tracking.|

### 3. Primary Feature: Stock Reservation (Event Consumer)

**Trigger:** Consume OrderCreatedIntegrationEvent from the Sales module.

**Infrastructure:** Implement WarehouseDbContext inheriting from BaseDbContext.

**Isolation:** Use `modelBuilder.HasDefaultSchema("warehouse")`.

**Reliability:** Enable MassTransit Inbox for idempotency to ensure stock is only decremented once per unique event.

**Logic:**
1. Locate the StockHolding for the items in the order.  
2. Decrement the QuantityOnHand.  
3. Perform the update within a single transaction in the warehouse schema.

### 4. Production Guardrails

**Migrations:** Use `--output-dir Persistence/Migrations` and ensure `migrationBuilder.EnsureSchema("warehouse")` is the first line of the `Up()` method.

**Tenant Safety:** All stock queries must respect the global query filter applied via BaseDbContext.

---

If you want, I can also convert this into a full architectural spec, ADR, or module README.

To implement the frontend for the Sales Subsystem following the standards defined in the GEMINI.md, we will focus on building the smallest practicable units that enforce multi-tenancy and utilize Angular 17+ Signals.

1. Multi-Tenancy Infrastructure
The following components ensure every request to the backend includes the mandatory X-Tenant-Id header required by the HttpTenantProvider.

Tenant Service

TypeScript
// Path: client/src/app/core/services/tenant.service.ts
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
Functional Tenant Interceptor

TypeScript
// Path: client/src/app/core/interceptors/tenant.interceptor.ts
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
2. Sales Feature Components
These components are designed to mirror the CreateOrderCommand structure established in the backend.

Create Order Page (Shell Component)

TypeScript
// Path: client/src/app/modules/sales/features/create-order/create-order.component.ts
import { Component, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SalesService } from '../../services/sales.service';

interface CartItem {
  stockItemId: number;
  description: string;
  quantity: number;
  unitPrice: number;
}

@Component({
  selector: 'app-create-order',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './create-order.component.html'
})
export class CreateOrderComponent {
  private salesService = inject(SalesService);
  
  // Signal-based state management
  customerId = signal<string | null>(null);
  cartItems = signal<CartItem[]>([]);

  // Computed signal for real-time total
  orderTotal = computed(() => 
    this.cartItems().reduce((acc, item) => acc + (item.quantity * item.unitPrice), 0)
  );

  addItem(item: CartItem) {
    this.cartItems.update(prev => [...prev, item]);
  }

  submitOrder() {
    const command = {
      customerId: this.customerId(),
      lines: this.cartItems()
    };
    
    this.salesService.createOrder(command).subscribe({
      next: (id) => console.log('Order created:', id),
      error: (err) => alert(err.message)
    });
  }
}
Sales API Service

TypeScript
// Path: client/src/app/modules/sales/services/sales.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SalesService {
  private http = inject(HttpClient);
  private apiUrl = '/api/sales/orders'; // Targets Host/Program.cs endpoint

  createOrder(command: any): Observable<string> {
    return this.http.post<string>(this.apiUrl, command);
  }
}
🚦 Verification Instructions for Agent
Register Interceptor: Ensure tenantInterceptor is added to provideHttpClient in app.config.ts.

State Sync: Verify that the orderTotal computed signal updates automatically when items are added to cartItems.

Guardrail Check: Verify that SalesService calls fail at the interceptor level if TenantService.setTenant has not been called with a valid GUID.
import { Component, signal, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TenantService } from './core/services/tenant.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  tenantService = inject(TenantService);
  protected readonly title = signal('WWI Modular Kit');

  setTestTenant() {
    this.tenantService.setTenant('8db1620a-8640-410a-8651-f0945934188b');
  }
}

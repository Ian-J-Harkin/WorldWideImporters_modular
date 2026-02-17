import { Component, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SalesService } from '../../services/sales.service';
import { TenantService } from '../../../../core/services/tenant.service';

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
    public tenantService = inject(TenantService);

    // Signal-based state management
    customerId = signal<string | null>(null);
    customerIdTouched = signal(false);
    cartItems = signal<CartItem[]>([]);

    // Computed validation signals
    isCustomerValid = computed(() => !!this.customerId()?.trim());
    isCartValid = computed(() => this.cartItems().length > 0);
    canSubmit = computed(() => this.isCustomerValid() && this.isCartValid());

    // Computed signal for real-time total
    orderTotal = computed(() =>
        this.cartItems().reduce((acc, item) => acc + (item.quantity * item.unitPrice), 0)
    );

    markCustomerTouched() {
        this.customerIdTouched.set(true);
    }

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

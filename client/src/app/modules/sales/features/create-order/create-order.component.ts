import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SalesService, Customer, Product } from '../../services/sales.service';
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
export class CreateOrderComponent implements OnInit {
    private salesService = inject(SalesService);
    public tenantService = inject(TenantService);

    // Data Signals
    customers = signal<Customer[]>([]);
    products = signal<Product[]>([]);

    // Form Signals
    selectedCustomerId = signal<string>("");
    cartItems = signal<CartItem[]>([]);

    // UI Feedback Signals
    submissionStatus = signal<'idle' | 'submitting' | 'success' | 'error'>('idle');
    lastOrderId = signal<string | null>(null);

    // Computed
    orderTotal = computed(() =>
        this.cartItems().reduce((acc, item) => acc + (item.quantity * item.unitPrice), 0)
    );

    canSubmit = computed(() =>
        !!this.selectedCustomerId() &&
        this.cartItems().length > 0 &&
        this.submissionStatus() !== 'submitting'
    );

    ngOnInit() {
        // Load Mock/Real Data
        this.salesService.getCustomers().subscribe(data => this.customers.set(data));
        this.salesService.getProducts().subscribe(data => this.products.set(data));
    }

    addToCart(product: Product) {
        this.cartItems.update(prev => {
            const existing = prev.find(p => p.stockItemId === product.stockItemId);
            if (existing) {
                // Increment quantity
                return prev.map(p => p.stockItemId === product.stockItemId
                    ? { ...p, quantity: p.quantity + 1 }
                    : p);
            } else {
                // Add new item
                return [...prev, {
                    stockItemId: product.stockItemId,
                    description: product.description,
                    unitPrice: product.unitPrice,
                    quantity: 1
                }];
            }
        });
    }

    removeFromCart(stockItemId: number) {
        this.cartItems.update(prev => prev.filter(p => p.stockItemId !== stockItemId));
    }

    submitOrder() {
        if (!this.canSubmit()) return;

        this.submissionStatus.set('submitting');

        const command = {
            customerId: this.selectedCustomerId(),
            lines: this.cartItems()
        };

        this.salesService.createOrder(command).subscribe({
            next: (id) => {
                this.lastOrderId.set(id);
                this.submissionStatus.set('success');

                // Reset Form
                this.cartItems.set([]);
                this.selectedCustomerId.set("");

                setTimeout(() => this.submissionStatus.set('idle'), 5000);
            },
            error: (err) => {
                console.error(err);
                this.submissionStatus.set('error');
            }
        });
    }

    // Helper for template binding
    updateSelectedCustomer(event: Event) {
        const value = (event.target as HTMLSelectElement).value;
        this.selectedCustomerId.set(value);
    }
}

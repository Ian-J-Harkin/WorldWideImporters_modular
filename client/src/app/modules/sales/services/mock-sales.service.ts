import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { SalesService, Customer, Product } from './sales.service';

@Injectable()
export class MockSalesService extends SalesService {

    getCustomers(): Observable<Customer[]> {
        return of([
            { id: '1001-mock-guid', name: 'Tailspin Toys (Head Office)' },
            { id: '1002-mock-guid', name: 'Wingtip Toys (Head Office)' },
            { id: '1003-mock-guid', name: 'Contoso Ltd' },
            { id: '1004-mock-guid', name: 'Wide World Importers' },
            { id: '1005-mock-guid', name: 'Northwind Traders' }
        ]).pipe(delay(500));
    }

    getProducts(): Observable<Product[]> {
        return of([
            { stockItemId: 220, description: 'USB Missile Launcher (Green)', unitPrice: 25.00 },
            { stockItemId: 150, description: 'Hollow Ride-on Whale (Blue)', unitPrice: 100.00 },
            { stockItemId: 226, description: 'USB Food Flash Drive - Sushi', unitPrice: 15.00 },
            { stockItemId: 227, description: 'USB Food Flash Drive - Hamburger', unitPrice: 15.00 },
            { stockItemId: 228, description: 'USB Food Flash Drive - Hot Dog', unitPrice: 15.00 }
        ]).pipe(delay(500));
    }

    createOrder(command: any): Observable<string> {
        console.log('[MOCK] Order Created:', command);
        // Return a fake GUID
        return of('mock-order-' + Math.floor(Math.random() * 10000)).pipe(delay(1000));
    }
}

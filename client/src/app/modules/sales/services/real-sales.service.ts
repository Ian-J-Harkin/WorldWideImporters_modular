import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { SalesService, Customer, Product } from './sales.service';

@Injectable()
export class RealSalesService extends SalesService {
    private http = inject(HttpClient);
    private apiUrl = '/api/sales/orders';

    createOrder(command: any): Observable<string> {
        return this.http.post<string>(this.apiUrl, command);
    }

    getCustomers(): Observable<Customer[]> {
        // Backend endpoint doesn't exist yet, return empty
        return of([]);
    }

    getProducts(): Observable<Product[]> {
        // Backend endpoint doesn't exist yet, return empty
        return of([]);
    }
}

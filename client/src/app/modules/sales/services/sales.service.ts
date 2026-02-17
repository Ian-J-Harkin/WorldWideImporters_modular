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

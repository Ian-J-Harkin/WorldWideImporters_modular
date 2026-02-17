import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Customer {
    id: string;
    name: string;
}

export interface Product {
    stockItemId: number;
    description: string;
    unitPrice: number;
}

export abstract class SalesService {
    abstract createOrder(command: any): Observable<string>;
    abstract getCustomers(): Observable<Customer[]>;
    abstract getProducts(): Observable<Product[]>;
}

import { Routes } from '@angular/router';
import { CreateOrderComponent } from './modules/sales/features/create-order/create-order.component';

export const routes: Routes = [
    { path: 'sales/create-order', component: CreateOrderComponent },
    { path: '', redirectTo: 'sales/create-order', pathMatch: 'full' }
];

# Technical Architecture: WWI Modular Kit

## 1. Data Mapping (WideWorldImporters -> Postgres)
Every module owns its PostgreSQL schema. No cross-schema joins allowed.

| WWI Legacy Table | New Module | Schema | Entity |
| :--- | :--- | :--- | :--- |
| Sales.Orders | **Sales** | `sales` | `Order` |
| Sales.OrderLines | **Sales** | `sales` | `OrderLine` |
| Sales.Customers | **Sales** | `sales` | `Customer` |
| Warehouse.StockItems | **Warehouse** | `warehouse` | `StockItem` |

## 2. Sales Subsystem Design
The Sales module implements a "Transactional Outbox" to maintain reliability during high-volume order processing.

### CreateOrder Workflow (VSA)
- **Path:** `src/Modules/Sales/Implementation/Features/Orders/CreateOrder/`
- **Action:** Saves `Order` to `sales.Orders` + Saves `Event` to `sales.OutboxState`.
- **Reliability:** MassTransit polls the Outbox and pushes to RabbitMQ asynchronously.

```mermaid
graph TD
    subgraph Sales_Module
        A[CreateOrderEndpoint] --> B[CreateOrderHandler]
        B --> C[(Postgres: sales schema)]
        C --> D[MassTransit Outbox]
    end
    D --> E{RabbitMQ Bus}
    E --> F[Warehouse_Module_Consumer]
```

## 3. Frontend Standards
- **Framework:** Angular 17+ (Standalone components).
- **Styling:** Tailwind CSS. 
    - > [!IMPORTANT]
    - > **Tailwind version must not exceed v3.4.** 
    - > Version 4.x has been tested and found incompatible with the current Angular build system and PostCSS pipeline in this environment. Do not upgrade without a verified, dedicated compatibility spike.
- **State Management:** Angular Signals for local component state and computed values.
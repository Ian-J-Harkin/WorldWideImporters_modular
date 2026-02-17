Part 2: Warehouse Subsystem Mission
Agent, once the Sales verification is complete, proceed with the Warehouse Module.

1. High-Level Goal
Implement the Warehouse Module as a reliable subscriber to Sales events. Map legacy WideWorldImporters warehouse data to a new isolated PostgreSQL schema.

2. Data Schema Mapping (WWI -> Postgres)
Legacy WWI Table	New Entity Name	Postgres Schema	Responsibility
Warehouse.StockItems	StockItem	warehouse	Product master data.
Warehouse.StockItemHoldings	StockHolding	warehouse	Real-time quantity tracking.
3. Primary Feature: Stock Reservation (Event Consumer)
Trigger: Consume OrderCreatedIntegrationEvent from the Sales module.

Infrastructure: Implement WarehouseDbContext inheriting from BaseDbContext.

Isolation: Use modelBuilder.HasDefaultSchema("warehouse").

Reliability: Enable MassTransit Inbox for idempotency to ensure stock is only decremented once per unique event.

Logic: 1.  Locate the StockHolding for the items in the order.
2.  Decrement the QuantityOnHand.
3.  Perform the update within a single transaction in the warehouse schema.

4. Production Guardrails
Migrations: Use --output-dir Persistence/Migrations and ensure migrationBuilder.EnsureSchema("warehouse") is the first line of the Up() method.

Tenant Safety: All stock queries must respect the global query filter applied via BaseDbContext.
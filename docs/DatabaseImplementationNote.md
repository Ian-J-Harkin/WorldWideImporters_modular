# Database Implementation Note

This document outlines the architecture, configuration, and security guardrails for the PostgreSQL database within the WWI Modular Monolith project.

## 🏗️ Architecture: Schema-per-Module
To ensure strict module isolation, each project module (e.g., Sales, Warehouse) owns its own schema within the shared PostgreSQL database.

- **Rule**: Cross-module joins are strictly forbidden at the database level.
- **Communication**: Inter-module data exchange occurs solely via Integration Events (MassTransit/RabbitMQ).

## 🐳 Infrastructure
### Local Development
Managed via `docker-compose.yml` at the project root.
- **Image**: `postgres:latest`
- **User/Pass**: `wwi_admin` / `wwi_password`
- **Database**: `wwi_modular_monolith`
- **Port**: `5432`

### Integration Testing
Managed via **Testcontainers** in `WWI_ModularKit.IntegrationTests`.
- Spins up a transient PostgreSQL container per test session.
- Ensures zero side effects and a clean state for every run.
- Automatically applies migrations before execution.

## 🧱 Implementation Details
### 1. Base Infrastructure (`BuildingBlocks`)
The [BaseDbContext.cs](file:///c:/Github/WWI_ModularKit/src/BuildingBlocks/Persistence/BaseDbContext.cs) provides the core logic:
- **Strict Multi-Tenancy**: Automatically applies a Global Query Filter to all entities inheriting from `BaseEntity`.
- **Auto-Stamping**: Overrides `SaveChangesAsync` to inject the current `TenantId` from the `ITenantProvider` into all new records.
- **Transactional Outbox**: Integrated with MassTransit to ensure business data and integration events are committed atomicaly.

### 2. Module Persistence (`Modules/[Module]`)
Each module defines its specific context, e.g., [SalesDbContext.cs](file:///c:/Github/WWI_ModularKit/src/Modules/Sales/Persistence/SalesDbContext.cs):
- **Schema Mapping**: Explicitly sets the schema using `modelBuilder.HasDefaultSchema("sales")`.
- **Design-Time Support**: Includes a `SalesDbContextFactory` to enable `dotnet ef migrations` commands from the module directory.

## 🛡️ Security Guardrails
1. **Tenant Enforcement**: The `ITenantProvider` is designed to throw a `BadHttpRequestException` if the `X-Tenant-Id` header is missing, preventing "orphan" data leaks.
2. **Schema-First Migrations**: All migrations must be local to the module (`Persistence/Migrations`) and must explicitly ensure their own schema exists in the `Up()` method.

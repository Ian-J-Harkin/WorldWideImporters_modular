# Project Constitution: WWI Modular Monolith Kit

## 🎯 High-Level Goal
Build a production-grade .NET 8/9 Modular Monolith using WideWorldImporters (WWI) data. This is a "Starter Kit" factory designed for high reliability, multi-tenancy, and automated scaffolding.

## 🏗️ Architectural Pillars (DO NOT DEVIATE)
1. **Vertical Slice Architecture (VSA):** Code is grouped by feature (e.g., `CreateOrder`), not by layer. Keep Commands, Handlers, and Validators in the same folder.
2. **Module Isolation (Schema-per-Module):** Each module (Sales, Warehouse, etc.) MUST have its own `DbContext` and own its unique PostgreSQL Schema. 
   - **Rule:** Cross-module Joins in C# are strictly forbidden. 
   - **Rule:** Cross-module communication must happen via Integration Events.
3. **Transactional Outbox:** Use MassTransit + RabbitMQ. All messages must be persisted to the module's own Outbox table within the same DB transaction as the business data.
4. **Multi-Tenancy:** Every entity inheriting from `BaseEntity` must include a `TenantId`. The `BaseDbContext` enforces global query filters.

## 📁 Directory Standards
- `src/BuildingBlocks`: Global infrastructure. No business logic here.
- `src/Modules/[Module]`: 
    - `/Contracts`: Public DTOs and Integration Events (NuGet-ready).
    - `/Features`: Internal logic grouped by vertical slice. All module-specific entities live in `/Entities`.
- `client`: Angular 17+ standalone components using Signals.

## 🛠️ Tech Stack & Conventions
- **Backend:** .NET 8/9, EF Core (Npgsql), MediatR, MassTransit, FluentValidation.
- **Database:** PostgreSQL (Development via Docker Compose).
- **Frontend:** Angular (Standalone, Tailwind CSS).
- **Coding Style:** File-scoped namespaces, Primary Constructors (C# 12+), `internal` by default for implementation classes.

## �️ Production Guardrails
1. **Strict Tenant Enforcement (No "Orphan" Data):** The `ITenantProvider` must throw a `BadHttpRequestException("Tenant ID is required.")` if the `X-Tenant-Id` header is missing. No data can be created or queried without a valid tenant context.
2. **Schema-First Migration Pattern:** 
   - Always use `--output-dir Persistence/Migrations` to keep migrations local to the module.
   - The first line of the `Up()` method must be `migrationBuilder.EnsureSchema("module_name");`.
   - All tables must be explicitly assigned to that schema to ensure logical partitioning.

## �🚀 First Mission for Agent
1. Index `src/BuildingBlocks/Infrastructure/Persistence/BaseDbContext.cs`.
2. Map the legacy WWI `Sales` schema to the new `src/Modules/Sales` module.
3. Implement the `CreateOrder` feature slice using the MassTransit Outbox pattern.
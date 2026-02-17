In its current state, the programming agent knows about the WideWorldImporters (WWI) schema because it is a very famous and well-documented sample. Large language models (LLMs) have been trained on Microsoft's official documentation and GitHub repositories that contain the exact table names, field types, and relationships (like Sales.Orders and Warehouse.StockItemHoldings).

However, to move from "generic knowledge" to "perfect implementation," providing the schema details directly in the project will eliminate any hallucinations and ensure the agent maps the legacy fields exactly as they appear in the source.

1. How the Agent knows the schema now
Training Data: The agent knows the standard WWI OLTP schema (e.g., Sales.Customers, Warehouse.StockItems, Application.People).

Your ARCHITECTURE.md: You have already provided a mapping table that tells the agent which legacy tables go into which new PostgreSQL schemas.

Vertical Slice Logic: By defining the CreateOrder feature, you've implicitly confirmed the existence of fields like StockItemID and Quantity.

2. How to "Feed" the Agent the Full Schema
You don't necessarily need to download and restore the full .bak database file (which can be over 1GB). Instead, you can provide the SQL DDL (Data Definition Language) scripts.

The Best Way to provide context:
Create a folder named /docs/legacy_schema/ and put .sql files there containing the CREATE TABLE statements for the WWI schemas. You can find these in the official Microsoft WWI GitHub repository.

Suggested files to include for your current mission:

Sales_Tables.sql (Contains Orders, OrderLines, Customers)

Warehouse_Tables.sql (Contains StockItems, StockItemHoldings)

Application_Tables.sql (Contains People, Cities, Countries)

3. Benefits of providing the Scripts directly:
Precise Types: The agent will know that StockItemHoldings.QuantityOnHand is an INT and LastCostPrice is a DECIMAL(18, 2).

Temporal Tables: WWI uses "Temporal Tables" (System-versioned). If the agent sees the DDL, it can decide whether to implement similar versioning in Postgres or stick to simple auditing.

No Guesswork: The agent won't have to guess which fields are nullable or which have default values (like SYSDATETIME()).

Recommendation for your next step:
If you want to be 100% sure, simply copy the CREATE TABLE scripts for the relevant schemas from the WWI repository and paste them into a new file in your docs folder. Then tell the agent:

"I have added the legacy WWI SQL scripts to docs/legacy_schema/. Please refer to these for the exact field names and types when implementing the Warehouse entities."

Exploring the WideWorldImporters OLTP database catalog
This documentation is the definitive guide to the tables and relationships in the WWI database, providing the exact "blueprint" the agent needs for accurate data mapping.
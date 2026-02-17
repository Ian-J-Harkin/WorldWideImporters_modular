INSERT INTO sales."Customers" ("Id", "Name", "CustomerCategoryName", "PrimaryContact", "PhoneNumber", "WebsiteURL", "DeliveryAddressLine1", "DeliveryPostalCode", "TenantId") 
VALUES ('682f8664-9640-410a-8651-f0945934188b', 'Tailspin Toys', 'Novelty Shop', 'Waldemar Farian', '(308) 555-0100', 'http://www.tailspintoys.com', 'Shop 38, 4247 E No Name St', '90410', '8db1620a-8640-410a-8651-f0945934188b') 
ON CONFLICT DO NOTHING;

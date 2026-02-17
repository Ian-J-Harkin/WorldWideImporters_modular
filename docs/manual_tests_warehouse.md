# Manual Test Suite: Warehouse Subsystem

## Prerequisites
- **Backend Running**: `dotnet run --project src/Host`
- **Frontend Running**: `npm start`
- **Configuration**: Ensure `UseInMemoryDatabase: true` in `appsettings.json`.

## Test Scenarios

### Scenario 1: Happy Path (Stock Reservation)
**Goal**: Verify that an order for an in-stock item triggers a reservation and confirms the order.

| Step | Action | Expected Outcome |
| :--- | :--- | :--- |
| 1 | Navigate to "Create Order" page. | Page loads with Mock Data. |
| 2 | Select Customer: **Tailspin Toys**. | Customer selected. |
| 3 | Add Product: **USB Missile Launcher** (ID 220). | Item added to cart. Price: $25.00. |
| 4 | Set Quantity: **5**. | Total Price: $125.00. |
| 5 | Click **"Submit Order"**. | Success Alert with Order ID. |
| 6 | Check Console Logs (Backend). | See Sequence below. |

**Expected Log Sequence:**
1. `[Sales] Order {Id} Created`
2. `[Warehouse] Processing Order...`
3. `[Warehouse] Stock Reserved. New Qty: 5` (10 - 5)
4. `[Sales] Stock Reserved for Order {Id}. Updating Status...`
5. `[Sales] Order {Id} Status -> Confirmed ✅`

---

### Scenario 2: Insufficient Stock
**Goal**: Verify that ordering more than available stock triggers a failure event (Note: Handling logic for failure is TBD in Sales, but Warehouse must publish failure).

| Step | Action | Expected Outcome |
| :--- | :--- | :--- |
| 1 | Restart Backend (to reset stock to 10). | Stock reset. |
| 2 | Create Order for **15** USB Missile Launchers. | Order Submitted. |
| 3 | Check Console Logs. | See Sequence below. |

**Expected Log Sequence:**
1. `[Warehouse] Insufficient Stock. Requested: 15, Available: 10`
2. `[Warehouse] Publishing StockInsufficientIntegrationEvent...` (If implemented)

## Troubleshooting
- **No Logs?**: Ensure `UseInMemoryDatabase` is `true`.
- **Wrong Qty?**: Restart backend to reset the In-Memory mock data.

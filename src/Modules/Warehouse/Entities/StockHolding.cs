using WWI_ModularKit.BuildingBlocks.Abstractions;

namespace WWI_ModularKit.Modules.Warehouse.Entities;

public class StockHolding : BaseEntity
{
    public int StockItemId { get; set; }
    public int QuantityOnHand { get; set; }

    public StockItem? StockItem { get; set; }
}

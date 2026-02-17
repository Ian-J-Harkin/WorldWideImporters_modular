using WWI_ModularKit.BuildingBlocks.Abstractions;

namespace WWI_ModularKit.Modules.Warehouse.Entities;

public class StockItem
{
    // Matches the WWI database StockItemId (int)
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

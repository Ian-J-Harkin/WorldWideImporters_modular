using WWI_ModularKit.BuildingBlocks.Abstractions;

namespace WWI_ModularKit.Modules.Sales.Entities;

public class OrderLine : BaseEntity
{
    public Guid OrderId { get; set; }
    public int StockItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int PickedQuantity { get; set; }
    public DateTime? PickingCompletedWhen { get; set; }
}

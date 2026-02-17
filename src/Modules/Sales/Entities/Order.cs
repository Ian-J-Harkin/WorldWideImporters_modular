using WWI_ModularKit.BuildingBlocks.Abstractions;

namespace WWI_ModularKit.Modules.Sales.Entities;

public class Order : BaseEntity
{
    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public string? CustomerPurchaseOrderNumber { get; set; }
    public bool IsUndeliverable { get; set; }
    public DateTime? PickingCompletedWhen { get; set; }
    
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public string Status { get; set; } = "Pending";

    public List<OrderLine> Lines { get; set; } = new();
}

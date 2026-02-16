using WWI_ModularKit.BuildingBlocks.Abstractions;

namespace WWI_ModularKit.Modules.Sales.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string CustomerCategoryName { get; set; } = string.Empty;
    public string PrimaryContact { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string WebsiteURL { get; set; } = string.Empty;
    public string DeliveryAddressLine1 { get; set; } = string.Empty;
    public string DeliveryPostalCode { get; set; } = string.Empty;
}

namespace ShipmentBookingSystem.Domain.Models;

public sealed class ShipmentSummary
{
    public int CustomerID { get; set; }
    public int ShipmentsCount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ProductSummary> Products { get; set; }
}
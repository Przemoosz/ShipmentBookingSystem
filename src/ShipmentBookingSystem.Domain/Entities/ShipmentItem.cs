namespace ShipmentBookingSystem.Domain.Entities;

public sealed class ShipmentItem
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid ShipmentId { get; set; }
	public string ProductCode { get; set; }
	public int Quantity { get; set; }
	public decimal UnitPrice { get; set; }
}
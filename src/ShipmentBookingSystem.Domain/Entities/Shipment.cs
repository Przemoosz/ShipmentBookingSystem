namespace ShipmentBookingSystem.Domain.Entities;

public sealed class Shipment
{
	public Guid Id { get; set; }
	public string ShipmentNumber { get; set; }
	public int CustomerId { get; set; }
	public DateTime CreatedAt { get; set; }
}
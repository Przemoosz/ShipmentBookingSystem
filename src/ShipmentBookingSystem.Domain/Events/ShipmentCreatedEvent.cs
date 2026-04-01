namespace ShipmentBookingSystem.Domain.Events;

public sealed record ShipmentCreatedEvent
{
	public Guid EventId { get; init; }
	public Guid ShipmentId { get; init; }
	public string ShipmentNumber { get; init; }
	public int CustomerId { get; init; }
	public decimal TotalAmount { get; init; }
	public DateTime OccurredAt { get; init; }
}
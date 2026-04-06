namespace ShipmentBookingSystem.Infrastructure.OutboxEvent;

public record OutboxEntity
{
	public Guid Id { get; init; }
	public string EventType { get; init; }
	public string Payload { get; init; }
	public DateTime CreatedAt { get; init; }
	public DateTime? ProcessedAt { get; init; }
	public int Attempts { get; init; }
	public string? LastError { get; init; }
	public bool IsProcessed { get; init; }
}

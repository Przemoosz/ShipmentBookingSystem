using System.Data;

namespace ShipmentBookingSystem.Application.Interfaces;

public interface IOutboxRepository
{
	Task SaveEventAsync(Guid id, string eventType, object eventPayload, IDbTransaction transaction);
	Task<IEnumerable<(Guid, string)>> GetPendingEventsAsync(CancellationToken ct);
	Task MarkEventAsFinishedAsync(Guid eventId);
	Task MarkEventAsFailedAsync(Guid eventId, string errorMessage);
}
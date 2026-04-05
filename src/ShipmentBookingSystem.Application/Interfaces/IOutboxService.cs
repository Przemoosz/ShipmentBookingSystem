using System.Data;
using ShipmentBookingSystem.Domain.Events;

namespace ShipmentBookingSystem.Application.Interfaces;

public interface IOutboxService
{
	Task SaveEventAsync(Guid id, string eventType, object eventPayload, IDbConnection connection, IDbTransaction transaction);
	Task<IEnumerable<(Guid, string)>> GetPendingEventsAsync(IDbConnection connection, CancellationToken ct);
	Task SaveEventAsFinishedAsync(Guid eventId, IDbConnection connection, IDbTransaction transaction);
	Task SaveEventAsFailedAsync(Guid eventId, string errorMessage, IDbConnection connection, IDbTransaction transaction);
}
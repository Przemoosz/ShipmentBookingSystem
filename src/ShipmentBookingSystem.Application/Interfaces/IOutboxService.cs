using System.Data;

namespace ShipmentBookingSystem.Application.Interfaces;

public interface IOutboxService
{
	Task SaveEventAsync(string eventType, object eventPayload, IDbConnection connection, IDbTransaction transaction);
}
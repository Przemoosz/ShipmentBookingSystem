using System.Data;
using ShipmentBookingSystem.Domain.Entities;

namespace ShipmentBookingSystem.Application.Interfaces;

public interface IShipmentRepository
{
	Task SaveAsync(Shipment shipment, IEnumerable<ShipmentItem> items, IDbTransaction transaction,
		CancellationToken ct);
}
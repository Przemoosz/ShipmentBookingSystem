using ShipmentBookingSystem.Domain.Entities;

namespace ShipmentBookingSystem.Application.Interfaces;

public interface IShipmentRepository
{
	Task AddAsync(Shipment shipment, IEnumerable<ShipmentItem> items);
}
using System.Data;
using ShipmentBookingSystem.Domain.Entities;
using ShipmentBookingSystem.Domain.Models;

namespace ShipmentBookingSystem.Application.Interfaces;

public interface IShipmentRepository
{
	Task AddShipmentAsync(Shipment shipment, CancellationToken ct);
    Task AddShipmentItemsAsync(IEnumerable<ShipmentItem> shipmentItems, CancellationToken ct);
    Task<ShipmentSummary> GetSummaryAsync(int customerId, DateTime createdFrom,
        DateTime createdTo, int minTotalAmount, int minShipments);
}
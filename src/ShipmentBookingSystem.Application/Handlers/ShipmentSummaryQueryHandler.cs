using ShipmentBookingSystem.Application.Interfaces;
using ShipmentBookingSystem.Application.Queries;
using ShipmentBookingSystem.Domain.Models;

namespace ShipmentBookingSystem.Application.Handlers;

public sealed class ShipmentSummaryQueryHandler
{
    private IShipmentRepository _shipmentRepository;

    public ShipmentSummaryQueryHandler(IUnitOfWork unitOfWork)
    {
        _shipmentRepository = unitOfWork.ShipmentRepository;
    }
    public async Task<ShipmentSummary> Handle(ShipmentSummaryQuery query, CancellationToken cancellationToken)
    {
        ShipmentSummary result = await _shipmentRepository.GetSummaryAsync(query.CustomerId, query.CreatedFrom, query.CreatedTo, query.MinTotalAmount, query.MinShipments);
        return result;
    }
}

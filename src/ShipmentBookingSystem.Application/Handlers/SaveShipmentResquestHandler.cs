using Microsoft.Extensions.Logging;
using ShipmentBookingSystem.Application.Interfaces;
using ShipmentBookingSystem.Application.Requests;
using ShipmentBookingSystem.Domain.Entities;
using ShipmentBookingSystem.Domain.Events;
using Wolverine;

namespace ShipmentBookingSystem.Application.Handlers;

public sealed class SaveShipmentResquestHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMessageBus _messageBus;
    private readonly ILogger<SaveShipmentResquestHandler> _logger;

    public SaveShipmentResquestHandler(IUnitOfWork unitOfWork, IMessageBus messageBus, ILogger<SaveShipmentResquestHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _messageBus = messageBus;
        _logger = logger;
    }

    public async Task Handle(SaveShipmentRequest request, CancellationToken ct)
    {
        Guid shipmentId = Guid.NewGuid();
        DateTime shipmentCreationDate = DateTime.UtcNow;
        Shipment shipment = new()
        {
            CreatedAt = shipmentCreationDate,
            CustomerId = request.CustomerId,
            Id = shipmentId,
            ShipmentNumber = request.ShipmentNumber,
        };
        List<ShipmentItem> shipmentItems = new(request.Items.Count);
        decimal totalAmount = 0;
        foreach (var item in request.Items) 
        {
            shipmentItems.Add(new ShipmentItem()
            {
                ShipmentId = shipment.Id,
                ProductCode = item.ProductCode,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
            });
            totalAmount += (item.Quantity * item.UnitPrice);
        }

        ShipmentCreatedEvent shipmentCreatedEvent = new ()
        {
            EventId = Guid.NewGuid(),
            ShipmentId = shipment.Id,
            ShipmentNumber = shipment.ShipmentNumber,
            CustomerId = shipment.CustomerId,
            TotalAmount = totalAmount,
            OccurredAt = shipmentCreationDate
        };

        try
        {
            await _unitOfWork.ShipmentRepository.AddShipmentAsync(shipment, ct);
            await _unitOfWork.ShipmentRepository.AddShipmentItemsAsync(shipmentItems, ct);

            await _messageBus.PublishAsync(shipmentCreatedEvent);

            _unitOfWork.SaveChanges();
        }
        catch (Exception ex)
        {
            _unitOfWork.RollbackChanges();
            _logger.LogError("Failed to save and publish shipment information", ex);
        }
    }

}


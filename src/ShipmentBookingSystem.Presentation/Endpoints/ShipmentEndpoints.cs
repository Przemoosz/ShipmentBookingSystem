using System.Data;
using Dapper;
using Microsoft.AspNetCore.Http;
using ShipmentBookingSystem.Application.Interfaces;
using ShipmentBookingSystem.Application.Validators;
using ShipmentBookingSystem.Domain.Entities;
using ShipmentBookingSystem.Domain.Events;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Http;
using Wolverine.Persistence.Durability;

namespace ShipmentBookingSystem.Presentation.Endpoints;

public static class ShipmentEndpoints
{
	[WolverinePost("/shipments")]
	public static async Task<IResult> Post(CreateShipment request,
			IShipmentRepository shipmentRepository,
			IDbConnection connection, IOutboxService outboxService, IMessageBus bus)
	{
		using var transaction = connection.BeginTransaction();
		
		Shipment shipment = new Shipment()
		{
			CreatedAt = DateTime.UtcNow,
			CustomerId = request.CustomerId,
			Id = Guid.NewGuid(),
			ShipmentNumber = request.ShipmentNumber,
		};
		var shipmentsItems = request.Items.Select(dto => new ShipmentItem()
		{
			Id = Guid.NewGuid(),
			ShipmentId = shipment.Id,
			ProductCode = dto.ProductCode,
			Quantity = dto.Quantity,
			UnitPrice = dto.UnitPrice,
		}).ToList();

		try
		{
			const string sqlShipment = "INSERT INTO Shipments (Id, ShipmentNumber, CustomerId, CreatedAt) VALUES (@Id, @ShipmentNumber, @CustomerId, @CreatedAt)";
			await connection.ExecuteAsync(new CommandDefinition(sqlShipment, shipment, cancellationToken: CancellationToken.None, transaction: transaction));
			const string sqlItem = "INSERT INTO ShipmentItems (Id, ShipmentId, ProductCode, Quantity, UnitPrice) VALUES (@Id, @ShipmentId, @ProductCode, @Quantity, @UnitPrice)";
			await connection.ExecuteAsync(new CommandDefinition(sqlItem, shipmentsItems, cancellationToken: CancellationToken.None, transaction: transaction));
			
			var totalAmount = shipmentsItems.Sum(x => x.Quantity * x.UnitPrice);

			var shipmentCreatedEvent = new ShipmentCreatedEvent()
			{
				EventId = Guid.NewGuid(),
				ShipmentId = shipment.Id,
				ShipmentNumber = shipment.ShipmentNumber,
				CustomerId = shipment.CustomerId,
				TotalAmount = totalAmount,
				OccurredAt = DateTime.UtcNow
			};
			await outboxService.SaveEventAsync(
				shipmentCreatedEvent.EventId,
				nameof(ShipmentCreatedEvent),
				shipmentCreatedEvent,
				connection, transaction);
			transaction.Commit();
			// await bus.PublishAsync(shipmentCreatedEvent);
		}
		catch
		{
			transaction.Rollback();
			throw;
		}
		return Results.Created();
	}
}
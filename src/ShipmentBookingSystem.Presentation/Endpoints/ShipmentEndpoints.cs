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

namespace ShipmentBookingSystem.Presentation.Endpoints;

public static class ShipmentEndpoints
{
	[WolverinePost("/shipments")]
	// [Transactional] 
	public static async Task<IResult> Post(CreateShipment request,
			IShipmentRepository shipmentRepository,
			IDbConnection connection, IMessageContext context)
		// CreateShipment command, 
		// IDbConnection conn, 
		// IMessageBus bus)
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
			await context.PublishAsync(new ShipmentCreatedEvent(){ CustomerId = shipment.CustomerId, ShipmentId = shipment.Id, ShipmentNumber = shipment.ShipmentNumber });
			//throw new ArgumentException();
			transaction.Commit();
		}
		catch
		{
			transaction.Rollback();
			throw;
		}
		const string sqlItem = "INSERT INTO ShipmentItems (Id, ShipmentId, ProductCode, Quantity, UnitPrice) VALUES (@Id, @ShipmentId, @ProductCode, @Quantity, @UnitPrice)";
		await connection.ExecuteAsync(new CommandDefinition(sqlItem, shipmentsItems, cancellationToken: CancellationToken.None));
		// await shipmentRepository.SaveAsync(shipment, shipmentsItems, CancellationToken.None);
		Console.WriteLine("created abc ");
		return Results.Created();
	}
}
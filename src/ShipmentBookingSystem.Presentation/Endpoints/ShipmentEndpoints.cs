using System.Data;
using Dapper;
using Microsoft.AspNetCore.Http;
using ShipmentBookingSystem.Application.Interfaces;
using ShipmentBookingSystem.Application.Validators;
using ShipmentBookingSystem.Domain.Entities;
using Wolverine.Attributes;
using Wolverine.Http;

namespace ShipmentBookingSystem.Presentation.Endpoints;

public static class ShipmentEndpoints
{
	[WolverinePost("/shipments")]
	[Transactional] 
	public static async Task<IResult> Post(CreateShipment request,
			IShipmentRepository shipmentRepository,
			IDbConnection connection)
		// CreateShipment command, 
		// IDbConnection conn, 
		// IMessageBus bus)
	{
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
		
		const string sqlShipment = "INSERT INTO Shipments (Id, ShipmentNumber, CustomerId, CreatedAt) VALUES (@Id, @ShipmentNumber, @CustomerId, @CreatedAt)";
		await connection.ExecuteAsync(new CommandDefinition(sqlShipment, shipment, cancellationToken: CancellationToken.None));
		throw new ArgumentException();
		const string sqlItem = "INSERT INTO ShipmentItems (Id, ShipmentId, ProductCode, Quantity, UnitPrice) VALUES (@Id, @ShipmentId, @ProductCode, @Quantity, @UnitPrice)";
		await connection.ExecuteAsync(new CommandDefinition(sqlItem, shipmentsItems, cancellationToken: CancellationToken.None));
		// await shipmentRepository.SaveAsync(shipment, shipmentsItems, CancellationToken.None);
		Console.WriteLine("created abc ");
		return Results.Created();
	}
}
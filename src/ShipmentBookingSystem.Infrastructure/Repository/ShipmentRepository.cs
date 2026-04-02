using System.Data;
using Dapper;
using ShipmentBookingSystem.Application.Interfaces;
using ShipmentBookingSystem.Domain.Entities;

namespace ShipmentBookingSystem.Infrastructure.Repository;

internal class ShipmentRepository : IShipmentRepository
{
	private readonly IDbConnection _db;

	public ShipmentRepository(IDbConnection db) => _db = db;

	public async Task SaveAsync(Shipment shipment, IEnumerable<ShipmentItem> items, IDbTransaction transaction, CancellationToken ct)
	{
		const string sqlShipment = "INSERT INTO Shipments (Id, ShipmentNumber, CustomerId, CreatedAt) VALUES (@Id, @ShipmentNumber, @CustomerId, @CreatedAt)";
		await _db.ExecuteAsync(new CommandDefinition(sqlShipment, shipment, cancellationToken: ct, transaction: transaction));
		throw new ArgumentException();
		const string sqlItem = "INSERT INTO ShipmentItems (Id, ShipmentId, ProductCode, Quantity, UnitPrice) VALUES (@Id, @ShipmentId, @ProductCode, @Quantity, @UnitPrice)";
		await _db.ExecuteAsync(new CommandDefinition(sqlItem, items, cancellationToken: ct));
	}
}
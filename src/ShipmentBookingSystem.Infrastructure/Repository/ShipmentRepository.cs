using System.Data;
using Dapper;
using Newtonsoft.Json;
using ShipmentBookingSystem.Application.Interfaces;
using ShipmentBookingSystem.Domain.Entities;
using ShipmentBookingSystem.Domain.Models;

namespace ShipmentBookingSystem.Infrastructure.Repository;

internal class ShipmentRepository : IShipmentRepository
{
    private const string SqlShipmentInsert = "INSERT INTO Shipments (Id, ShipmentNumber, CustomerId, CreatedAt) VALUES (@Id, @ShipmentNumber, @CustomerId, @CreatedAt)";

    private const string SqlItemsInsert = "INSERT INTO ShipmentItems (Id, ShipmentId, ProductCode, Quantity, UnitPrice) VALUES (@Id, @ShipmentId, @ProductCode, @Quantity, @UnitPrice)";

    private const string SqlSummaryQuery = @"
    SELECT 
    s.CustomerId as customerId,
    COUNT(s.Id) as shipmentsCount,
    SUM (items.TotalShipmentAmount) as totalAmount,
    (
        SELECT si.ProductCode as productCode,
        SUM(si.Quantity) as totalQuantity
        FROM ShipmentsItems si
        WHERE si.ShipmentId IN
        (
            SELECT Id FROM Shipments 
            WHERE CustomerId = s.customerId
        )
        GROUP BY si.ProductCode
        FOR JSON PATH
    ) as products
    FROM Shipments s
    OUTTER APPLY
    (
        SELECT SUM (Quantity * UnitPrice)
        AS TotalShipmentsAmount
        FROM ShipmentsItems
        WHERE ShipmentID = s.Id
    ) items
    WHERE
    s.CustomerId = @customerId AND
    s.CreatedAt >= @createdFrom AND
    s.CreatedAt <= @createdTo 
    GROUP BY s.CustomerId
    HAVING SUM
    (
    items.TotalShipmentsAmount >= @minTotalAmount
    AND
    COUNT (s.Id) >= @minShipments
    )
    FOR JSON PATH
";

    private readonly IDbConnection _dbConnection;

    private IDbTransaction _transaction;
    
    public ShipmentRepository(IDbConnection db, IDbTransaction dbTransaction)
    {
        _transaction = dbTransaction;
        _dbConnection = db;
    }

	public Task AddShipmentAsync(Shipment shipment, CancellationToken ct)
	{
		return _dbConnection.ExecuteAsync(new CommandDefinition(SqlShipmentInsert, shipment, cancellationToken: ct, transaction: _transaction));
	}

    public Task AddShipmentItemsAsync(IEnumerable<ShipmentItem> items, CancellationToken ct)
    {
        return _dbConnection.ExecuteAsync(new CommandDefinition(SqlItemsInsert, items, cancellationToken: ct, transaction: _transaction));
    }

    public async Task<ShipmentSummary> GetSummaryAsync(int customerId, DateTime createdFrom, DateTime createdTo, int minTotalAmount, int minShipments)
    {
        var summary = await _dbConnection.QueryFirstOrDefaultAsync<string>(new CommandDefinition(SqlSummaryQuery,
            new
            {
                customerId = customerId,
                createdFrom = createdFrom,
                createdTo = createdTo,
                minTotalAmount = minTotalAmount,
                minShipments = minShipments
            }));
        var summaryObject = JsonConvert.DeserializeObject<ShipmentSummary>(summary);
        return summaryObject;
    }
}
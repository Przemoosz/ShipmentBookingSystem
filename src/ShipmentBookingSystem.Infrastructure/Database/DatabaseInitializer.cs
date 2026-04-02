using Dapper;
using Microsoft.Data.SqlClient;

namespace ShipmentBookingSystem.Infrastructure.Database;

internal sealed class DatabaseInitializer : IDatabaseInitializer
{
	private const string SqlScript = @"
		USE ShipmentDb;
		IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Shipments')
		BEGIN
		    CREATE TABLE Shipments (
		        Id UNIQUEIDENTIFIER PRIMARY KEY,
		        ShipmentNumber NVARCHAR(100) NOT NULL,
		        CustomerId INT NOT NULL,
		        CreatedAt DATETIME2 NOT NULL
		    );

		    CREATE TABLE ShipmentItems (
		        Id UNIQUEIDENTIFIER PRIMARY KEY,
		        ShipmentId UNIQUEIDENTIFIER NOT NULL,
		        ProductCode NVARCHAR(50) NOT NULL,
		        Quantity INT NOT NULL,
		        UnitPrice DECIMAL(18, 2) NOT NULL,
		        FOREIGN KEY (ShipmentId) REFERENCES Shipments(Id)
		    );
		END";

	public async Task InitializeAsync(string connectionString)
	{
		await using var conn = new SqlConnection(connectionString);
		await conn.ExecuteAsync(@"
        IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ShipmentDb')
        BEGIN
            CREATE DATABASE ShipmentDb;
        END");
		await conn.ExecuteAsync(SqlScript);
	}
}
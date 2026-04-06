using System.Data;
using Dapper;

namespace ShipmentBookingSystem.Infrastructure.Database;

internal sealed class DatabaseInitializer : IDatabaseInitializer
{
	private readonly IDbConnection _dbConnection;

	private const string SqlScript = @"
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
        END;

        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OutboxEvents')
        BEGIN
            CREATE TABLE [dbo].[OutboxEvents]
            (
                Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                EventType NVARCHAR(500) NOT NULL,
                Payload NVARCHAR(MAX) NOT NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                ProcessedAt DATETIME2 NULL,
                Attempts INT DEFAULT 0,
                LastError NVARCHAR(MAX) NULL,
                IsProcessed BIT DEFAULT 0
            );
            
            CREATE INDEX IX_OutboxEvents_Processed 
                ON [dbo].[OutboxEvents](IsProcessed, CreatedAt);
        END;
    ";

	public DatabaseInitializer(IDbConnection dbConnection)
	{
		_dbConnection = dbConnection;
	}
    
	public async Task InitializeAsync()
	{
		await _dbConnection.ExecuteAsync(SqlScript);
	}
}
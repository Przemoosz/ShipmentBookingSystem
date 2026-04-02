using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.SqlServer.Server;
using ShipmentBookingSystem.Application.Interfaces;

namespace ShipmentBookingSystem.Infrastructure.OutboxEvent;

public record OutboxEvent
{
	public Guid Id { get; init; }
	public string EventType { get; init; }
	public string Payload { get; init; }
	public DateTime CreatedAt { get; init; }
	public DateTime? ProcessedAt { get; init; }
	public int Attempts { get; init; }
	public string? LastError { get; init; }
	public bool IsProcessed { get; init; }
}

internal sealed class OutboxService : IOutboxService
{
	public async Task SaveEventAsync(string eventType, object eventPayload, IDbConnection connection, IDbTransaction transaction)
	{
		var payload = JsonSerializer.Serialize(eventPayload);
        
		const string sql = @"
            INSERT INTO [dbo].[OutboxEvents] (EventType, Payload, CreatedAt, IsProcessed)
            VALUES (@EventType, @Payload, @CreatedAt, 0)";
        
		await connection.ExecuteAsync(sql, new
		{
			EventType = eventType,
			Payload = payload,
			CreatedAt = DateTime.UtcNow,
		},  transaction);
	}
}
using System.Collections;
using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using ShipmentBookingSystem.Application.Interfaces;
using ShipmentBookingSystem.Domain.Events;

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
	private readonly ILogger<OutboxService> _logger;

	public OutboxService(ILogger<OutboxService> logger)
	{
		_logger = logger;
	}
	public async Task SaveEventAsync(Guid id, string eventType, object eventPayload, IDbConnection connection, IDbTransaction transaction)
	{
		const string sql = @"
            INSERT INTO [dbo].[OutboxEvents] (Id, EventType, Payload, CreatedAt, IsProcessed)
            VALUES (@Id, @EventType, @Payload, @CreatedAt, 0)";
		
		try
		{
			var payload = JsonSerializer.Serialize(eventPayload);
			await connection.ExecuteAsync(sql, new
			{
				Id = id,
				EventType = eventType,
				Payload = payload,
				CreatedAt = DateTime.UtcNow,
			},  transaction);
			_logger.LogInformation("Event saved to outbox. EventType: {EventType}", eventType);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to save event to outbox. EventType: {EventType}", eventType);
			throw;
		}
	}

	public async Task<IEnumerable<(Guid, string)>> GetPendingEventsAsync(IDbConnection connection, CancellationToken ct)
	{
		const string sql = @"
            SELECT 
                Id,
                Payload
            FROM [dbo].[OutboxEvents]
            WHERE IsProcessed = 0
            ORDER BY CreatedAt ASC";
        
		try
		{
			var result = await connection.QueryAsync<(Guid, string)>(
				new CommandDefinition(sql, cancellationToken: ct));
			_logger.LogInformation("Retrieved {Count} pending events from outbox", result.Count());
			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to retrieve pending events from outbox");
			throw;
		}
	}

	public async Task SaveEventAsFinishedAsync(Guid eventId, IDbConnection connection, IDbTransaction transaction)
	{
		const string sql = @"
            UPDATE [dbo].[OutboxEvents]
            SET IsProcessed = 1,
                ProcessedAt = @ProcessedAt
            WHERE Id = @Id";
        
		try
		{
			await connection.ExecuteAsync(sql, new
			{
				Id = eventId,
				ProcessedAt = DateTime.UtcNow
			}, transaction);
			
			_logger.LogInformation("Event marked as finished. EventId: {EventId}", eventId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to mark event as finished. EventId: {EventId}", eventId);
			throw;
		}
	}

	public async Task SaveEventAsFailedAsync(Guid eventId, string errorMessage, IDbConnection connection, IDbTransaction transaction)
	{
		const string sql = @"
            UPDATE [dbo].[OutboxEvents]
            SET Attempts = Attempts + 1,
                LastError = @LastError
            WHERE Id = @Id";
        
		try
		{
			await connection.ExecuteAsync(sql, new
			{
				Id = eventId,
				LastError = errorMessage
			}, transaction);
			
			_logger.LogWarning("Event marked as failed. EventId: {EventId}, Error: {Error}", eventId, errorMessage);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to mark event as failed. EventId: {EventId}", eventId);
			throw;
		}
	}
}

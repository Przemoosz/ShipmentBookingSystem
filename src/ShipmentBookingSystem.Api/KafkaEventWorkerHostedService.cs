using System.Data;
using ShipmentBookingSystem.Application.Interfaces;
using Wolverine;

namespace ShipmentBookingSystem.Api;

public class KafkaEventWorkerHostedService: IHostedService
{
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public KafkaEventWorkerHostedService(IServiceScopeFactory  serviceScopeFactory)
	{
		_serviceScopeFactory = serviceScopeFactory;
	}
	
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		
		while(cancellationToken.IsCancellationRequested == false)
		{
			using  var scope = _serviceScopeFactory.CreateScope();
			var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();
			var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();
			var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
			// 1. Pobierz oczekujące eventy z bazy
			var result = await outboxService.GetPendingEventsAsync(dbConnection!, cancellationToken);

			foreach ((Guid, string) valueTuple in result)
			{
				try
				{
					await bus.PublishAsync(valueTuple.Item2);
					await outboxService.SaveEventAsFinishedAsync(valueTuple.Item1, dbConnection!, null!);
				}
				catch (Exception e)
				{
					await outboxService.SaveEventAsFailedAsync(valueTuple.Item1, e.Message, dbConnection!, null!);
				}
			}
			
			await Task.Delay(5000);
			
			// 2. Wyślij do Kafki
			// 3. Oznacz jako wysłane lub nieudane
		}	
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
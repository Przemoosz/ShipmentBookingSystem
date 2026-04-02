using Microsoft.Extensions.DependencyInjection;
using ShipmentBookingSystem.Application.Interfaces;
using ShipmentBookingSystem.Infrastructure.Database;
using ShipmentBookingSystem.Infrastructure.OutboxEvent;
using ShipmentBookingSystem.Infrastructure.Repository;

namespace ShipmentBookingSystem.Infrastructure
{
	public static class InfrastructureInstaller
	{
		public static void InstallInfrastructure(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
			serviceCollection.AddScoped<IShipmentRepository, ShipmentRepository>();
			serviceCollection.AddScoped<IOutboxService, OutboxService>();
		}
	}
}

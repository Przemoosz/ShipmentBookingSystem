using Microsoft.Extensions.DependencyInjection;
using ShipmentBookingSystem.Infrastructure.Database;

namespace ShipmentBookingSystem.Infrastructure
{
	public static class InfrastructureInstaller
	{
		public static void InstallInfrastructure(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddTransient<IDatabaseInitializer, DatabaseInitializer>();
		}
	}
}

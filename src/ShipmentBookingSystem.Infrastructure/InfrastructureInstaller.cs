using Microsoft.Extensions.DependencyInjection;
using ShipmentBookingSystem.Application.Interfaces;
using ShipmentBookingSystem.Infrastructure.Abstraction;
using ShipmentBookingSystem.Infrastructure.Database;

namespace ShipmentBookingSystem.Infrastructure;

public static class InfrastructureInstaller
{
	public static void InstallInfrastructure(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
		serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
	}
}
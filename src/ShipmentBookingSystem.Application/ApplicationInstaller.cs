using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ShipmentBookingSystem.Application.Validators;

namespace ShipmentBookingSystem.Application;

public static class ApplicationInstaller
{
	public static void InstallApplication(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddScoped<IValidator<CreateShipment>, CreateShipmentValidator>();
	}
}
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ShipmentBookingSystem.Application.Handlers;
using ShipmentBookingSystem.Application.Requests;
using ShipmentBookingSystem.Application.Validators;

namespace ShipmentBookingSystem.Application;

public static class ApplicationInstaller
{
	public static void InstallApplication(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddScoped<IValidator<CreateShipmentRequest>, CreateShipmentValidator>();
		serviceCollection.AddScoped<SaveShipmentResquestHandler>();
		serviceCollection.AddScoped<ShipmentSummaryQueryHandler>();
	}
}
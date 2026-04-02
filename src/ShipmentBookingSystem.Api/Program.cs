using JasperFx.Resources;
using ShipmentBookingSystem.Application;
using ShipmentBookingSystem.Domain;
using ShipmentBookingSystem.Infrastructure;
using ShipmentBookingSystem.Presentation;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Dapper;
using Microsoft.Data.SqlClient;
using ShipmentBookingSystem.Infrastructure.Database;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

namespace ShipmentBookingSystem.Api
{
	public sealed class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.InstallApplication();
			builder.Services.InstallDomain();
			builder.Services.InstallInfrastructure();
			builder.Services.InstallPresentation();
			builder.Host.UseWolverine(opts =>
			{
				var assembly = Assembly.Load("ShipmentBookingSystem.Presentation");
				opts.Discovery.IncludeAssembly(assembly);
				opts.UseFluentValidation();
				opts.UseFluentValidationProblemDetail();
				opts.Services.AddResourceSetupOnStartup();
			});
			builder.Services.AddControllers();
			builder.Services.AddWolverineHttp();
			builder.Services.AddOpenApi();

			var app = builder.Build();
			var connectionString = builder.Configuration.GetConnectionString("Master");
			Console.WriteLine(connectionString);

			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
			}
			
			app.MapWolverineEndpoints(opts => {
				opts.UseFluentValidationProblemDetailMiddleware(); 
				opts.UseDataAnnotationsValidationProblemDetailMiddleware();
			});
			app.UseHttpsRedirection();
			
			app.UseAuthorization();
			var dbInitializer = app.Services.GetService<IDatabaseInitializer>();
			if (string.IsNullOrEmpty(connectionString) || dbInitializer == null)
			{
				throw new InvalidOperationException(
					"Can not initialize database initializer or connection string is empty");
			}

			await dbInitializer.InitializeAsync(connectionString);
			
			await app.RunAsync();
		}
	}
}
public class HelloEndpoint
{
	[WolverineGet("/get")]
	public string Get() => "Hello.";
}



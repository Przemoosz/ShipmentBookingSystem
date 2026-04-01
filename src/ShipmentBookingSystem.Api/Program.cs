
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Dapper.Extensions.MSSQL;
using ShipmentBookingSystem.Application;
using ShipmentBookingSystem.Domain;
using ShipmentBookingSystem.Infrastructure;
using ShipmentBookingSystem.Presentation;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

namespace ShipmentBookingSystem.Api
{
	public sealed class Program
	{
		public static void Main(string[] args)
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
			});
			builder.Services.AddControllers();
			builder.Services.AddWolverineHttp();
			builder.Services.AddOpenApi();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
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
			
			app.Run();
		}
	}
}
public class HelloEndpoint
{
	[WolverineGet("/get")]
	public string Get() => "Hello.";
}
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
using Wolverine.SqlServer;

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
			var connectionString = builder.Configuration.GetConnectionString("Default");
			if (string.IsNullOrEmpty(connectionString))
			{
				throw new ArgumentNullException(nameof(connectionString),
					"Connection string is empty");
			}


			builder.Services.AddScoped<IDbConnection>(_ =>
			{
				var conn = new SqlConnection(connectionString);
				conn.Open();
				return conn;
			});
			builder.Host.UseWolverine(opts =>{
				var assembly = Assembly.Load("ShipmentBookingSystem.Presentation");
				opts.Discovery.IncludeAssembly(assembly);
				opts.UseFluentValidation();
				opts.UseFluentValidationProblemDetail();
				opts.PersistMessagesWithSqlServer(connectionString);
				opts.Services.AddResourceSetupOnStartup();
				opts.Policies.AutoApplyTransactions();
			});
			builder.Services.AddControllers();
			builder.Services.AddWolverineHttp();
			builder.Services.AddOpenApi();

			var app = builder.Build();

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
			using (IServiceScope scope = app.Services.CreateScope())
			{
				var dbInitializer = scope.ServiceProvider.GetService<IDatabaseInitializer>();
				if (dbInitializer == null)
				{
					throw new InvalidOperationException(
						"Can not initialize database initializer");
				}
				await dbInitializer.InitializeAsync(connectionString);

			}
			await app.RunAsync();
		}
	}
}
public class HelloEndpoint
{
	[WolverineGet("/get")]
	public string Get() => "Hello.";
}



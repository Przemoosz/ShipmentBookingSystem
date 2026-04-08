using System.Data;
using System.Reflection;
using JasperFx.Resources;
using Microsoft.Data.SqlClient;
using ShipmentBookingSystem.Application;
using ShipmentBookingSystem.Domain;
using ShipmentBookingSystem.Domain.Events;
using ShipmentBookingSystem.Infrastructure;
using ShipmentBookingSystem.Infrastructure.Abstraction;
using ShipmentBookingSystem.Presentation;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Kafka;

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
			builder.Services.AddScoped<IDbConnection>(serviceProvider =>
			{
				var a = builder.Configuration.GetConnectionString("Default");

                var conn = new SqlConnection(a);
				conn.Open();
				return conn;
			});
			builder.Host.UseWolverine(opts =>{
				var presentationAssembly = Assembly.Load("ShipmentBookingSystem.Presentation");
				var applicationAssembly = Assembly.Load("ShipmentBookingSystem.Application");
				opts.Discovery.IncludeAssembly(presentationAssembly);
				opts.Discovery.IncludeAssembly(applicationAssembly);
				opts.UseFluentValidation();
				opts.UseFluentValidationProblemDetail();
				// opts.PersistMessagesWithSqlServer(connectionString); // tbr
				opts.Services.AddResourceSetupOnStartup();
				opts.Policies.AutoApplyTransactions(); // tbr
				opts.UseKafka(builder.Configuration.GetSection("Kafka:BootstrapServers").Value); // add null check
				opts.PublishMessage<ShipmentCreatedEvent>()
					.ToKafkaTopic("shipment-created-event")
					.Specification(spec =>
					{
						spec.NumPartitions = 1;
						spec.ReplicationFactor = 1;
					})
					.TopicCreation(async (client, topic) =>
					{
						topic.Specification.NumPartitions = 1;
						topic.Specification.ReplicationFactor = 1;
						await client.CreateTopicsAsync([topic.Specification]);
					});

			});
			builder.Services.AddControllers(); //tbr
			builder.Services.AddWolverineHttp();
			builder.Services.AddOpenApi(); //tbr
			var app = builder.Build();

			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi(); // tbr
			}
			
			app.MapWolverineEndpoints(opts => {
				opts.UseFluentValidationProblemDetailMiddleware(); 
				opts.UseDataAnnotationsValidationProblemDetailMiddleware();
			});
			app.UseHttpsRedirection(); // probably tbr
			
			app.UseAuthorization();
			using (IServiceScope scope = app.Services.CreateScope())
			{
				var dbInitializer = scope.ServiceProvider.GetService<IDatabaseInitializer>();
				if (dbInitializer == null)
				{
					throw new InvalidOperationException(
						"Can not initialize database initializer");
				}
				await dbInitializer.InitializeAsync();

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



using JasperFx.Resources;
using Microsoft.Data.SqlClient;
using ShipmentBookingSystem.Application;
using ShipmentBookingSystem.Domain;
using ShipmentBookingSystem.Infrastructure;
using ShipmentBookingSystem.Infrastructure.Database;
using ShipmentBookingSystem.Presentation;
using System.Data;
using System.Reflection;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;
using Wolverine.Kafka;
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

			var kafkaBootstrapServers = builder.Configuration.GetSection("Kafka:BootstrapServers").Value; 



			builder.Services.AddScoped<IDbConnection>(_ =>
			{
				var conn = new SqlConnection(connectionString);
				conn.Open(); // probably tbr
				return conn;
			});
			builder.Host.UseWolverine(opts =>{
				var presentationAssembly = Assembly.Load("ShipmentBookingSystem.Presentation");
				var applicationAssembly = Assembly.Load("ShipmentBookingSystem.Application");
				opts.Discovery.IncludeAssembly(presentationAssembly);
				opts.Discovery.IncludeAssembly(applicationAssembly);
				opts.UseFluentValidation();
				opts.UseFluentValidationProblemDetail();
				opts.PersistMessagesWithSqlServer(connectionString); // tbr
				opts.Services.AddResourceSetupOnStartup();
				opts.Policies.AutoApplyTransactions(); // tbr
				opts.UseKafka(kafkaBootstrapServers); // add null check
				opts.PublishMessage<string>()
					.ToKafkaTopic("colors")
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



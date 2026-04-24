using Confluent.Kafka;
using JasperFx.Resources;
using Microsoft.Data.SqlClient;
using ShipmentBookingSystem.Application;
using ShipmentBookingSystem.Domain;
using ShipmentBookingSystem.Domain.Events;
using ShipmentBookingSystem.Infrastructure;
using ShipmentBookingSystem.Infrastructure.Abstraction;
using ShipmentBookingSystem.Presentation;
using System.Data;
using System.Reflection;
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

            builder.Services.AddSingleton<IProducer<string, string>>(_ =>
            {
                var bootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
                    ?? throw new InvalidOperationException("Missing Kafka:BootstrapServers");

                var config = new ProducerConfig
                {
                    BootstrapServers = bootstrapServers,
                    Acks = Acks.All,
                    EnableIdempotence = true,
                    MessageSendMaxRetries = 3,
                    RetryBackoffMs = 200,
                    MessageTimeoutMs = 7000,
                    RequestTimeoutMs = 5000
                };

                return new ProducerBuilder<string, string>(config).Build();
            });


            builder.Host.UseWolverine(opts =>{
				var presentationAssembly = Assembly.Load("ShipmentBookingSystem.Presentation");
				var applicationAssembly = Assembly.Load("ShipmentBookingSystem.Application");
				opts.Discovery.IncludeAssembly(presentationAssembly);
				opts.Discovery.IncludeAssembly(applicationAssembly);
				opts.UseFluentValidation();
				opts.UseFluentValidationProblemDetail();
				opts.Services.AddResourceSetupOnStartup();
				opts.UseKafka(builder.Configuration.GetSection("Kafka:BootstrapServers").Value);
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
			builder.Services.AddControllers();
			builder.Services.AddWolverineHttp();
			builder.Services.AddOpenApi();
			var app = builder.Build();


			
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
				await dbInitializer.InitializeAsync();

			}
			await app.RunAsync();
		}
	}
}

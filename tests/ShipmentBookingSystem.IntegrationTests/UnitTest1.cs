using Confluent.Kafka;
using JasperFx.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ShipmentBookingSystem.Api;
using ShipmentBookingSystem.Application.Requests;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.Kafka;
using Testcontainers.MsSql;
namespace ShipmentBookingSystem.IntegrationTests
{
    public class UnitTest1: IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly IntegrationTestWebAppFactory _factory;
        private readonly HttpClient _client;

        public UnitTest1(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
        [Fact]
        public async Task Test1()
        {
            var request = new CreateShipmentRequest(
                 ShipmentNumber: "TEST-SHIP-001",
                 CustomerId: 1234,
                 Items: new List<ShipmentBookingSystem.Application.Validators.ShipmentItem>
                 {
                    new("PRODUCT-A", 10, 15.50m),
                    new("PRODUCT-B", 5, 25.00m)
                 }
             );

            // Act
            var response = await _client.PostAsJsonAsync("/shipments", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }

    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {

        private readonly MsSqlContainer _dbContainer;
        private readonly KafkaContainer _kafkaContainer;

        public IntegrationTestWebAppFactory()
        {
            _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("Password123!")
                .Build();
            KafkaConfiguration kafkaConfiguration = new KafkaConfiguration();
            _kafkaContainer = new KafkaBuilder()
                .Build();
        }
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var a = _dbContainer.GetConnectionString();
            var b =_kafkaContainer.GetConnectionString();

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // 1. Zamiast Clear(), po prostu dodaj nową kolekcję na końcu. 
                // Ostatnie dodane źródło zawsze nadpisuje poprzednie (Last in wins).
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Default"] = a,
                    ["Kafka:BootstrapServers"] = b,
                    ["Logging:LogLevel:Default"] = "Information"
                });
            });
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            await _kafkaContainer.StartAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _kafkaContainer.StopAsync();
            await _dbContainer.DisposeAsync();
            await _kafkaContainer.DisposeAsync();   
        }
    }
}

using Confluent.Kafka;
using JasperFx.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ShipmentBookingSystem.Api;
using ShipmentBookingSystem.Application.Requests;
using ShipmentBookingSystem.Application.Validators;
using ShipmentBookingSystem.Domain.Events;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Testcontainers.Kafka;
using Testcontainers.MsSql;
namespace ShipmentBookingSystem.IntegrationTests
{
    
    public class ShipmentsCreateTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly IntegrationTestWebAppFactory _factory;
        private readonly HttpClient _client;

        public ShipmentsCreateTests(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ShipmentAndShipmentsItems_ShouldBeSavedInKafkaAndInSqlDb()
        {
            var request = new CreateShipmentRequest(
                 ShipmentNumber: "TEST-SHIP-001",
                 CustomerId: 1234,
                 Items: new List<ShipmentItem>
                 {
                    new("PRODUCT-A", 10, 15.50m),
                    new("PRODUCT-B", 5, 25.00m)
                 }
             );

            // Act
            var response = await _client.PostAsJsonAsync("/shipments", request);

            // Assert API
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Assert SQL
            var sqlSnapshot = await GetShipmentFromSqlAsync(request.ShipmentNumber);
            Assert.NotNull(sqlSnapshot);
            Assert.Equal(request.CustomerId, sqlSnapshot!.Value.CustomerId);
            Assert.Equal(request.Items.Count, sqlSnapshot.Value.ItemsCount);

            var expectedTotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
            Assert.Equal(expectedTotal, sqlSnapshot.Value.TotalAmount);

            // Assert Kafka
            var kafkaEvent = await ConsumeShipmentCreatedEventAsync(
                request.ShipmentNumber,
                TimeSpan.FromSeconds(20));

            Assert.NotNull(kafkaEvent);
            Assert.Equal(request.ShipmentNumber, kafkaEvent!.ShipmentNumber);
            Assert.Equal(request.CustomerId, kafkaEvent.CustomerId);
        }

        [Fact]
        public async Task ShipmentAndShipmentsItems_ShouldNotBeSavedInKafkaAndInSqlDb_InCaseOfKafkaContainerError()
        {
            var request = new CreateShipmentRequest(
                 ShipmentNumber: "TEST-SHIP-001",
                 CustomerId: 1234,
                 Items: new List<ShipmentItem>
                 {
                    new("PRODUCT-A", 10, 15.50m),
                    new("PRODUCT-B", 5, 25.00m)
                 }
             );
            await _factory.PauseKafkaContainer();

            // Act
            var response = await _client.PostAsJsonAsync("/shipments", request);

            await _factory.UnpauseKafkaContainer();

            // Assert API
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            // Assert SQL
            var sqlSnapshot = await GetShipmentFromSqlAsync(request.ShipmentNumber);
            Assert.Null(sqlSnapshot);

            // Assert Kafka
            var kafkaEvent = await ConsumeShipmentCreatedEventAsync(
                request.ShipmentNumber,
                TimeSpan.FromSeconds(20));

            Assert.Null(kafkaEvent);
        }

        [Fact]
        public async Task ShipmentAndShipmentsItems_ShouldNotBeSavedInKafkaAndInSqlDb_InCaseOfSqlContainerError()
        {
            await Task.Delay(4000);
            var request = new CreateShipmentRequest(
                 ShipmentNumber: "TEST-SHIP-001",
                 CustomerId: 1234,
                 Items: new List<ShipmentItem>
                 {
                    new("PRODUCT-A", 10, 15.50m),
                    new("PRODUCT-B", 5, 25.00m)
                 }
             );
            await _factory.PauseSqlContainer();

            // Act
            var response = await _client.PostAsJsonAsync("/shipments", request);

            await _factory.UnpauseSqlContainer();

            // Assert API
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

            // Assert SQL
            var sqlSnapshot = await GetShipmentFromSqlAsync(request.ShipmentNumber);
            Assert.Null(sqlSnapshot);

            // Assert Kafka
            var kafkaEvent = await ConsumeShipmentCreatedEventAsync(
                request.ShipmentNumber,
                TimeSpan.FromSeconds(20));

            Assert.Null(kafkaEvent);
        }

        private async Task<(Guid ShipmentId, int CustomerId, int ItemsCount, decimal TotalAmount)?> GetShipmentFromSqlAsync(string shipmentNumber)
        {
            const string sql = """
                SELECT
                    s.Id,
                    s.CustomerId,
                    COUNT(si.Id) AS ItemsCount,
                    SUM(CAST(si.Quantity * si.UnitPrice AS decimal(18,2))) AS TotalAmount
                FROM Shipments s
                INNER JOIN ShipmentItems si ON si.ShipmentId = s.Id
                WHERE s.ShipmentNumber = @ShipmentNumber
                GROUP BY s.Id, s.CustomerId
                """;

            await using var connection = new SqlConnection(_factory.GetSqlConnectionString());
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ShipmentNumber", shipmentNumber);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return null;
            }

            return (
                ShipmentId: reader.GetGuid(0),
                CustomerId: reader.GetInt32(1),
                ItemsCount: reader.GetInt32(2),
                TotalAmount: reader.GetDecimal(3));
        }

        private async Task<ShipmentCreatedEvent?> ConsumeShipmentCreatedEventAsync(string shipmentNumber, TimeSpan timeout)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _factory.GetKafkaBootstrapAddress(),
                GroupId = $"it-shipment-created-{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe("shipment-created-event");

            var deadline = DateTime.UtcNow.Add(timeout);

            while (DateTime.UtcNow < deadline)
            {
                var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(500));
                if (consumeResult is null)
                {
                    await Task.Delay(100);
                    continue;
                }

                var payload = consumeResult.Message.Value;
                if (string.IsNullOrWhiteSpace(payload))
                {
                    continue;
                }

                var message = JsonSerializer.Deserialize<ShipmentCreatedEvent>(payload);
                if (message is not null && message.ShipmentNumber == shipmentNumber)
                {
                    return message;
                }
            }

            return null;
        }
    } 
}

    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer;
        private readonly KafkaContainer _kafkaContainer;

        public Task PauseKafkaContainer() => _kafkaContainer.PauseAsync();
        public Task PauseSqlContainer() => _dbContainer.PauseAsync();
        public Task UnpauseKafkaContainer() => _kafkaContainer.UnpauseAsync();
        public Task UnpauseSqlContainer() => _dbContainer.UnpauseAsync();
        public string GetKafkaBootstrapAddress() => _kafkaContainer.GetBootstrapAddress();
        public string GetSqlConnectionString() => _dbContainer.GetConnectionString();

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


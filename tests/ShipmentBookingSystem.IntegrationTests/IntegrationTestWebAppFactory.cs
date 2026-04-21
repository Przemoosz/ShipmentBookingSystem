using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ShipmentBookingSystem.Api;
using Testcontainers.Kafka;
using Testcontainers.MsSql;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
	public MsSqlContainer DbContainer { get; }
	private readonly KafkaContainer _kafkaContainer;

	public Task PauseKafkaContainer() => _kafkaContainer.PauseAsync();
	public Task PauseSqlContainer() => DbContainer.PauseAsync();
	public Task UnpauseKafkaContainer() => _kafkaContainer.UnpauseAsync();
	public Task UnpauseSqlContainer() => DbContainer.UnpauseAsync();
	public string GetKafkaBootstrapAddress() => _kafkaContainer.GetBootstrapAddress();
	public string GetSqlConnectionString() => DbContainer.GetConnectionString();

	public IntegrationTestWebAppFactory()
	{
		DbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
			.WithPassword("Password123!")
			.WithExposedPort(59277)
			.Build();
		_kafkaContainer = new KafkaBuilder()
			.Build();
	}
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		var a = DbContainer.GetConnectionString();
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
		await DbContainer.StartAsync();
		await _kafkaContainer.StartAsync();
	}

	async Task IAsyncLifetime.DisposeAsync()
	{
		await DbContainer.StopAsync();
		await _kafkaContainer.StopAsync();
		await DbContainer.DisposeAsync();
		await _kafkaContainer.DisposeAsync();   
	}
}
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ShipmentBookingSystem.Api;
using Testcontainers.Kafka;
using Testcontainers.MsSql;

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
		_kafkaContainer = new KafkaBuilder("confluentinc/cp-kafka:7.7.8")
            .Build();
	}
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((context, config) =>
		{
			config.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["ConnectionStrings:Default"] = _dbContainer.GetConnectionString(),
				["Kafka:BootstrapServers"] = _kafkaContainer.GetConnectionString(),
			});
		});
	}

	public async Task ExecuteSQLAsync(string sql)
	{
		if (_dbContainer.State == DotNet.Testcontainers.Containers.TestcontainersStates.Running)
		{
			await _dbContainer.ExecScriptAsync(sql);
		}
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
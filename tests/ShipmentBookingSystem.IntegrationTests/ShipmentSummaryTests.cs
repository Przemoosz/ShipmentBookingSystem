using System.Net;
using Newtonsoft.Json;

namespace ShipmentBookingSystem.IntegrationTests;

public class ShipmentSummaryTests : IClassFixture<IntegrationTestWebAppFactory>
{
	private readonly IntegrationTestWebAppFactory _factory;
	private readonly HttpClient _client;

	public ShipmentSummaryTests(IntegrationTestWebAppFactory factory)
	{
		_factory = factory;
		_client = factory.CreateClient();
	}

	[Fact]
	public async Task ShipmentSummaryIsCorrect()
	{
		// Arrange
		var endpoint = "/shipments/summary?customerId=1234&createdFrom=2022-01-01&createdTo=2026-03-31&minTotalAmount=1&minShipments=1";
		await PrepareDatabaseAsync();
		// Act
		var httpResponseMessage = await _client.GetAsync(endpoint);

		// Assert
		Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
		
		var result = JsonConvert.DeserializeObject<Root>(await httpResponseMessage.Content.ReadAsStringAsync());

		Assert.NotNull(result);
		Assert.NotNull(result.products);
		Assert.NotEmpty(result.products);
		Assert.Equal(1234, result.customerID);
		Assert.Equal(3, result.shipmentsCount);
		Assert.Equal(27, result.totalAmount); 
	}
	
	
	private async Task PrepareDatabaseAsync()
	{
		const string SQLShipment = """
		                           			INSERT INTO Shipments (Id, ShipmentNumber, CustomerId, CreatedAt) VALUES 
		                           			('00000000-0000-0000-0000-000000000001', 'TEST-SHIP-001', 1234, '2024-01-01T10:00:00Z'),
		                           			('00000000-0000-0000-0000-000000000002', 'TEST-SHIP-002', 1234, '2024-01-02T10:00:00Z');
		                           """;
		const string SQLItems = """
		                           			INSERT INTO ShipmentItems (Id, ShipmentId, ProductCode, Quantity, UnitPrice) VALUES 
		                           			('00000000-0000-0000-0000-000000000011', '00000000-0000-0000-0000-000000000001', 'PRODUCT-A', 10, 15.50),
		                           			('00000000-0000-0000-0000-000000000012', '00000000-0000-0000-0000-000000000001', 'PRODUCT-B', 5, 25.00),
		                           			('00000000-0000-0000-0000-000000000013', '00000000-0000-0000-0000-000000000002', 'PRODUCT-A', 20, 15.50),
		                           			('00000000-0000-0000-0000-000000000014', '00000000-0000-0000-0000-000000000002', 'PRODUCT-C', 2, 100.00);
		                        """;
		await _factory.DbContainer.ExecScriptAsync(SQLShipment);
		await _factory.DbContainer.ExecScriptAsync(SQLItems);
	}
	
	internal class Product
	{
		public string productCode { get; set; }
		public int totalQuantity { get; set; }
	}

	internal class Root
	{
		public int customerID { get; set; }
		public int shipmentsCount { get; set; }
		public double totalAmount { get; set; }
		public List<Product> products { get; set; }
	}
}
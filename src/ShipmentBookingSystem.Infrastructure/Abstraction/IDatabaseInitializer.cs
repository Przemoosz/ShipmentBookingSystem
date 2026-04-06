namespace ShipmentBookingSystem.Infrastructure.Database;

public interface IDatabaseInitializer
{
	Task InitializeAsync();
}
namespace ShipmentBookingSystem.Infrastructure.Abstraction;

public interface IDatabaseInitializer
{
	Task InitializeAsync();
}
using ShipmentBookingSystem.Application.Validators;

namespace ShipmentBookingSystem.Application.Requests;

public record CreateShipmentRequest(
	string ShipmentNumber, 
	int CustomerId, 
	List<ShipmentItem> Items);

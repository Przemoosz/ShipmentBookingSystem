using ShipmentBookingSystem.Application.Validators;

namespace ShipmentBookingSystem.Application.Requests;

public sealed class SaveShipmentRequest
{
    public string ShipmentNumber { get; init; }
	public int CustomerId { get; init; }
	public List<ShipmentItem> Items { get; init; }

    private SaveShipmentRequest(string shipmentNumber, int customerId, List<ShipmentItem> shipmentItems)
    {
		ShipmentNumber = shipmentNumber;
		CustomerId = customerId;	
        Items = shipmentItems;
    }

	public static SaveShipmentRequest FromCreateRequest(CreateShipmentRequest request)
	{
		return new SaveShipmentRequest(request.ShipmentNumber, request.CustomerId, request.Items);
	}
}

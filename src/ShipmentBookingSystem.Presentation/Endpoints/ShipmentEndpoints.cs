using Microsoft.AspNetCore.Http;
using ShipmentBookingSystem.Application.Validators;
using Wolverine.Attributes;
using Wolverine.Http;

namespace ShipmentBookingSystem.Presentation.Endpoints;

public static class ShipmentEndpoints
{
	[WolverinePost("/shipments")]
	[Transactional] 
	public static async Task<IResult> Post(CreateShipment request)
		// CreateShipment command, 
		// IDbConnection conn, 
		// IMessageBus bus)
	{
		Console.WriteLine("created abc ");
		return Results.Created();
	}
}
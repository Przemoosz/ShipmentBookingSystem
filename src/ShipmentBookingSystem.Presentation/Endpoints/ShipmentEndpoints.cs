using System.Data;
using Dapper;
using Microsoft.AspNetCore.Http;
using ShipmentBookingSystem.Application.Interfaces;
using ShipmentBookingSystem.Application.Queries;
using ShipmentBookingSystem.Application.Requests;
using ShipmentBookingSystem.Application.Validators;
using ShipmentBookingSystem.Domain.Entities;
using ShipmentBookingSystem.Domain.Events;
using ShipmentBookingSystem.Domain.Models;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.Http;
using Wolverine.Persistence.Durability;

namespace ShipmentBookingSystem.Presentation.Endpoints;

public static class ShipmentEndpoints
{
	[WolverineGet("/shipments/summary")]
    public static async Task<IResult> Post(int customerId, DateTime createdFrom,
		DateTime createdTo, int minTotalAmount, int minShipments,
		IMessageBus messageBus)
	{
		var query = new ShipmentSummaryQuery
		{
			CustomerId = customerId,
			CreatedFrom = createdFrom,
			CreatedTo = createdTo,
			MinTotalAmount = minTotalAmount,
			MinShipments = minShipments
		};
		try
		{
            var summary = await messageBus.InvokeAsync<ShipmentSummary>(query);
            return Results.Ok(summary);
        }
		catch (Exception ex)
		{
			return Results.InternalServerError(ex);
		}
	}

    [WolverinePost("/shipments")]
    public static async Task<IResult> Get(CreateShipmentRequest request,
        IMessageBus messageBus, 
		CancellationToken cancellationToken)
    {
		SaveShipmentRequest saveShipmentRequest = SaveShipmentRequest.FromCreateRequest(request);
		try
		{
			await messageBus.InvokeAsync(saveShipmentRequest, cancellationToken);
		}
		catch (Exception ex)
		{ 
			return Results.InternalServerError(ex);
		}
        return Results.Created();
    }
}
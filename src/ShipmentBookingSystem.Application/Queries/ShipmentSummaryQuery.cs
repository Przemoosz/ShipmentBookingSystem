namespace ShipmentBookingSystem.Application.Queries;

public sealed record ShipmentSummaryQuery
{
    public int CustomerId { get; init; }
    public DateTime CreatedFrom { get; init; }
    public DateTime CreatedTo { get; init; }
    public int MinTotalAmount { get; init; }
    public int MinShipments { get; init; }
}

using FluentValidation;

namespace ShipmentBookingSystem.Application.Validators;

internal sealed class CreateShipmentValidator: AbstractValidator<CreateShipment>
{
	public CreateShipmentValidator()
	{
		ClassLevelCascadeMode = CascadeMode.Stop;
		RuleFor(x => x.ShipmentNumber).NotEmpty();
		RuleFor(x => x.Items).NotEmpty();
		RuleForEach(x => x.Items).ChildRules(item => {
			item.RuleFor(i => i.Quantity).GreaterThan(0);
			item.RuleFor(i => i.UnitPrice).GreaterThan(0);
		}); 
		// to do check if shipment does not exist
	}
}

public record CreateShipment(
	string ShipmentNumber, 
	int CustomerId, 
	List<ShipmentItemDto> Items);

public record ShipmentItemDto(string ProductCode, int Quantity, decimal UnitPrice);
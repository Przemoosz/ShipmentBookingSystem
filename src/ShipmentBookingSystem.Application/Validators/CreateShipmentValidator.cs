using FluentValidation;
using ShipmentBookingSystem.Application.Requests;

namespace ShipmentBookingSystem.Application.Validators;

internal sealed class CreateShipmentValidator: AbstractValidator<CreateShipmentRequest>
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

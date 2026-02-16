using FluentValidation;

namespace WWI_ModularKit.Modules.Sales.Features.Orders.CreateOrder;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one order line is required.");
        RuleForEach(x => x.Lines).SetValidator(new CreateOrderRequestLineValidator());
    }
}

public class CreateOrderRequestLineValidator : AbstractValidator<CreateOrderRequestLine>
{
    public CreateOrderRequestLineValidator()
    {
        RuleFor(x => x.StockItemId).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
    }
}

using FluentValidation;

namespace rentalApp.Models.Dtos.Validators;

public class CreatePropertyDtoValidator : AbstractValidator<CreatePropertyDto>
{
    public CreatePropertyDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PricePerNight).GreaterThan(0);
    }
}

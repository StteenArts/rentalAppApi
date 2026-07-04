using FluentValidation;

namespace rentalApp.Models.Dtos.Validators;

public class ToggleWishlistDtoValidator : AbstractValidator<ToggleWishlistDto>
{
    public ToggleWishlistDtoValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
    }
}

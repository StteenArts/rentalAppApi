using FluentValidation;

namespace rentalApp.Models.Dtos.Validators;

public class CreateReservationDtoValidator : AbstractValidator<CreateReservationDto>
{
    public CreateReservationDtoValidator()
    {
        RuleFor(x => x.PropertyId).NotEmpty();
        RuleFor(x => x.CheckIn).NotEmpty();
        RuleFor(x => x.CheckOut).NotEmpty();
        RuleFor(x => x.CheckIn)
            .Must(checkIn => checkIn.Date >= DateTime.UtcNow.Date)
            .WithMessage("CheckIn no puede ser una fecha pasada");
        RuleFor(x => x)
            .Must(x => x.CheckOut.Date > x.CheckIn.Date)
            .WithMessage("CheckOut must be after CheckIn")
            .WithName("CheckOut");
    }
}

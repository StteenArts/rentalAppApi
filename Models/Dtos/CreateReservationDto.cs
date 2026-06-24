namespace rentalApp.Models.Dtos;

public class CreateReservationDto
{
    public Guid PropertyId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}
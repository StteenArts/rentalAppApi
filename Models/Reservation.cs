using rentalApp.Models.Enum;

namespace rentalApp.Models;

public class Reservation
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }

    public Guid UserId { get; set; }

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    public decimal TotalAmount { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

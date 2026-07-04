using System.Text.Json.Serialization;
using rentalApp.Models.Enum;

namespace rentalApp.Models;

public class Reservation
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }

    [JsonIgnore]
    public Property? Property { get; set; }

    public Guid UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }

    public DateTime CheckIn { get; set; }

    public DateTime CheckOut { get; set; }

    public decimal TotalAmount { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Payment? Payment { get; set; }
}

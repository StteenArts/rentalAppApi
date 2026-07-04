using System.Text.Json.Serialization;
using rentalApp.Models.Enum;

namespace rentalApp.Models;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReservationId { get; set; }

    [JsonIgnore]
    public Reservation? Reservation { get; set; }

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
}

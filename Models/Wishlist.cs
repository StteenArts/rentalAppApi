namespace rentalApp.Models;

public class Wishlist
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PropertyId { get; set; }
    public Property? Property { get; set; }
}

namespace rentalApp.Models;

public class Property
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; }

    public string Description { get; set; }

    public string City { get; set; }

    public decimal PricePerNight { get; set; }

    public string ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public Guid OwnerId { get; set; }

    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
}
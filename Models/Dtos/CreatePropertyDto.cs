namespace rentalApp.Models.Dtos;

public class CreatePropertyDto
{
    public string Title { get; set; }

    public string Description { get; set; }

    public string City { get; set; }

    public decimal PricePerNight { get; set; }

    public string ImageUrl { get; set; }
}
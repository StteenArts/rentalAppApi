namespace rentalApp.Models.Dtos;

public class PropertyFilterDto
{
    public string? Location { get; set; }

    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
}
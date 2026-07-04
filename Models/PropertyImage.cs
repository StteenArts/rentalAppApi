using System.Text.Json.Serialization;

namespace rentalApp.Models;

public class PropertyImage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PropertyId { get; set; }

    [JsonIgnore]
    public Property? Property { get; set; }

    public string Url { get; set; }

    public bool IsMain { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

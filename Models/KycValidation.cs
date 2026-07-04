using rentalApp.Models.Enum;

namespace rentalApp.Models;

public class KycValidation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string FileName { get; set; }
    public byte[] EncryptedData { get; set; }

    // Simulated OCR/AI extraction output - see Services/KycService.cs
    public string? ExtractedFirstName { get; set; }
    public string? ExtractedLastName { get; set; }
    public string? ExtractedDocumentNumber { get; set; }
    public DateTime? ExtractedBirthDate { get; set; }

    public KycStatus Status { get; set; } = KycStatus.Pending;
    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

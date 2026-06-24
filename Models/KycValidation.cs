namespace rentalApp.Models;

public class KycValidation
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string FileName { get; set; }
    public byte[] EncryptedData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
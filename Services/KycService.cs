namespace rentalApp.Services;

public class KycVerificationResult
{
    public bool Approved { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DocumentNumber { get; set; }
    public DateTime? BirthDate { get; set; }
}

// Simula el veredicto de un proveedor de OCR/IA de verificación de identidad.
// TODO producción: reemplazar por una integración real (ver "Funcionalidades pendientes" en el README).
public class KycService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".pdf"];
    private const long MinFileSizeBytes = 1024;
    private const long MaxFileSizeBytes = 8 * 1024 * 1024;

    // Umbral usado solo para simular la confianza del veredicto de IA una vez el archivo
    // ya pasó el formato/tamaño válidos (ver IsValidDocument). No requiere leer el contenido.
    private const long ApprovalConfidenceThresholdBytes = 20 * 1024;

    // Validación de formato/tamaño que no requiere leer el contenido del archivo
    // (file.Length y la extensión están disponibles sin copiar el stream a memoria).
    public bool IsValidDocument(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return AllowedExtensions.Contains(extension)
            && file.Length >= MinFileSizeBytes
            && file.Length <= MaxFileSizeBytes;
    }

    // Debe llamarse solo después de que IsValidDocument haya devuelto true.
    public KycVerificationResult Simulate(IFormFile file)
    {
        var approved = file.Length >= ApprovalConfidenceThresholdBytes;

        if (!approved)
            return new KycVerificationResult { Approved = false };

        return new KycVerificationResult
        {
            Approved = true,
            FirstName = "Juan",
            LastName = "Pérez",
            DocumentNumber = $"CC-{Random.Shared.Next(10_000_000, 99_999_999)}",
            BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }
}

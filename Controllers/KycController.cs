using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rentalApp.Data;
using rentalApp.Extensions;
using rentalApp.Models;
using rentalApp.Models.Enum;
using rentalApp.Services;
using RentalApp.Services;

namespace rentalApp.Controllers;

[ApiController]
[Route("api/kyc")]
public class KycController : ControllerBase
{
    private readonly CryptoService _cryptoService;
    private readonly KycService _kycService;
    private readonly AppDbContext _context;
    private readonly NotificationService _notificationService;

    public KycController(
        CryptoService cryptoService,
        KycService kycService,
        AppDbContext context,
        NotificationService notificationService)
    {
        _cryptoService = cryptoService;
        _kycService = kycService;
        _context = context;
        _notificationService = notificationService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Problem(detail: "File is required", statusCode: StatusCodes.Status400BadRequest);

        if (!_kycService.IsValidDocument(file))
            return Problem(
                detail: "Unsupported file: allowed types are .jpg, .jpeg, .png, .pdf, between 1KB and 8MB",
                statusCode: StatusCodes.Status400BadRequest);

        var userId = User.GetUserId();

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var rawData = ms.ToArray();

        var verdict = _kycService.Simulate(file);

        var encrypted = _cryptoService.Encrypt(rawData);
        Array.Clear(rawData, 0, rawData.Length);

        var doc = new KycValidation
        {
            UserId = userId,
            FileName = file.FileName,
            EncryptedData = encrypted,
            ExtractedFirstName = verdict.FirstName,
            ExtractedLastName = verdict.LastName,
            ExtractedDocumentNumber = verdict.DocumentNumber,
            ExtractedBirthDate = verdict.BirthDate,
            Status = verdict.Approved ? KycStatus.Approved : KycStatus.Rejected,
            ReviewedAt = DateTime.UtcNow
        };

        _context.KycValidations.Add(doc);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        await _notificationService.SendAsync(
            userId,
            verdict.Approved ? "Verificación KYC aprobada" : "Verificación KYC rechazada",
            verdict.Approved
                ? "Tu documento fue verificado correctamente. Ya puedes reservar."
                : "No pudimos verificar tu documento. Intenta subir uno nuevo.",
            user!.Email
        );

        return StatusCode(StatusCodes.Status201Created, new
        {
            doc.Id,
            doc.Status,
            doc.ExtractedFirstName,
            doc.ExtractedLastName,
            doc.ExtractedDocumentNumber,
            doc.ExtractedBirthDate,
            doc.ReviewedAt
        });
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();

        var latest = await _context.KycValidations
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new
            {
                k.Id,
                k.Status,
                k.ExtractedFirstName,
                k.ExtractedLastName,
                k.ExtractedDocumentNumber,
                k.ExtractedBirthDate,
                k.CreatedAt,
                k.ReviewedAt
            })
            .FirstOrDefaultAsync();

        if (latest == null)
            return Problem(detail: "No KYC submission found", statusCode: StatusCodes.Status404NotFound);

        return Ok(latest);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var doc = await _context.KycValidations.FindAsync(id);

        if (doc == null)
            return Problem(detail: "Not found", statusCode: StatusCodes.Status404NotFound);

        var userId = User.GetUserId();
        if (doc.UserId != userId && !User.IsInRole("Admin"))
            return Problem(detail: "Forbidden", statusCode: StatusCodes.Status403Forbidden);

        // Sobrescribir antes de borrar para un borrado seguro de datos sensibles
        Array.Clear(doc.EncryptedData, 0, doc.EncryptedData.Length);

        _context.KycValidations.Remove(doc);
        await _context.SaveChangesAsync();

        return Ok("Securely deleted");
    }
}

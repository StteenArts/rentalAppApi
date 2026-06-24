using Microsoft.AspNetCore.Mvc;
using rentalApp.Data;
using rentalApp.Models;
using RentalApp.Services;

namespace rentalApp.Controllers;

public class KycController: ControllerBase
{
    private readonly CryptoService _cryptoService;
    private readonly AppDbContext _context;

    public KycController(CryptoService cryptoService, AppDbContext context)
    {
        _cryptoService = cryptoService;
        _context = context;
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null)
            return BadRequest();

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var rawData = ms.ToArray();

        // ENCRIPTAR
        var encrypted = _cryptoService.Encrypt(rawData);

        // guardar temporalmente (DB o storage)
        var doc = new KycValidation
        {
            UserId = Guid.NewGuid(), // reemplaza por usuario real
            FileName = file.FileName,
            EncryptedData = encrypted
        };

        _context.KycValidations.Add(doc);
        await _context.SaveChangesAsync();

        // eliminar archivo original (ya no existe porque fue memoria)
        Array.Clear(rawData, 0, rawData.Length);

        return Ok("File secured and processed");
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var doc = await _context.KycValidations.FindAsync(id);

        if (doc == null)
            return NotFound();

        // sobrescribir datos antes de borrar
        Array.Clear(doc.EncryptedData, 0, doc.EncryptedData.Length);

        _context.KycValidations.Remove(doc);
        await _context.SaveChangesAsync();

        return Ok("Securely deleted");
    }
}
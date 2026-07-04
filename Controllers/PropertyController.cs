using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rentalApp.Data;
using rentalApp.Extensions;
using rentalApp.Models;
using rentalApp.Models.Dtos;
using rentalApp.Models.Enum;

namespace RentalApp.Controllers;

[ApiController]
[Route("api/properties")]
public class PropertiesController : ControllerBase
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public PropertiesController(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    // GET: listar todo (público)
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var properties = await _context.Properties
            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .ToListAsync();

        return Ok(properties);
    }

    // GET: por id (público)
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var property = await _context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
            return Problem(detail: "Property not found", statusCode: StatusCodes.Status404NotFound);

        return Ok(property);
    }

    // GET: propiedades del owner autenticado
    [Authorize(Roles = "Owner")]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var ownerId = User.GetUserId();

        var properties = await _context.Properties
            .Include(p => p.Images)
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync();

        return Ok(properties);
    }

    // POST: crear propiedad (solo Owner/Admin, OwnerId viene del token)
    [Authorize(Roles = "Owner,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePropertyDto dto)
    {
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            City = dto.City,
            PricePerNight = dto.PricePerNight,
            ImageUrl = dto.ImageUrl,
            OwnerId = User.GetUserId()
        };

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, property);
    }

    // PUT: actualizar (solo el owner dueño o admin)
    [Authorize(Roles = "Owner,Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreatePropertyDto dto)
    {
        var property = await _context.Properties.FindAsync(id);

        if (property == null)
            return Problem(detail: "Property not found", statusCode: StatusCodes.Status404NotFound);

        if (property.OwnerId != User.GetUserId() && !User.IsInRole("Admin"))
            return Problem(detail: "You do not own this property", statusCode: StatusCodes.Status403Forbidden);

        property.Title = dto.Title;
        property.Description = dto.Description;
        property.City = dto.City;
        property.PricePerNight = dto.PricePerNight;
        property.ImageUrl = dto.ImageUrl;

        await _context.SaveChangesAsync();

        return Ok(property);
    }

    // DELETE (solo el owner dueño o admin)
    [Authorize(Roles = "Owner,Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var property = await _context.Properties.FindAsync(id);

        if (property == null)
            return Problem(detail: "Property not found", statusCode: StatusCodes.Status404NotFound);

        if (property.OwnerId != User.GetUserId() && !User.IsInRole("Admin"))
            return Problem(detail: "You do not own this property", statusCode: StatusCodes.Status403Forbidden);

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();

        return Ok("Deleted");
    }

    // Search (público) - filtra por ciudad y disponibilidad real en el rango de fechas
    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        string? city,
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = _context.Properties.Include(p => p.Images).Where(p => p.IsActive).AsQueryable();

        if (!string.IsNullOrEmpty(city))
            query = query.Where(x => x.City.Contains(city));

        if (startDate.HasValue && endDate.HasValue)
        {
            // Npgsql exige DateTimeKind.Utc para comparar contra columnas timestamptz
            var checkIn = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
            var checkOut = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);

            query = query.Where(p => !_context.Reservations.Any(r =>
                r.PropertyId == p.Id &&
                r.Status != ReservationStatus.Cancelled &&
                checkIn < r.CheckOut && checkOut > r.CheckIn));
        }

        var results = await query.ToListAsync();

        return Ok(results);
    }

    // Imágenes de propiedad
    [Authorize(Roles = "Owner,Admin")]
    [HttpPost("{id}/images")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        var property = await _context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null)
            return Problem(detail: "Property not found", statusCode: StatusCodes.Status404NotFound);

        if (property.OwnerId != User.GetUserId() && !User.IsInRole("Admin"))
            return Problem(detail: "You do not own this property", statusCode: StatusCodes.Status403Forbidden);

        if (file == null || file.Length == 0)
            return Problem(detail: "File is required", statusCode: StatusCodes.Status400BadRequest);

        if (file.Length > MaxImageSizeBytes)
            return Problem(detail: "File exceeds the 5MB limit", statusCode: StatusCodes.Status400BadRequest);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
            return Problem(detail: "Unsupported image type", statusCode: StatusCodes.Status400BadRequest);

        var propertyFolder = Path.Combine(_environment.WebRootPath, "uploads", "properties", id.ToString());
        Directory.CreateDirectory(propertyFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(propertyFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var image = new PropertyImage
        {
            PropertyId = id,
            Url = $"/uploads/properties/{id}/{fileName}",
            IsMain = property.Images.Count == 0
        };

        _context.PropertyImages.Add(image);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new { image.Id, image.Url, image.IsMain });
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpDelete("{id}/images/{imageId}")]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId)
    {
        var property = await _context.Properties.FindAsync(id);

        if (property == null)
            return Problem(detail: "Property not found", statusCode: StatusCodes.Status404NotFound);

        if (property.OwnerId != User.GetUserId() && !User.IsInRole("Admin"))
            return Problem(detail: "You do not own this property", statusCode: StatusCodes.Status403Forbidden);

        var image = await _context.PropertyImages.FirstOrDefaultAsync(i => i.Id == imageId && i.PropertyId == id);

        if (image == null)
            return Problem(detail: "Image not found", statusCode: StatusCodes.Status404NotFound);

        var filePath = Path.Combine(_environment.WebRootPath, image.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        _context.PropertyImages.Remove(image);
        await _context.SaveChangesAsync();

        return Ok("Deleted");
    }
}

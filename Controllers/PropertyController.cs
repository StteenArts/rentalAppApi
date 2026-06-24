using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rentalApp.Data;
using rentalApp.Models;
using rentalApp.Models.Dtos;

namespace RentalApp.Controllers;

[ApiController]
[Route("api/properties")]
public class PropertiesController : ControllerBase
{
    private readonly AppDbContext _context;

    public PropertiesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: listar todo
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var properties = await _context.Properties.ToListAsync();
        return Ok(properties);
    }

    // GET: por id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var property = await _context.Properties.FindAsync(id);

        if (property == null)
            return NotFound();

        return Ok(property);
    }

    // POST: crear propiedad
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
            ImageUrl = dto.ImageUrl
        };

        _context.Properties.Add(property);
        await _context.SaveChangesAsync();

        return Ok(property);
    }

    // PUT: actualizar
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, CreatePropertyDto dto)
    {
        var property = await _context.Properties.FindAsync(id);

        if (property == null)
            return NotFound();

        property.Title = dto.Title;
        property.Description = dto.Description;
        property.City = dto.City;
        property.PricePerNight = dto.PricePerNight;
        property.ImageUrl = dto.ImageUrl;

        await _context.SaveChangesAsync();

        return Ok(property);
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var property = await _context.Properties.FindAsync(id);

        if (property == null)
            return NotFound();

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();

        return Ok("Deleted");
    }
    
    // Search
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        string? city,
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = _context.Properties.AsQueryable();

        if (!string.IsNullOrEmpty(city))
            query = query.Where(x => x.City.Contains(city));

        var results = await query.ToListAsync();

        return Ok(results);
    }
}
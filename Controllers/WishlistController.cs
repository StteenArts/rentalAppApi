using Microsoft.AspNetCore.Mvc;
using rentalApp.Models.Dtos;
using rentalApp.Services;

namespace rentalApp.Controllers;

[ApiController]
[Route("api/wishlist")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _service;

    public WishlistController(IWishlistService service)
    {
        _service = service;
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle(ToggleWishlistDto dto)
    {
        var userId = Guid.NewGuid(); // luego Auth real

        var result = await _service.ToggleWishlist(userId, dto);

        return Ok(new
        {
            added = result
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = Guid.NewGuid();

        var list = await _service.GetWishlist(userId);

        return Ok(list);
    }

    [HttpDelete("{propertyId}")]
    public async Task<IActionResult> Remove(Guid propertyId)
    {
        var userId = Guid.NewGuid();

        await _service.Remove(userId, propertyId);

        return Ok();
    }
}
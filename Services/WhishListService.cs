using Microsoft.EntityFrameworkCore;
using rentalApp.Data;
using rentalApp.Models;
using rentalApp.Models.Dtos;

namespace rentalApp.Services;

public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;

    public WishlistService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ToggleWishlist(Guid userId, ToggleWishlistDto dto)
    {
        var existing = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.PropertyId == dto.PropertyId);

        if (existing != null)
        {
            _context.Wishlists.Remove(existing);
            await _context.SaveChangesAsync();
            return false; // removido
        }

        var wishlist = new Wishlist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PropertyId = dto.PropertyId
        };

        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync();

        return true; // agregado
    }
    
    public async Task<List<Wishlist>> GetWishlist(Guid userId)
    {
        return await _context.Wishlists
            .Include(w => w.Property)
            .ThenInclude(p => p!.Images)
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }

    public async Task Remove(Guid userId, Guid propertyId)
    {
        var item = await _context.Wishlists
            .FirstOrDefaultAsync(x => x.UserId == userId && x.PropertyId == propertyId);

        if (item != null)
        {
            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}

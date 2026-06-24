using rentalApp.Models;
using rentalApp.Models.Dtos;

namespace rentalApp.Services;

public interface IWishlistService
{
    Task<bool> ToggleWishlist(Guid userId, ToggleWishlistDto dto);

    Task<List<Wishlist>> GetWishlist(Guid userId);

    Task Remove(Guid userId, Guid propertyId);
}

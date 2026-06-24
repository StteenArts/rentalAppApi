using Microsoft.EntityFrameworkCore;
using rentalApp.Models;

namespace rentalApp.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Property> Properties { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<KycValidation>  KycValidations { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    
    public DbSet<Notification> Notifications { get; set; }
    
}
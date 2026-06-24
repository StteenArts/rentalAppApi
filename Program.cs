using Microsoft.EntityFrameworkCore;
using rentalApp.Data;
using rentalApp.Services;
using RentalApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<CryptoService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();
//Sedeer fake to property
/*using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Properties.Any())
    {
        db.Properties.AddRange(
            new Property
            {
                Title = "Beach House",
                Description = "Beautiful house near the beach",
                City = "Cartagena",
                PricePerNight = 120,
                ImageUrl = "https://example.com/1.jpg"
            },
            new Property
            {
                Title = "Mountain Cabin",
                Description = "Cabin in the mountains",
                City = "Medellín",
                PricePerNight = 80,
                ImageUrl = "https://example.com/2.jpg"
            }
        );

        db.SaveChanges();
    }
}*/

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
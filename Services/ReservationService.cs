using Microsoft.EntityFrameworkCore;
using rentalApp.Data;
using rentalApp.Models;
using rentalApp.Models.Dtos;
using rentalApp.Models.Enum;

namespace rentalApp.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;

    public ReservationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation> CreateReservation(Guid userId, CreateReservationDto dto)
    {
        // Search Property
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == dto.PropertyId);

        if (property == null)
            throw new Exception("Property not found");

        // Validate (double booking)
        var hasConflict = await _context.Reservations.AnyAsync(r =>
            r.PropertyId == dto.PropertyId &&
            r.CheckIn < dto.CheckOut &&
            dto.CheckIn < r.CheckOut
        );

        if (hasConflict)
            throw new Exception("Property already booked in these dates");

        //  Force Hourly estándar
        var checkIn = dto.CheckIn.Date.AddHours(14);   // 2 PM
        var checkOut = dto.CheckOut.Date.AddHours(12); // 12 PM

        //  create reservation
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PropertyId = dto.PropertyId,
            CheckIn = checkIn,
            CheckOut = checkOut,
            Status = ReservationStatus.Confirmed,
            TotalAmount = property.PricePerNight * (checkOut - checkIn).Days
        };

        //  save en DB
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }
    
    public async Task<List<Reservation>> GetByUserId(Guid userId)
    {
        return await _context.Reservations
            .Where(x => x.UserId == userId)
            .ToListAsync();
    }
}
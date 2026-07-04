using Microsoft.EntityFrameworkCore;
using rentalApp.Data;
using rentalApp.Exceptions;
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
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == dto.PropertyId);

        if (property == null)
            throw new DomainException(StatusCodes.Status404NotFound, "Property not found");

        var isKycApproved = await _context.KycValidations
            .AnyAsync(k => k.UserId == userId && k.Status == KycStatus.Approved);

        if (!isKycApproved)
            throw new DomainException(StatusCodes.Status403Forbidden, "You must complete KYC verification before making a reservation");

        // Force el rango de fechas a horarios estándar de check-in/check-out antes de validar solapamiento
        // Se normaliza a UTC porque Npgsql exige DateTimeKind.Utc para columnas timestamptz
        var checkIn = DateTime.SpecifyKind(dto.CheckIn.Date.AddHours(14), DateTimeKind.Utc);   // 2 PM
        var checkOut = DateTime.SpecifyKind(dto.CheckOut.Date.AddHours(12), DateTimeKind.Utc); // 12 PM

        if (checkOut <= checkIn)
            throw new DomainException(StatusCodes.Status400BadRequest, "CheckOut must be after CheckIn");

        var hasConflict = await _context.Reservations.AnyAsync(r =>
            r.PropertyId == dto.PropertyId &&
            r.Status != ReservationStatus.Cancelled &&
            r.CheckIn < checkOut &&
            checkIn < r.CheckOut
        );

        if (hasConflict)
            throw new DomainException(StatusCodes.Status409Conflict, "Property already booked in these dates");

        var nights = (dto.CheckOut.Date - dto.CheckIn.Date).Days;

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PropertyId = dto.PropertyId,
            CheckIn = checkIn,
            CheckOut = checkOut,
            Status = ReservationStatus.Confirmed,
            TotalAmount = property.PricePerNight * nights
        };

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

    public async Task<List<Reservation>> GetByOwnerId(Guid ownerId, bool isAdmin)
    {
        return await _context.Reservations
            .Where(r => isAdmin || r.Property!.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<Reservation> CancelReservation(Guid userId, Guid reservationId, bool isAdmin)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);

        if (reservation == null)
            throw new DomainException(StatusCodes.Status404NotFound, "Reservation not found");

        if (reservation.UserId != userId && !isAdmin)
            throw new DomainException(StatusCodes.Status403Forbidden, "You do not own this reservation");

        if (reservation.Status == ReservationStatus.Cancelled)
            throw new DomainException(StatusCodes.Status400BadRequest, "Reservation is already cancelled");

        reservation.Status = ReservationStatus.Cancelled;
        await _context.SaveChangesAsync();

        return reservation;
    }
}

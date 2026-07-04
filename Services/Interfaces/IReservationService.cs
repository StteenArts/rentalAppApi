using rentalApp.Models;
using rentalApp.Models.Dtos;

namespace rentalApp.Services;

public interface IReservationService
{
    Task<Reservation> CreateReservation(Guid userId, CreateReservationDto dto);
    Task<List<Reservation>> GetByUserId(Guid userId);
    Task<List<Reservation>> GetByOwnerId(Guid ownerId, bool isAdmin);
    Task<Reservation> CancelReservation(Guid userId, Guid reservationId, bool isAdmin);
}

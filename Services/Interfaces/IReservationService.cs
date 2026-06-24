using rentalApp.Models;
using rentalApp.Models.Dtos;

namespace rentalApp.Services;

public interface IReservationService
{
    Task<Reservation> CreateReservation(Guid userId, CreateReservationDto dto);
    Task<List<Reservation>> GetByUserId(Guid userId);
}
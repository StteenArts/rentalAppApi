using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rentalApp.Data;
using rentalApp.Extensions;
using rentalApp.Models.Dtos;
using rentalApp.Services;
using RentalApp.Services;

namespace rentalApp.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly NotificationService _notificationService;
    private readonly AppDbContext _context;

    public ReservationController(
        IReservationService reservationService,
        NotificationService notificationService,
        AppDbContext context)
    {
        _reservationService = reservationService;
        _notificationService = notificationService;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation(CreateReservationDto dto)
    {
        var userId = User.GetUserId();

        var result = await _reservationService.CreateReservation(userId, dto);

        var user = await _context.Users.FindAsync(userId);
        await _notificationService.SendAsync(
            userId,
            "Reserva confirmada",
            "Tu reserva fue confirmada correctamente.",
            user!.Email
        );

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> MyReservations()
    {
        var userId = User.GetUserId();

        var reservations = await _reservationService.GetByUserId(userId);

        return Ok(reservations);
    }

    [Authorize(Roles = "Owner,Admin")]
    [HttpGet("owner")]
    public async Task<IActionResult> OwnerReservations()
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var reservations = await _reservationService.GetByOwnerId(userId, isAdmin);

        return Ok(reservations);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = User.GetUserId();
        var isAdmin = User.IsInRole("Admin");

        var reservation = await _reservationService.CancelReservation(userId, id, isAdmin);

        var user = await _context.Users.FindAsync(reservation.UserId);
        await _notificationService.SendAsync(
            reservation.UserId,
            "Reserva cancelada",
            "Tu reserva fue cancelada.",
            user!.Email
        );

        return Ok(reservation);
    }
}

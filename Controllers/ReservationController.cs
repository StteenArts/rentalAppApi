using Microsoft.AspNetCore.Mvc;
using rentalApp.Models;
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
    private readonly UserService _userService;

    public ReservationController(
        IReservationService reservationService,
        NotificationService notificationService,
        UserService userService)
    {
        _reservationService = reservationService;
        _notificationService = notificationService;
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation(CreateReservationDto dto, string email)
    {
        var user = await _userService.GetUserByEmail(email);

        var result = await _reservationService.CreateReservation(user.Id, dto);

        await _notificationService.SendAsync(
            user.Id,
            "Reserva confirmada",
            "Tu reserva fue confirmada correctamente.",
            user.Email
        );

        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> MyReservations(string email)
    {
        var user = await _userService.GetUserByEmail(email);

        var reservations = await _reservationService.GetByUserId(user.Id);

        return Ok(reservations);
    }
}
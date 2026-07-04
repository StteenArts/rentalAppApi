using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rentalApp.Data;
using rentalApp.Extensions;

namespace rentalApp.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = User.GetUserId();

        var notifications = await _context.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPut("read/{id}")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = User.GetUserId();
        var notification = await _context.Notifications.FindAsync(id);

        if (notification == null)
            return Problem(detail: "Notification not found", statusCode: StatusCodes.Status404NotFound);

        if (notification.UserId != userId)
            return Problem(detail: "You do not own this notification", statusCode: StatusCodes.Status403Forbidden);

        notification.IsRead = true;

        await _context.SaveChangesAsync();

        return Ok(notification);
    }
}

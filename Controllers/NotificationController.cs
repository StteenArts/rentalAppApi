using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rentalApp.Data;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserNotifications(Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPut("read/{id}")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var notification = await _context.Notifications.FindAsync(id);

        if (notification == null)
            return NotFound();

        notification.IsRead = true;

        await _context.SaveChangesAsync();

        return Ok();
    }
}
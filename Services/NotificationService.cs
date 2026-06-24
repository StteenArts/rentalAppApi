using rentalApp.Data;
using rentalApp.Models;

namespace RentalApp.Services;

public class NotificationService
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public NotificationService(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task SendAsync(Guid userId, string title, string message, string email)
    {
        // 1. Guardar notificación interna
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message
        };

        _context.Notifications.Add(notification);

        // 2. Enviar email
        await _emailService.SendEmailAsync(email, title, message);

        await _context.SaveChangesAsync();
    }
}
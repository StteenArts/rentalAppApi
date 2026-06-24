namespace RentalApp.Services;

public class EmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Simulación (en prueba técnica esto es suficiente)
        Console.WriteLine("==== EMAIL SENT ====");
        Console.WriteLine($"To: {to}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {body}");
        Console.WriteLine("===================");

        return Task.CompletedTask;
    }
}
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rentalApp.Data;
using rentalApp.Extensions;
using rentalApp.Models.Enum;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Owner,Admin")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> Metrics(DateTime? startDate, DateTime? endDate)
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = User.GetUserId();

        var propertiesQuery = _context.Properties.AsQueryable();
        if (!isAdmin)
            propertiesQuery = propertiesQuery.Where(p => p.OwnerId == userId);

        var reservationsQuery = _context.Reservations
            .Where(r => isAdmin || r.Property!.OwnerId == userId);

        if (startDate.HasValue)
        {
            var from = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
            reservationsQuery = reservationsQuery.Where(r => r.CheckIn >= from);
        }

        if (endDate.HasValue)
        {
            var to = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
            reservationsQuery = reservationsQuery.Where(r => r.CheckOut <= to);
        }

        var totalProperties = await propertiesQuery.CountAsync();
        var totalReservations = await reservationsQuery.CountAsync();
        var activeReservations = await reservationsQuery.CountAsync(r => r.Status == ReservationStatus.Confirmed);
        var totalIncome = await reservationsQuery
            .Where(r => r.Status != ReservationStatus.Cancelled)
            .SumAsync(r => (decimal?)r.TotalAmount) ?? 0;

        var stayRanges = await reservationsQuery
            .Where(r => r.Status != ReservationStatus.Cancelled)
            .Select(r => new { r.CheckIn, r.CheckOut })
            .ToListAsync();

        var bookedNights = stayRanges.Sum(r => (r.CheckOut - r.CheckIn).TotalDays);

        var occupancyRate = totalProperties == 0
            ? 0
            : Math.Round(bookedNights / (totalProperties * 30.0) * 100, 2);

        return Ok(new
        {
            totalProperties,
            totalReservations,
            activeReservations,
            totalIncome,
            occupancyRate
        });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(Guid? propertyId, DateTime? startDate, DateTime? endDate)
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = User.GetUserId();

        var query = _context.Reservations
            .Include(r => r.Property)
            .Include(r => r.User)
            .Where(r => isAdmin || r.Property!.OwnerId == userId);

        if (propertyId.HasValue)
        {
            var property = await _context.Properties.FindAsync(propertyId.Value);
            if (property == null)
                return Problem(detail: "Property not found", statusCode: StatusCodes.Status404NotFound);

            if (!isAdmin && property.OwnerId != userId)
                return Problem(detail: "You do not own this property", statusCode: StatusCodes.Status403Forbidden);

            query = query.Where(r => r.PropertyId == propertyId.Value);
        }

        if (startDate.HasValue)
        {
            var from = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
            query = query.Where(r => r.CheckIn >= from);
        }

        if (endDate.HasValue)
        {
            var to = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
            query = query.Where(r => r.CheckOut <= to);
        }

        var data = await query.OrderBy(r => r.CheckIn).ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Reservations");

        sheet.Cell(1, 1).Value = "Property";
        sheet.Cell(1, 2).Value = "Guest";
        sheet.Cell(1, 3).Value = "Price";
        sheet.Cell(1, 4).Value = "CheckIn";
        sheet.Cell(1, 5).Value = "CheckOut";
        sheet.Cell(1, 6).Value = "Status";

        int row = 2;

        foreach (var r in data)
        {
            sheet.Cell(row, 1).Value = r.Property?.Title;
            sheet.Cell(row, 2).Value = r.User?.Email;
            sheet.Cell(row, 3).Value = r.TotalAmount;
            sheet.Cell(row, 4).Value = r.CheckIn;
            sheet.Cell(row, 5).Value = r.CheckOut;
            sheet.Cell(row, 6).Value = r.Status.ToString();
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "report.xlsx");
    }
}

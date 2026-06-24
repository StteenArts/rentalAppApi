using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rentalApp.Data;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> Metrics()
    {
        var totalProperties = await _context.Properties.CountAsync();
        var totalReservations = await _context.Reservations.CountAsync();
        var totalIncome = await _context.Reservations.SumAsync(x => x.TotalAmount);

        return Ok(new
        {
            totalProperties,
            totalReservations,
            totalIncome
        });
    }
    
    
    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var data = await _context.Reservations
            .Include(x => x.PropertyId)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Reservations");

        sheet.Cell(1, 1).Value = "Property";
        sheet.Cell(1, 2).Value = "Price";
        sheet.Cell(1, 3).Value = "CheckIn";
        sheet.Cell(1, 4).Value = "CheckOut";

        int row = 2;

        foreach (var r in data)
        {
            sheet.Cell(row, 1).Value = r.PropertyId.ToString();
            sheet.Cell(row, 2).Value = r.TotalAmount;
            sheet.Cell(row, 3).Value = r.CheckIn;
            sheet.Cell(row, 4).Value = r.CheckOut;
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
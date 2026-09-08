using Microsoft.AspNetCore.Mvc;
using Online_Shopping_System.Services;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Online_Shopping_System.Models.Carts;
using Online_Shopping_System.Models.Orders;
using Online_Shopping_System.Models.Payment;
using Online_Shopping_System.Models.Users;
using Online_Shopping_System.Data;
using System.Security.Claims;

[ApiController]
[Route("[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
        QuestPDF.Settings.License = LicenseType.Evaluation;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("orders/excel")]
    public async Task<IActionResult> OrdersExcel()
    {
        try
        {
            var file = await _reportService.GenerateOrdersExcelAsync();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Orders-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

    }


    [Authorize(Roles = "Admin")]
    [HttpGet("orders/pdf")]
    public async Task<IActionResult> OrdersPdf()
    {
        try
        {
            var file = await _reportService.GenerateOrdersPdfAsync();

            return File(
                file,
                "application/pdf",
                $"Orders-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
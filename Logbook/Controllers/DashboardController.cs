using Microsoft.AspNetCore.Mvc;
using Logbook.Services;
using Logbook.Models;
using Logbook.ViewModels;

namespace Logbook.Controllers;

public class DashboardController : Controller
{
    private readonly IJobApplicationService _service;
    private readonly PdfExportService _pdfExport;
    private readonly string _jobseekerName;

    public DashboardController(
    IJobApplicationService service,
    PdfExportService pdfExport,
    IConfiguration configuration)

    {
        _service = service;
        _pdfExport = pdfExport;
        _jobseekerName = configuration["Logbook:JobseekerName"] ?? "Jobseeker";
    }

    // GET /Dashboard
    // Optional query params: startDate, endDate (defaults to current week Mon-Sun)
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        var start = startDate ?? GetMondayOfCurrentWeek();
        var end = endDate ?? start.AddDays(6);

        var applications = await _service.GetByDateRangeAsync(start, end);
        var appList = applications.ToList();

        var viewModel = new DashboardViewModel
        {
            StartDate = start,
            EndDate = end,
            TotalApplications = appList.Count,
            StatusBreakdown = appList
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return View(viewModel);
    }

    // GET /Dashboard/Export?startDate=...&endDate=...
    public async Task<IActionResult> Export(DateTime? startDate, DateTime? endDate)
    {
        var start = startDate ?? GetMondayOfCurrentWeek();
        var end = endDate ?? start.AddDays(6);

        var applications = await _service.GetByDateRangeAsync(start, end);

        var pdfBytes = _pdfExport.GenerateWeeklyReport(
            applications, start, end, _jobseekerName);

        var fileName = $"Logbook_{start:yyyy-MM-dd}_{end:yyyy-MM-dd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    // Returns the Monday of the current week
    private static DateTime GetMondayOfCurrentWeek()
    {
        var today = DateTime.Today;
        var daysFromMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return today.AddDays(-daysFromMonday);
    }
}
using Microsoft.AspNetCore.Mvc;
using Logbook.Services;
using Logbook.Models;
using Logbook.ViewModels;

namespace Logbook.Controllers;

public class DashboardController : Controller
{
    private readonly IJobApplicationService _service;

    public DashboardController(IJobApplicationService service)
    {
        _service = service;
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

    // Returns the Monday of the current week
    private static DateTime GetMondayOfCurrentWeek()
    {
        var today = DateTime.Today;
        var daysFromMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return today.AddDays(-daysFromMonday);
    }
}
using Microsoft.AspNetCore.Mvc;
using Logbook.Services;

namespace Logbook.Controllers;

public class ApplicationsController : Controller
{
    private readonly IJobApplicationService _service;

    public ApplicationsController(IJobApplicationService service)
    {
        _service = service;
    }

    // GET /Applications
    public async Task<IActionResult> Index()
    {
        var applications = await _service.GetAllAsync();
        return View(applications);
    }
}

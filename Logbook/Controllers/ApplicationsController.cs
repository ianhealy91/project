using Microsoft.AspNetCore.Mvc;
using Logbook.Services;
using Logbook.Models;
using Logbook.ViewModels;

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
    // GET /Applications/Create
    public IActionResult Create()
    {
        return View(new AddEditViewModel());
    }

    // POST /Applications/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AddEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var application = new JobApplication
        {
            CompanyName = model.CompanyName,
            RoleTitle = model.RoleTitle,
            DateApplied = model.DateApplied,
            Source = model.Source,
            Status = model.Status,
            Notes = model.Notes
        };

        await _service.AddAsync(application);

        TempData["SuccessMessage"] = $"Application to {application.CompanyName} added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Applications/Details
    public async Task<IActionResult> Details(int id)
    {
        var application = await _service.GetByIdAsync(id);
        if (application is null)
            return NotFound();

        return View(application);
    }
}

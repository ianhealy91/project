using Microsoft.AspNetCore.Mvc;
using Logbook.Models;
using Logbook.Services;
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
    public async Task<IActionResult> Index(ApplicationStatus? status, string? search, string? sortBy)
    {
        var applications = await _service.GetFilteredAsync(status, search, sortBy);
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentSearch = search ?? string.Empty;
        ViewBag.CurrentSort = sortBy ?? string.Empty;
        return View(applications);
    }

    // GET /Applications/Details/5
    public async Task<IActionResult> Details(int id, ApplicationStatus? status, string? search, string? sortBy)
    {
        var application = await _service.GetByIdAsync(id);
        if (application is null) return NotFound();

        var returnUrl = Url.Action("Index", new { status, search, sortBy });
        ViewBag.ReturnUrl = returnUrl;

        return View(application);
    }

    // GET /Applications/Create
    public IActionResult Create()
    {
        return View(new AddEditViewModel { DateApplied = DateTime.Today });
    }

    // POST /Applications/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AddEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _service.AddAsync(new JobApplication
        {
            CompanyName = model.CompanyName,
            RoleTitle = model.RoleTitle,
            DateApplied = model.DateApplied,
            Source = model.Source,
            Status = model.Status,
            Notes = model.Notes,
            FollowUpDate = model.FollowUpDate
        });

        TempData["SuccessMessage"] = "Application added successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Applications/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var application = await _service.GetByIdAsync(id);
        if (application is null) return NotFound();

        return View(new AddEditViewModel
        {
            Id = application.Id,
            CompanyName = application.CompanyName,
            RoleTitle = application.RoleTitle,
            DateApplied = application.DateApplied,
            Source = application.Source,
            Status = application.Status,
            Notes = application.Notes,
            FollowUpDate = application.FollowUpDate
        });
    }

    // POST /Applications/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AddEditViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var result = await _service.UpdateAsync(new JobApplication
        {
            Id = model.Id,
            CompanyName = model.CompanyName,
            RoleTitle = model.RoleTitle,
            DateApplied = model.DateApplied,
            Source = model.Source,
            Status = model.Status,
            Notes = model.Notes,
            FollowUpDate = model.FollowUpDate
        });

        if (result is null) return NotFound();

        TempData["SuccessMessage"] = "Application updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    // GET /Applications/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var application = await _service.GetByIdAsync(id);
        if (application is null) return NotFound();
        return View(application);
    }

    // POST /Applications/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _service.DeleteAsync(id);
        TempData["SuccessMessage"] = "Application deleted.";
        return RedirectToAction(nameof(Index));
    }
}
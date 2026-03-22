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

    // GET /Applications/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var application = await _service.GetByIdAsync(id);
        if (application is null)
            return NotFound();

        return View(application);
    }

    // GET /Applications/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var application = await _service.GetByIdAsync(id);
        if (application is null)
            return NotFound();

        var model = new AddEditViewModel
        {
            Id = application.Id,
            CompanyName = application.CompanyName,
            RoleTitle = application.RoleTitle,
            DateApplied = application.DateApplied,
            Source = application.Source,
            Status = application.Status,
            Notes = application.Notes
        };

        return View(model);
    }

    // POST /Applications/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AddEditViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var application = new JobApplication
        {
            Id = model.Id,
            CompanyName = model.CompanyName,
            RoleTitle = model.RoleTitle,
            DateApplied = model.DateApplied,
            Source = model.Source,
            Status = model.Status,
            Notes = model.Notes
        };

        var result = await _service.UpdateAsync(application);
        if (result is null)
            return NotFound();

        TempData["SuccessMessage"] = $"Application to {application.CompanyName} updated successfully.";
        return RedirectToAction(nameof(Details), new { id = application.Id });
    }

    // GET /Applications/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var application = await _service.GetByIdAsync(id);
        if (application is null)
            return NotFound();

        return View(application);
    }

    // POST /Applications/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var application = await _service.GetByIdAsync(id);
        var companyName = application?.CompanyName ?? "the application";

        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        TempData["SuccessMessage"] = $"Application to {companyName} deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}

using Microsoft.AspNetCore.Mvc;

namespace Logbook.Controllers;

public class DashboardController : Controller
{
    // GET /Dashboard
    public IActionResult Index()
    {
        return View();
    }
}

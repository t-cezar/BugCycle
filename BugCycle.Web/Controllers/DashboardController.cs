using BugCycle.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _dashboard.GetForCurrentUserAsync(ct));
}

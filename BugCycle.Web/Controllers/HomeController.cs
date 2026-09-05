using System.Diagnostics;
using BugCycle.Web.Models;
using BugCycle.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Un utilizator deja autentificat nu are ce sa caute pe landing page.
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }

    public IActionResult Problem(int status = 500, string? message = null)
    {
        Response.StatusCode = status;

        return View(new ProblemViewModel
        {
            Status = status,
            Message = string.IsNullOrWhiteSpace(message)
                ? "A aparut o eroare neasteptata."
                : message
        });
    }

    public IActionResult AccessDenied()
        => View("Problem", new ProblemViewModel
        {
            Status = StatusCodes.Status403Forbidden,
            Message = "Nu ai permisiunea sa accesezi aceasta pagina."
        });

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
}

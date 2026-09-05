using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Domain.Constants;
using BugCycle.Domain.Entities;
using BugCycle.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IProjectMemberRepository _members;
    private readonly IIssueRepository _issues;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IProjectMemberRepository members,
        IIssueRepository issues,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _members = members;
        _issues = issues;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FullName = model.FullName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            // Erorile venite de la Identity se transforma in erori per camp,
            // ca sa apara langa input-ul potrivit si nu doar in sumar.
            foreach (var error in result.Errors)
            {
                var field = error.Code.Contains("Password") ? nameof(model.Password)
                    : error.Code.Contains("Email") || error.Code.Contains("UserName") ? nameof(model.Email)
                    : string.Empty;

                ModelState.AddModelError(field, error.Description);
            }

            return View(model);
        }

        // Fiecare cont primeste rolul de baza User; Admin se acorda ulterior din panoul de administrare
        await _userManager.AddToRoleAsync(user, AppRoles.User);

        await _signInManager.SignInAsync(user, isPersistent: true);

        _logger.LogInformation("Cont nou inregistrat: {Email}", model.Email);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Email sau parola incorecte.");
            return View(model);
        }

        _logger.LogInformation("Autentificare reusita pentru {Email}", model.Email);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        return View(await BuildProfileAsync(user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            var invalid = await BuildProfileAsync(user);
            invalid.FullName = model.FullName;
            return View(invalid);
        }

        user.FullName = model.FullName.Trim();
        await _userManager.UpdateAsync(user);

        // Numele apare in cookie prin claims, deci sesiunea trebuie
        // reincarcata ca schimbarea sa fie vizibila imediat in interfata.
        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] = "Profilul a fost actualizat.";

        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ChangePasswordAsync(
            user, model.CurrentPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                var field = error.Code.Contains("Password") && error.Code.Contains("Mismatch")
                    ? nameof(model.CurrentPassword)
                    : nameof(model.NewPassword);

                ModelState.AddModelError(field, error.Description);
            }

            return View(model);
        }

        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] = "Parola a fost schimbata.";

        return RedirectToAction(nameof(Profile));
    }

    private async Task<ProfileViewModel> BuildProfileAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var projectCounts = await _members.CountProjectsPerUserAsync(new[] { user.Id });
        var assigned = await _issues.ListAssignedToUserAsync(user.Id);

        return new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Initials = user.Initials,
            CreatedAt = user.CreatedAt,
            Roles = roles.OrderBy(r => r).ToList(),
            ProjectCount = projectCounts.TryGetValue(user.Id, out var c) ? c : 0,
            AssignedIssueCount = assigned.Count
        };
    }
}

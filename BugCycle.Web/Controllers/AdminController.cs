using BugCycle.Application.DTOs.Labels;
using BugCycle.Application.Interfaces;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Domain.Constants;
using BugCycle.Domain.Exceptions;
using BugCycle.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
[Route("Admin")]
public class AdminController : Controller
{
    private readonly IUserAdminService _users;
    private readonly ILabelService _labels;
    private readonly IProjectService _projects;
    private readonly ICurrentUserService _currentUser;

    public AdminController(
        IUserAdminService users,
        ILabelService labels,
        IProjectService projects,
        ICurrentUserService currentUser)
    {
        _users = users;
        _labels = labels;
        _projects = projects;
        _currentUser = currentUser;
    }

    [HttpGet("")]
    public IActionResult Index() => RedirectToAction(nameof(Users));

    [HttpGet("Users")]
    public async Task<IActionResult> Users(CancellationToken ct)
        => View(new AdminUsersViewModel
        {
            Users = await _users.ListAsync(ct),
            AllRoles = await _users.ListRolesAsync(ct),
            CurrentUserId = _currentUser.UserId
        });

    [HttpPost("Users/{id}/Roles/Add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRole(string id, string role, CancellationToken ct)
    {
        try
        {
            await _users.AddRoleAsync(id, role, ct);
            TempData["Success"] = $"Rolul {role} a fost acordat.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpPost("Users/{id}/Roles/Remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRole(string id, string role, CancellationToken ct)
    {
        try
        {
            await _users.RemoveRoleAsync(id, role, ct);
            TempData["Success"] = $"Rolul {role} a fost retras.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpGet("Labels")]
    public async Task<IActionResult> Labels(int? editId, CancellationToken ct = default)
    {
        var labels = await _labels.ListAsync(true, ct);

        var editing = editId.HasValue
            ? labels.FirstOrDefault(l => l.Id == editId.Value)
            : null;

        return View(new AdminLabelsViewModel
        {
            Labels = labels,
            EditingLabelId = editing?.Id,
            NewLabel = editing is null
                ? new SaveLabelDto()
                : new SaveLabelDto { Name = editing.Name, ColorHex = editing.ColorHex }
        });
    }

    [HttpPost("Labels")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveLabel(
        [Bind(Prefix = nameof(AdminLabelsViewModel.NewLabel))] SaveLabelDto dto,
        int? editId,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Labels), new AdminLabelsViewModel
            {
                Labels = await _labels.ListAsync(true, ct),
                NewLabel = dto,
                EditingLabelId = editId
            });
        }

        try
        {
            if (editId.HasValue)
            {
                await _labels.UpdateAsync(editId.Value, dto, ct);
                TempData["Success"] = "Eticheta a fost actualizata.";
            }
            else
            {
                await _labels.CreateAsync(dto, ct);
                TempData["Success"] = "Eticheta a fost creata.";
            }
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(
                $"{nameof(AdminLabelsViewModel.NewLabel)}.{nameof(dto.Name)}",
                ex.Message);

            return View(nameof(Labels), new AdminLabelsViewModel
            {
                Labels = await _labels.ListAsync(true, ct),
                NewLabel = dto,
                EditingLabelId = editId
            });
        }

        return RedirectToAction(nameof(Labels));
    }

    [HttpPost("Labels/{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLabel(int id, CancellationToken ct)
    {
        await _labels.DeleteAsync(id, ct);
        TempData["Success"] = "Eticheta a fost stearsa de pe toate issue-urile.";

        return RedirectToAction(nameof(Labels));
    }

    [HttpGet("Projects")]
    public async Task<IActionResult> Projects(bool includeArchived = true, CancellationToken ct = default)
        => View(new AdminProjectsViewModel
        {
            Projects = await _projects.ListAllForAdminAsync(includeArchived, ct),
            IncludeArchived = includeArchived
        });

    [HttpPost("Projects/{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProject(int id, CancellationToken ct)
    {
        await _projects.DeleteAsync(id, ct);
        TempData["Success"] = "Proiectul si tot continutul lui au fost sterse.";

        return RedirectToAction(nameof(Projects));
    }
}

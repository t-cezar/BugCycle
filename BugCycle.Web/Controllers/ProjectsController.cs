using BugCycle.Application.DTOs.Issues;
using BugCycle.Application.DTOs.Projects;
using BugCycle.Application.DTOs.Sprints;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Domain.Enums;
using BugCycle.Domain.Exceptions;
using BugCycle.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers;

[Route("Projects")]
public class ProjectsController : Controller
{
    private readonly IProjectService _projects;
    private readonly ISprintService _sprints;
    private readonly IIssueService _issues;
    private readonly ILabelService _labels;

    public ProjectsController(
        IProjectService projects,
        ISprintService sprints,
        IIssueService issues,
        ILabelService labels)
    {
        _projects = projects;
        _sprints = sprints;
        _issues = issues;
        _labels = labels;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(bool includeArchived = false, CancellationToken ct = default)
        => View(new ProjectListViewModel
        {
            Projects = await _projects.ListForCurrentUserAsync(includeArchived, ct),
            IncludeArchived = includeArchived
        });

    [HttpGet("Create")]
    public IActionResult Create() => View(new SaveProjectDto());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaveProjectDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var created = await _projects.CreateAsync(dto, ct);
            return RedirectToAction(nameof(Backlog), new { id = created.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(nameof(dto.Key), ex.Message);
            return View(dto);
        }
    }

    [HttpGet("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(id, ct);

        ViewData["ProjectId"] = id;
        ViewData["ProjectName"] = project.Name;
        ViewData["IsArchived"] = project.IsArchived;

        return View(new SaveProjectDto
        {
            Key = project.Key,
            Name = project.Name,
            Description = project.Description
        });
    }

    [HttpPost("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SaveProjectDto dto, CancellationToken ct)
    {
        ViewData["ProjectId"] = id;
        ViewData["ProjectName"] = dto.Name;

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _projects.UpdateAsync(id, dto, ct);
            TempData["Success"] = "Proiectul a fost actualizat.";
            return RedirectToAction(nameof(Backlog), new { id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(nameof(dto.Key), ex.Message);
            return View(dto);
        }
    }

    [HttpPost("{id:int}/Archive")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id, bool archived, CancellationToken ct)
    {
        await _projects.SetArchivedAsync(id, archived, ct);

        TempData["Success"] = archived
            ? "Proiectul a fost arhivat."
            : "Proiectul a fost dezarhivat.";

        return RedirectToAction(nameof(Index), new { includeArchived = archived });
    }

    [HttpGet("{id:int}")]
    public IActionResult Details(int id) => RedirectToAction(nameof(Backlog), new { id });

    [HttpGet("{id:int}/Backlog")]
    public async Task<IActionResult> Backlog(
        int id,
        [FromQuery] IssueFilter filter,
        bool showAll = false,
        CancellationToken ct = default)
    {
        var project = await _projects.GetByIdAsync(id, ct);

        filter.ProjectId = id;
        filter.BacklogOnly = !showAll;

        var sprints = await _sprints.ListForProjectAsync(id, ct);

        return View(new BacklogViewModel
        {
            Project = project,
            Permissions = await _projects.GetPermissionsAsync(id, ct),
            Issues = await _issues.ListAsync(filter, ct),
            OpenSprints = sprints.Where(s => s.Status != SprintStatus.Completed).ToList(),
            AssignableUsers = await _projects.ListAssignableUsersAsync(id, ct),
            Labels = await _labels.ListAsync(false, ct),
            Filter = filter,
            ShowBacklogOnly = !showAll
        });
    }

    [HttpGet("{id:int}/Sprints")]
    public async Task<IActionResult> Sprints(int id, CancellationToken ct)
        => View(new SprintsViewModel
        {
            Project = await _projects.GetByIdAsync(id, ct),
            Permissions = await _projects.GetPermissionsAsync(id, ct),
            Sprints = await _sprints.ListForProjectAsync(id, ct),
            NewSprint = new SaveSprintDto()
        });

    [HttpPost("{id:int}/Sprints")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSprint(
        int id,
        [Bind(Prefix = nameof(SprintsViewModel.NewSprint))] SaveSprintDto dto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Sprints), new SprintsViewModel
            {
                Project = await _projects.GetByIdAsync(id, ct),
                Permissions = await _projects.GetPermissionsAsync(id, ct),
                Sprints = await _sprints.ListForProjectAsync(id, ct),
                NewSprint = dto
            });
        }

        await _sprints.CreateAsync(id, dto, ct);
        TempData["Success"] = "Sprintul a fost creat.";

        return RedirectToAction(nameof(Sprints), new { id });
    }

    [HttpPost("{id:int}/Sprints/{sprintId:int}/Start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartSprint(int id, int sprintId, CancellationToken ct)
    {
        try
        {
            await _sprints.StartAsync(sprintId, ct);
            TempData["Success"] = "Sprintul a fost pornit.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Sprints), new { id });
    }

    [HttpPost("{id:int}/Sprints/{sprintId:int}/Complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteSprint(int id, int sprintId, CancellationToken ct)
    {
        try
        {
            await _sprints.CompleteAsync(sprintId, ct);
            TempData["Success"] =
                "Sprintul a fost incheiat. Issue-urile nefinalizate s-au intors in backlog.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Sprints), new { id });
    }

    [HttpPost("{id:int}/Sprints/{sprintId:int}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSprint(int id, int sprintId, CancellationToken ct)
    {
        await _sprints.DeleteAsync(sprintId, ct);
        TempData["Success"] = "Sprintul a fost sters. Issue-urile lui s-au intors in backlog.";

        return RedirectToAction(nameof(Sprints), new { id });
    }

    [HttpGet("{id:int}/Board")]
    public async Task<IActionResult> Board(int id, CancellationToken ct)
        => View(new BoardViewModel
        {
            Project = await _projects.GetByIdAsync(id, ct),
            Permissions = await _projects.GetPermissionsAsync(id, ct),
            ActiveSprint = await _sprints.GetActiveForProjectAsync(id, ct)
        });

    [HttpGet("{id:int}/Members")]
    public async Task<IActionResult> Members(int id, CancellationToken ct)
        => View(new MembersViewModel
        {
            Project = await _projects.GetByIdAsync(id, ct),
            Permissions = await _projects.GetPermissionsAsync(id, ct),
            Members = await _projects.ListMembersAsync(id, ct),
            NewMember = new AddProjectMemberDto()
        });

    [HttpPost("{id:int}/Members")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(
        int id,
        [Bind(Prefix = nameof(MembersViewModel.NewMember))] AddProjectMemberDto dto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Members), new MembersViewModel
            {
                Project = await _projects.GetByIdAsync(id, ct),
                Permissions = await _projects.GetPermissionsAsync(id, ct),
                Members = await _projects.ListMembersAsync(id, ct),
                NewMember = dto
            });
        }

        try
        {
            await _projects.AddMemberAsync(id, dto, ct);
            TempData["Success"] = "Membrul a fost adaugat.";
        }
        catch (Exception ex) when (ex is DomainException or NotFoundException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Members), new { id });
    }

    [HttpPost("{id:int}/Members/{userId}/Role")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateMemberRole(
        int id,
        string userId,
        ProjectRole role,
        CancellationToken ct)
    {
        try
        {
            await _projects.UpdateMemberRoleAsync(
                id, userId, new UpdateProjectMemberRoleDto { Role = role }, ct);

            TempData["Success"] = "Rolul a fost actualizat.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Members), new { id });
    }

    [HttpPost("{id:int}/Members/{userId}/Remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int id, string userId, CancellationToken ct)
    {
        try
        {
            await _projects.RemoveMemberAsync(id, userId, ct);
            TempData["Success"] =
                "Membrul a fost scos, iar issue-urile lui din acest proiect au fost dezasignate.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Members), new { id });
    }
}

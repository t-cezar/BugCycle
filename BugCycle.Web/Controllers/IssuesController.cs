using BugCycle.Application.DTOs.Comments;
using BugCycle.Application.DTOs.Issues;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Domain.Enums;
using BugCycle.Domain.Exceptions;
using BugCycle.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers;

[Route("Issues")]
public class IssuesController : Controller
{
    private readonly IIssueService _issues;
    private readonly IProjectService _projects;
    private readonly ISprintService _sprints;
    private readonly ILabelService _labels;
    private readonly ICommentService _comments;

    public IssuesController(
        IIssueService issues,
        IProjectService projects,
        ISprintService sprints,
        ILabelService labels,
        ICommentService comments)
    {
        _issues = issues;
        _projects = projects;
        _sprints = sprints;
        _labels = labels;
        _comments = comments;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
        => View(new IssueDetailViewModel
        {
            Issue = await _issues.GetByIdAsync(id, ct),
            NewComment = new SaveCommentDto()
        });

    [HttpGet("Create")]
    public async Task<IActionResult> Create(
        int projectId,
        int? sprintId = null,
        CancellationToken ct = default)
    {
        var model = await BuildCreateModelAsync(projectId, ct);
        model.Input.SprintId = sprintId;

        return View(model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(Prefix = nameof(CreateIssueViewModel.Input))] CreateIssueDto input,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var invalid = await BuildCreateModelAsync(input.ProjectId, ct);
            invalid.Input = input;
            return View(invalid);
        }

        try
        {
            var created = await _issues.CreateAsync(input, ct);
            TempData["Success"] = $"Issue-ul {created.DisplayKey} a fost creat.";

            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            var invalid = await BuildCreateModelAsync(input.ProjectId, ct);
            invalid.Input = input;
            return View(invalid);
        }
    }

    [HttpGet("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var issue = await _issues.GetByIdAsync(id, ct);

        if (!issue.CanEdit)
        {
            throw new ForbiddenException("Nu ai permisiunea sa editezi acest issue.");
        }

        var model = await BuildEditModelAsync(issue.ProjectId, ct);
        model.IssueId = issue.Id;
        model.DisplayKey = issue.DisplayKey;
        model.ProjectName = issue.ProjectName;
        model.Input = new UpdateIssueDto
        {
            Title = issue.Title,
            Description = issue.Description,
            Type = issue.Type,
            Status = issue.Status,
            Priority = issue.Priority,
            AssigneeId = issue.Assignee?.Id,
            SprintId = issue.SprintId,
            StoryPoints = issue.StoryPoints,
            LabelIds = issue.Labels.Select(l => l.Id).ToList()
        };

        return View(model);
    }

    [HttpPost("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        int projectId,
        [Bind(Prefix = nameof(EditIssueViewModel.Input))] UpdateIssueDto input,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(await RebuildEditModelAsync(id, projectId, input, ct));
        }

        try
        {
            var updated = await _issues.UpdateAsync(id, input, ct);
            TempData["Success"] = $"Issue-ul {updated.DisplayKey} a fost actualizat.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await RebuildEditModelAsync(id, projectId, input, ct));
        }
    }

    [HttpPost("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int projectId, CancellationToken ct)
    {
        await _issues.DeleteAsync(id, ct);
        TempData["Success"] = "Issue-ul a fost sters definitiv.";

        return RedirectToAction("Backlog", "Projects", new { id = projectId });
    }

    [HttpPost("{id:int}/Sprint")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetSprint(
        int id,
        int projectId,
        int? sprintId,
        CancellationToken ct)
    {
        try
        {
            await _issues.SetSprintAsync(id, sprintId, ct);
            TempData["Success"] = sprintId is null
                ? "Issue-ul a fost mutat in backlog."
                : "Issue-ul a fost adaugat in sprint.";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Backlog", "Projects", new { id = projectId });
    }

    [HttpPost("{id:int}/Comments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(
        int id,
        [Bind(Prefix = nameof(IssueDetailViewModel.NewComment))] SaveCommentDto dto,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Details), new IssueDetailViewModel
            {
                Issue = await _issues.GetByIdAsync(id, ct),
                NewComment = dto
            });
        }

        await _comments.CreateAsync(id, dto, ct);

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/Comments/{commentId:int}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComment(int id, int commentId, CancellationToken ct)
    {
        await _comments.DeleteAsync(commentId, ct);
        TempData["Success"] = "Comentariul a fost sters.";

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<CreateIssueViewModel> BuildCreateModelAsync(int projectId, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(projectId, ct);
        var sprints = await _sprints.ListForProjectAsync(projectId, ct);

        return new CreateIssueViewModel
        {
            Input = new CreateIssueDto { ProjectId = projectId },
            ProjectName = project.Name,
            ProjectKey = project.Key,
            AssignableUsers = await _projects.ListAssignableUsersAsync(projectId, ct),
            OpenSprints = sprints.Where(s => s.Status != SprintStatus.Completed).ToList(),
            Labels = await _labels.ListAsync(false, ct)
        };
    }

    private async Task<EditIssueViewModel> BuildEditModelAsync(int projectId, CancellationToken ct)
    {
        var sprints = await _sprints.ListForProjectAsync(projectId, ct);

        return new EditIssueViewModel
        {
            ProjectId = projectId,
            AssignableUsers = await _projects.ListAssignableUsersAsync(projectId, ct),
            OpenSprints = sprints.Where(s => s.Status != SprintStatus.Completed).ToList(),
            Labels = await _labels.ListAsync(false, ct)
        };
    }

    private async Task<EditIssueViewModel> RebuildEditModelAsync(
        int id,
        int projectId,
        UpdateIssueDto input,
        CancellationToken ct)
    {
        var model = await BuildEditModelAsync(projectId, ct);
        var issue = await _issues.GetByIdAsync(id, ct);

        model.IssueId = id;
        model.DisplayKey = issue.DisplayKey;
        model.ProjectName = issue.ProjectName;
        model.Input = input;

        return model;
    }
}

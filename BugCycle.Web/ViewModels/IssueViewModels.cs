using BugCycle.Application.DTOs.Comments;
using BugCycle.Application.DTOs.Issues;
using BugCycle.Application.DTOs.Labels;
using BugCycle.Application.DTOs.Sprints;
using BugCycle.Application.DTOs.Users;

namespace BugCycle.Web.ViewModels;

public class CreateIssueViewModel
{
    public CreateIssueDto Input { get; set; } = new();

    public string ProjectName { get; set; } = string.Empty;
    public string ProjectKey { get; set; } = string.Empty;

    public List<UserSummaryDto> AssignableUsers { get; set; } = new();
    public List<SprintDto> OpenSprints { get; set; } = new();
    public List<LabelDto> Labels { get; set; } = new();
}

public class EditIssueViewModel
{
    public int IssueId { get; set; }
    public string DisplayKey { get; set; } = string.Empty;

    public UpdateIssueDto Input { get; set; } = new();

    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;

    public List<UserSummaryDto> AssignableUsers { get; set; } = new();
    public List<SprintDto> OpenSprints { get; set; } = new();
    public List<LabelDto> Labels { get; set; } = new();
}

public class IssueDetailViewModel
{
    public IssueDetailDto Issue { get; set; } = new();
    public SaveCommentDto NewComment { get; set; } = new();
}

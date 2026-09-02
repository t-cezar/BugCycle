using BugCycle.Application.DTOs.Projects;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Enums;

namespace BugCycle.Application.Mapping;

public static class ProjectMappings
{
    public static ProjectDto ToDto(
        this Project project,
        ProjectRole? myRole = null,
        int issueCount = 0) => new()
    {
        Id = project.Id,
        Key = project.Key,
        Name = project.Name,
        Description = project.Description,
        IsArchived = project.IsArchived,
        CreatedAt = project.CreatedAt,
        CreatedByName = project.CreatedBy?.FullName ?? string.Empty,
        MemberCount = project.Members?.Count ?? 0,
        IssueCount = issueCount,
        MyRole = myRole
    };

    public static ProjectDetailDto ToDetailDto(
        this Project project,
        ProjectRole? myRole = null,
        int issueCount = 0) => new()
    {
        Id = project.Id,
        Key = project.Key,
        Name = project.Name,
        Description = project.Description,
        IsArchived = project.IsArchived,
        CreatedAt = project.CreatedAt,
        CreatedByName = project.CreatedBy?.FullName ?? string.Empty,
        MemberCount = project.Members?.Count ?? 0,
        IssueCount = issueCount,
        MyRole = myRole,
        Members = project.Members?
            .OrderBy(m => m.Role)
            .ThenBy(m => m.User?.FullName)
            .Select(m => m.ToDto())
            .ToList() ?? new List<ProjectMemberDto>()
    };

    public static ProjectMemberDto ToDto(this ProjectMember member) => new()
    {
        User = member.User?.ToSummaryDto() ?? new(),
        Role = member.Role,
        JoinedAt = member.JoinedAt
    };

    public static List<ProjectMemberDto> ToDtos(this IEnumerable<ProjectMember> members)
        => members.Select(m => m.ToDto()).ToList();
}

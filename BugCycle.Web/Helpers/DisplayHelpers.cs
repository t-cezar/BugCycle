using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BugCycle.Domain.Enums;

namespace BugCycle.Web.Helpers;

// Traduceri de la valorile din domeniu la clase CSS si etichete afisabile.
// Le pun aici, si nu in view-uri, ca aceeasi culoare de status sa apara identic pe dashboard,
// in backlog si pe board.

public static class DisplayHelpers
{
    public static string StatusClass(IssueStatus status) => status switch
    {
        IssueStatus.ToDo => "status-todo",
        IssueStatus.InProgress => "status-progress",
        IssueStatus.InReview => "status-review",
        IssueStatus.Done => "status-done",
        _ => "status-todo"
    };

    public static string PriorityClass(IssuePriority priority) => priority switch
    {
        IssuePriority.Low => "priority-low",
        IssuePriority.Medium => "priority-medium",
        IssuePriority.High => "priority-high",
        IssuePriority.Critical => "priority-critical",
        _ => "priority-medium"
    };

    public static string TypeClass(IssueType type) => type switch
    {
        IssueType.Bug => "type-bug",
        IssueType.Task => "type-task",
        IssueType.Story => "type-story",
        _ => "type-task"
    };

    public static string TypeIcon(IssueType type) => type switch
    {
        IssueType.Bug => "B",
        IssueType.Task => "T",
        IssueType.Story => "S",
        _ => "?"
    };

    public static string SprintStatusClass(SprintStatus status) => status switch
    {
        SprintStatus.Planned => "sprint-planned",
        SprintStatus.Active => "sprint-active",
        SprintStatus.Completed => "sprint-completed",
        _ => "sprint-planned"
    };

    public static string ProjectRoleClass(ProjectRole role) => role switch
    {
        ProjectRole.Owner => "role-owner",
        ProjectRole.Member => "role-member",
        ProjectRole.Viewer => "role-viewer",
        _ => "role-viewer"
    };

    // Citeste atributul Display de pe valoarea enum, ca "InProgress" sa apara
    // ca "In Progress" fara sa se repete tabele de traducere in fiecare view.
    public static string Label<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();

        var display = typeof(TEnum)
            .GetField(name!, BindingFlags.Public | BindingFlags.Static)
            ?.GetCustomAttribute<DisplayAttribute>();

        return display?.Name ?? name!;
    }

    public static string RelativeTime(DateTime utc)
    {
        var delta = DateTime.UtcNow - utc;

        return delta switch
        {
            { TotalMinutes: < 1 } => "acum",
            { TotalMinutes: < 60 } => $"acum {(int)delta.TotalMinutes} min",
            { TotalHours: < 24 } => $"acum {(int)delta.TotalHours} h",
            { TotalDays: < 30 } => $"acum {(int)delta.TotalDays} zile",
            _ => utc.ToLocalTime().ToString("dd MMM yyyy")
        };
    }
}

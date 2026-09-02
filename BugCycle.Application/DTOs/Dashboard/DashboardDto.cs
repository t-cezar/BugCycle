using BugCycle.Application.DTOs.Issues;
using BugCycle.Application.DTOs.Projects;
using BugCycle.Domain.Enums;

namespace BugCycle.Application.DTOs.Dashboard;

public class DashboardDto
{
    //Issue-urile asignate utilizatorului curent, nefinalizate
    public List<IssueDto> MyIssues { get; set; } = new();

    // Contoare pe cele patru statusuri, calculate din interogare
    public Dictionary<IssueStatus, int> CountsByStatus { get; set; } = new();

    public List<ProjectDto> MyProjects { get; set; } = new();

    public int TotalAssigned => CountsByStatus.Values.Sum();

    public int OpenAssigned => CountsByStatus
        .Where(kv => kv.Key != IssueStatus.Done)
        .Sum(kv => kv.Value);
}

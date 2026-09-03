using BugCycle.Application.DTOs.Issues;
using BugCycle.Application.DTOs.Sprints;

namespace BugCycle.Application.Interfaces.Services;

public interface ISprintService
{
    Task<List<SprintDto>> ListForProjectAsync(int projectId, CancellationToken ct = default);

    Task<SprintDto> GetByIdAsync(int id, CancellationToken ct = default);

    Task<SprintDto?> GetActiveForProjectAsync(int projectId, CancellationToken ct = default);

    Task<SprintDto> CreateAsync(int projectId, SaveSprintDto dto, CancellationToken ct = default);

    Task<SprintDto> UpdateAsync(int id, SaveSprintDto dto, CancellationToken ct = default);

    Task<SprintDto> StartAsync(int id, CancellationToken ct = default);

    Task<SprintDto> CompleteAsync(int id, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<List<IssueDto>> ListBoardIssuesAsync(int sprintId, CancellationToken ct = default);
}

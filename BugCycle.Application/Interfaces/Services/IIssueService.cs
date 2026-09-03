using BugCycle.Application.DTOs.Issues;

namespace BugCycle.Application.Interfaces.Services;

public interface IIssueService
{
    Task<List<IssueDto>> ListAsync(IssueFilter filter, CancellationToken ct = default);

    Task<IssueDetailDto> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IssueDto> CreateAsync(CreateIssueDto dto, CancellationToken ct = default);

    Task<IssueDto> UpdateAsync(int id, UpdateIssueDto dto, CancellationToken ct = default);

    Task<IssueDto> MoveAsync(int id, MoveIssueDto dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<IssueDto> SetSprintAsync(int id, int? sprintId, CancellationToken ct = default);
}

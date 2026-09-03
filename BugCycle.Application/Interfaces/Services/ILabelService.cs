using BugCycle.Application.DTOs.Labels;

namespace BugCycle.Application.Interfaces.Services;

public interface ILabelService
{
    Task<List<LabelDto>> ListAsync(bool includeUsageCounts = false, CancellationToken ct = default);

    Task<LabelDto> GetByIdAsync(int id, CancellationToken ct = default);

    Task<LabelDto> CreateAsync(SaveLabelDto dto, CancellationToken ct = default);

    Task<LabelDto> UpdateAsync(int id, SaveLabelDto dto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}

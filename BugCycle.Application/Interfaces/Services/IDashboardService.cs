using BugCycle.Application.DTOs.Dashboard;

namespace BugCycle.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetForCurrentUserAsync(CancellationToken ct = default);
}

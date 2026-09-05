using BugCycle.Application.DTOs.Issues;
using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Enums;
using BugCycle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BugCycle.Infrastructure.Repositories;

public class IssueRepository : Repository<Issue>, IIssueRepository
{
    public IssueRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Issue?> GetDetailAsync(int id, CancellationToken ct = default)
        => await Context.Issues
            .AsNoTracking()
            .Include(i => i.Project)
            .Include(i => i.Sprint)
            .Include(i => i.Reporter)
            .Include(i => i.Assignee)
            .Include(i => i.IssueLabels)
                .ThenInclude(il => il.Label)
            .Include(i => i.Comments)
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<Issue?> GetWithLabelsAsync(int id, CancellationToken ct = default)
        => await Context.Issues
            .Include(i => i.IssueLabels)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<List<Issue>> ListAsync(IssueFilter filter, CancellationToken ct = default)
    {
        var query = BuildQuery(filter);

        return await query
            .OrderByDescending(i => i.Priority)
            .ThenByDescending(i => i.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Issue>> ListAssignedToUserAsync(string userId, CancellationToken ct = default)
        => await Context.Issues
            .AsNoTracking()
            .Include(i => i.Project)
            .Include(i => i.Sprint)
            .Include(i => i.Assignee)
            .Include(i => i.Reporter)
            .Where(i => i.AssigneeId == userId && i.Status != IssueStatus.Done)
            .OrderByDescending(i => i.Priority)
            .ThenByDescending(i => i.UpdatedAt)
            .ToListAsync(ct);

    public async Task<Dictionary<IssueStatus, int>> CountByStatusForUserAsync(
        string userId,
        CancellationToken ct = default)
        => await Context.Issues
            .AsNoTracking()
            .Where(i => i.AssigneeId == userId)
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, ct);

    public async Task<int> GetNextNumberAsync(int projectId, CancellationToken ct = default)
    {
        var max = await Context.Issues
            .Where(i => i.ProjectId == projectId)
            .Select(i => (int?)i.Number)
            .MaxAsync(ct);

        return (max ?? 0) + 1;
    }

    public async Task<int> GetMaxBoardOrderAsync(
        int? sprintId,
        IssueStatus status,
        CancellationToken ct = default)
    {
        var max = await Context.Issues
            .Where(i => i.SprintId == sprintId && i.Status == status)
            .Select(i => (int?)i.BoardOrder)
            .MaxAsync(ct);

        return max ?? 0;
    }

    public async Task<List<Issue>> ListColumnAsync(
        int? sprintId,
        IssueStatus status,
        CancellationToken ct = default)
        => await Context.Issues
            .Where(i => i.SprintId == sprintId && i.Status == status)
            .OrderBy(i => i.BoardOrder)
            .ThenBy(i => i.Id)
            .ToListAsync(ct);

    public async Task<List<Issue>> ListAssignedInProjectAsync(
        int projectId,
        string userId,
        CancellationToken ct = default)
        => await Context.Issues
            .Where(i => i.ProjectId == projectId && i.AssigneeId == userId)
            .ToListAsync(ct);

    private IQueryable<Issue> BuildQuery(IssueFilter filter)
    {
        var query = Context.Issues
            .AsNoTracking()
            .Include(i => i.Project)
            .Include(i => i.Sprint)
            .Include(i => i.Reporter)
            .Include(i => i.Assignee)
            .Include(i => i.IssueLabels)
                .ThenInclude(il => il.Label)
            .AsQueryable();

        if (filter.ProjectId.HasValue)
        {
            query = query.Where(i => i.ProjectId == filter.ProjectId.Value);
        }

        if (filter.BacklogOnly)
        {
            query = query.Where(i => i.SprintId == null);
        }
        else if (filter.SprintId.HasValue)
        {
            query = query.Where(i => i.SprintId == filter.SprintId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.AssigneeId))
        {
            query = query.Where(i => i.AssigneeId == filter.AssigneeId);
        }

        if (!string.IsNullOrWhiteSpace(filter.ReporterId))
        {
            query = query.Where(i => i.ReporterId == filter.ReporterId);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(i => i.Type == filter.Type.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(i => i.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(i => i.Priority == filter.Priority.Value);
        }

        if (filter.LabelId.HasValue)
        {
            query = query.Where(i => i.IssueLabels.Any(il => il.LabelId == filter.LabelId.Value));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(i => i.Title.Contains(term));
        }

        return query;
    }
}

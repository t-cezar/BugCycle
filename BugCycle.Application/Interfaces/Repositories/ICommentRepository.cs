using BugCycle.Domain.Entities;

namespace BugCycle.Application.Interfaces.Repositories;

public interface ICommentRepository : IRepository<Comment>
{
    Task<List<Comment>> ListForIssueAsync(int issueId, CancellationToken ct = default);

    Task<Comment?> GetWithIssueAsync(int id, CancellationToken ct = default);
}

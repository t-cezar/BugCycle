using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Domain.Entities;
using BugCycle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BugCycle.Infrastructure.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Comment>> ListForIssueAsync(int issueId, CancellationToken ct = default)
        => await Context.Comments
            .AsNoTracking()
            .Include(c => c.Author)
            .Where(c => c.IssueId == issueId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);

    public async Task<Comment?> GetWithIssueAsync(int id, CancellationToken ct = default)
        => await Context.Comments
            .Include(c => c.Issue)
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
}

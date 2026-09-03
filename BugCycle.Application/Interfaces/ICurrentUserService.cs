namespace BugCycle.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }

    string RequireUserId();
}

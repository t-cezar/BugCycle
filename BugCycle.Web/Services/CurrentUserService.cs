using System.Security.Claims;
using BugCycle.Application.Interfaces;
using BugCycle.Domain.Constants;
using BugCycle.Domain.Exceptions;

namespace BugCycle.Web.Services;


// Implementarea singurei punti dintre HTTP si stratul de business; Cand s-ar adauga o
// schema JWT pentru un client mobil, clasa aceasta nu s-ar modifica: ambele
// scheme populeaza acelasi ClaimsPrincipal.
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => Principal?.IsInRole(AppRoles.Admin) ?? false;

    public string RequireUserId()
        => UserId ?? throw new ForbiddenException(
            "Actiunea necesita un utilizator autentificat.");
}

using BugCycle.Application.Authorization;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BugCycle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjectAccessGuard, ProjectAccessGuard>();

        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISprintService, SprintService>();
        services.AddScoped<IIssueService, IssueService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ILabelService, LabelService>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}

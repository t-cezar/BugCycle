using BugCycle.Domain.Constants;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BugCycle.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    private const string DemoPassword = "Parola123!";

    public static async Task SeedAsync(IServiceProvider services, SeedAdminOptions adminOptions)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);

        var admin = await EnsureUserAsync(
            userManager,
            adminOptions.Email,
            adminOptions.FullName,
            adminOptions.Password,
            new[] { AppRoles.Admin, AppRoles.User });

        var ana = await EnsureUserAsync(
            userManager, "ana@bugcycle.local", "Ana Popescu", DemoPassword, new[] { AppRoles.User });

        var bogdan = await EnsureUserAsync(
            userManager, "bogdan@bugcycle.local", "Bogdan Ionescu", DemoPassword, new[] { AppRoles.User });

        var carmen = await EnsureUserAsync(
            userManager, "carmen@bugcycle.local", "Carmen Radu", DemoPassword, new[] { AppRoles.User });

        var labels = await SeedLabelsAsync(context);

        if (await context.Projects.AnyAsync())
        {
            logger.LogInformation("Datele de demo exista deja; seed-ul se opreste aici.");
            return;
        }

        await SeedDemoProjectsAsync(context, admin, ana, bogdan, carmen, labels);

        logger.LogInformation("Seed complet: roluri, utilizatori, etichete si doua proiecte de demo.");
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        IEnumerable<string> roles)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(
                    $"Nu s-a putut crea utilizatorul de seed '{email}': {errors}");
            }
        }

        foreach (var role in roles)
        {
            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        return user;
    }

    private static async Task<Dictionary<string, Label>> SeedLabelsAsync(AppDbContext context)
    {
        var desired = new (string Name, string Color)[]
        {
            ("frontend", "#2563eb"),
            ("backend", "#7c3aed"),
            ("database", "#0891b2"),
            ("regression", "#dc2626"),
            ("ux", "#db2777"),
            ("tech-debt", "#a16207")
        };

        foreach (var (name, color) in desired)
        {
            if (!await context.Labels.AnyAsync(l => l.Name == name))
            {
                context.Labels.Add(new Label { Name = name, ColorHex = color });
            }
        }

        await context.SaveChangesAsync();

        return await context.Labels.ToDictionaryAsync(l => l.Name, l => l);
    }

    private static async Task SeedDemoProjectsAsync(
        AppDbContext context,
        ApplicationUser admin,
        ApplicationUser ana,
        ApplicationUser bogdan,
        ApplicationUser carmen,
        Dictionary<string, Label> labels)
    {
        var now = DateTime.UtcNow;

        var platform = new Project
        {
            Key = "BUG",
            Name = "BugCycle Platform",
            Description = "Dezvoltarea platformei interne de urmarire a bug-urilor.",
            CreatedById = ana.Id,
            CreatedAt = now.AddDays(-40),
            Members =
            {
                new ProjectMember { UserId = ana.Id, Role = ProjectRole.Owner, JoinedAt = now.AddDays(-40) },
                new ProjectMember { UserId = bogdan.Id, Role = ProjectRole.Member, JoinedAt = now.AddDays(-38) },
                new ProjectMember { UserId = admin.Id, Role = ProjectRole.Member, JoinedAt = now.AddDays(-30) },
                new ProjectMember { UserId = carmen.Id, Role = ProjectRole.Viewer, JoinedAt = now.AddDays(-20) }
            }
        };

        var mobile = new Project
        {
            Key = "MOB",
            Name = "Mobile Companion App",
            Description = "Aplicatia mobila care consuma acelasi API.",
            CreatedById = bogdan.Id,
            CreatedAt = now.AddDays(-15),
            Members =
            {
                new ProjectMember { UserId = bogdan.Id, Role = ProjectRole.Owner, JoinedAt = now.AddDays(-15) },
                new ProjectMember { UserId = ana.Id, Role = ProjectRole.Member, JoinedAt = now.AddDays(-14) }
            }
        };

        context.Projects.AddRange(platform, mobile);
        await context.SaveChangesAsync();

        var activeSprint = new Sprint
        {
            ProjectId = platform.Id,
            Name = "Sprint 4 - Board si permisiuni",
            Goal = "Board-ul kanban functional si autorizarea per proiect.",
            StartDate = now.Date.AddDays(-5),
            EndDate = now.Date.AddDays(9),
            Status = SprintStatus.Active
        };

        var plannedSprint = new Sprint
        {
            ProjectId = platform.Id,
            Name = "Sprint 5 - Rapoarte",
            Goal = "Primele rapoarte de progres si burndown.",
            StartDate = now.Date.AddDays(10),
            EndDate = now.Date.AddDays(24),
            Status = SprintStatus.Planned
        };

        var mobileSprint = new Sprint
        {
            ProjectId = mobile.Id,
            Name = "Sprint 1 - Autentificare",
            Goal = "Login pe mobil peste acelasi API.",
            StartDate = now.Date.AddDays(-2),
            EndDate = now.Date.AddDays(12),
            Status = SprintStatus.Active
        };

        context.Sprints.AddRange(activeSprint, plannedSprint, mobileSprint);
        await context.SaveChangesAsync();

        var seeds = new List<IssueSeed>
        {
            Make(platform, 1, activeSprint.Id, "Cardurile nu isi pastreaza ordinea dupa refresh",
                IssueType.Bug, IssueStatus.InProgress, IssuePriority.High, ana, bogdan, 3, 0,
                "Dupa drag and drop, ordinea se pierde la reincarcarea paginii.",
                new[] { "frontend", "regression" }),

            Make(platform, 2, activeSprint.Id, "Un Viewer poate muta carduri pe board",
                IssueType.Bug, IssueStatus.Done, IssuePriority.Critical, bogdan, ana, 5, 0,
                "Verificarea de permisiuni lipsea din service layer.",
                new[] { "backend" }),

            Make(platform, 3, activeSprint.Id, "Etichetele nu se pot modifica la editarea unui issue",
                IssueType.Bug, IssueStatus.InReview, IssuePriority.High, ana, admin, 3, 0,
                "Colectia many-to-many nu era reconciliata la update.",
                new[] { "backend", "database" }),

            Make(platform, 4, activeSprint.Id, "Adauga contor de story points in header-ul sprintului",
                IssueType.Task, IssueStatus.ToDo, IssuePriority.Medium, ana, bogdan, 2, 0,
                null, new[] { "frontend" }),

            Make(platform, 5, activeSprint.Id, "Scoaterea unui membru lasa issue-uri asignate lui",
                IssueType.Bug, IssueStatus.Done, IssuePriority.Critical, admin, ana, 5, 1,
                "Dezasignarea trebuie facuta in aceeasi tranzactie cu scoaterea membrului.",
                new[] { "backend", "database" }),

            Make(platform, 6, activeSprint.Id, "Mesaj de eroare vizibil cand drag-ul este respins",
                IssueType.Story, IssueStatus.ToDo, IssuePriority.Medium, bogdan, admin, 3, 1,
                null, new[] { "frontend", "ux" }),

            Make(platform, 7, activeSprint.Id, "Refactor: scoate logica de business din controllere",
                IssueType.Task, IssueStatus.InProgress, IssuePriority.Low, ana, ana, 8, 1,
                null, new[] { "tech-debt", "backend" }),

            Make(platform, 8, null, "Export backlog in CSV",
                IssueType.Story, IssueStatus.ToDo, IssuePriority.Low, ana, null, 5, 0,
                null, new[] { "backend" }),

            Make(platform, 9, null, "Burndown chart pentru sprintul activ",
                IssueType.Story, IssueStatus.ToDo, IssuePriority.Medium, carmen, null, 8, 0,
                null, new[] { "frontend" }),

            Make(platform, 10, null, "Notificare pe email la asignare",
                IssueType.Story, IssueStatus.ToDo, IssuePriority.Low, ana, null, null, 0,
                null, Array.Empty<string>()),

            Make(platform, 11, plannedSprint.Id, "Raport de timp petrecut pe status",
                IssueType.Story, IssueStatus.ToDo, IssuePriority.Medium, ana, bogdan, 5, 0,
                null, new[] { "backend" }),

            Make(mobile, 1, mobileSprint.Id, "Ecran de login care consuma /api/auth",
                IssueType.Story, IssueStatus.InProgress, IssuePriority.High, bogdan, ana, 5, 0,
                null, new[] { "frontend" }),

            Make(mobile, 2, mobileSprint.Id, "Lista de issue-uri asignate mie",
                IssueType.Story, IssueStatus.ToDo, IssuePriority.Medium, bogdan, bogdan, 3, 0,
                null, new[] { "frontend" }),

            Make(mobile, 3, null, "Cache offline pentru backlog",
                IssueType.Task, IssueStatus.ToDo, IssuePriority.Low, ana, null, 8, 0,
                null, new[] { "tech-debt" })
        };

        context.Issues.AddRange(seeds.Select(s => s.Issue));
        await context.SaveChangesAsync();

        foreach (var seed in seeds)
        {
            foreach (var name in seed.LabelNames)
            {
                if (labels.TryGetValue(name, out var label))
                {
                    context.IssueLabels.Add(new IssueLabel
                    {
                        IssueId = seed.Issue.Id,
                        LabelId = label.Id
                    });
                }
            }
        }

        var firstBug = seeds[0].Issue;
        var permissionBug = seeds[1].Issue;

        context.Comments.AddRange(
            new Comment
            {
                IssueId = firstBug.Id,
                AuthorId = bogdan.Id,
                Body = "Am reprodus problema: BoardOrder nu se salva la drop.",
                CreatedAt = now.AddDays(-3)
            },
            new Comment
            {
                IssueId = firstBug.Id,
                AuthorId = ana.Id,
                Body = "Renumerotam toata coloana la fiecare mutare, e cel mai simplu si sigur.",
                CreatedAt = now.AddDays(-2)
            },
            new Comment
            {
                IssueId = permissionBug.Id,
                AuthorId = ana.Id,
                Body = "Verificarea era doar pe controller. Am mutat-o in ProjectAccessGuard.",
                CreatedAt = now.AddDays(-1)
            });

        await context.SaveChangesAsync();
    }

    // Issue-ul impreuna cu numele etichetelor lui.
    // Legaturile IssueLabels se pot insera abia dupa primul
    //SaveChanges, cand issue-urile au Id.
    private sealed record IssueSeed(Issue Issue, string[] LabelNames);

    private static IssueSeed Make(
        Project project,
        int number,
        int? sprintId,
        string title,
        IssueType type,
        IssueStatus status,
        IssuePriority priority,
        ApplicationUser reporter,
        ApplicationUser? assignee,
        int? storyPoints,
        int boardOrder,
        string? description,
        string[] labelNames)
    {
        var createdAt = DateTime.UtcNow.AddDays(-number);

        var issue = new Issue
        {
            ProjectId = project.Id,
            Number = number,
            SprintId = sprintId,
            Title = title,
            Description = description,
            Type = type,
            Status = status,
            Priority = priority,
            ReporterId = reporter.Id,
            AssigneeId = assignee?.Id,
            StoryPoints = storyPoints,
            BoardOrder = boardOrder,
            CreatedAt = createdAt,
            UpdatedAt = createdAt.AddHours(6),
            ClosedAt = status == IssueStatus.Done ? createdAt.AddDays(1) : null
        };

        return new IssueSeed(issue, labelNames);
    }
}

public class SeedAdminOptions
{
    public string Email { get; set; } = "admin@bugcycle.local";
    public string Password { get; set; } = "Admin123!";
    public string FullName { get; set; } = "Site Administrator";
}

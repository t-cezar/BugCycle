# BugCycle

Issue tracker for small teams. Work is grouped into projects, sprints, and a kanban board.

Built with ASP.NET Core 8. Cookie authentication. REST API plus Razor pages. The board is a Vue 3 widget; the rest of the UI is server-rendered.

## Run

Needs .NET 8 SDK and SQL Server LocalDB.

1. Open `BugCycle.sln` in Visual Studio.
2. Set **BugCycle.Web** as the startup project.
3. Run the **https** profile (F5).

Or from a terminal:

```bash
dotnet run --project BugCycle.Web
```

The app listens on `https://localhost:7215`.

Connection string is in `BugCycle.Web/appsettings.json` (`DefaultConnection`). Database `BugCycleDb` is created and migrated on startup. Demo data is inserted only if the database has no projects yet.

Swagger is available in Development at `/swagger`.

Seeded accounts:

| Email | Password | Notes |
|---|---|---|
| `ana@bugcycle.local` | `Parola123!` | Owner on the demo platform project |
| `bogdan@bugcycle.local` | `Parola123!` | Member / owner on the mobile project |
| `carmen@bugcycle.local` | `Parola123!` | Viewer |
| `admin@bugcycle.local` | `Admin123!` | System Admin |

Admin email and password can be changed in `appsettings.json` under `SeedAdmin`.


## Architecture

Four projects. Dependencies go inward. Domain has no reference to ASP.NET or EF.

```
BugCycle.Web            HTTP, Razor, API, Identity cookie setup
        |
        +-- BugCycle.Application    use cases, DTOs, permission checks
                |
                +-- BugCycle.Domain    entities, enums, exceptions
        |
        +-- BugCycle.Infrastructure    EF Core, SQL Server, Identity stores
                |
                +-- BugCycle.Application
                +-- BugCycle.Domain
```

- **Domain** - `Project`, `Issue`, `Sprint`, `Comment`, `Label`, `ProjectMember`, `ApplicationUser`. Enums for status, type, priority, project role, sprint state. `DomainException`, `NotFoundException`, `ForbiddenException`.
- **Application** - services (`ProjectService`, `IssueService`, `SprintService`, ...), repository interfaces, DTOs, manual mapping (no AutoMapper). `ProjectAccessGuard` resolves what the current user may do on a project.
- **Infrastructure** - `AppDbContext`, Fluent API configs (explicit `DeleteBehavior`), repositories, EF migrations, `DbSeeder`, `IdentityService`.
- **Web** - MVC controllers and views. REST under `/api`. `GlobalExceptionHandler` maps domain exceptions to HTTP status codes. API calls get ProblemDetails. Razor requests are redirected to an error page. Unauthenticated `/api` calls return 401 instead of a login redirect.

```
BugCycle.sln
BugCycle.Domain/
  Entities/  Enums/  Exceptions/  Constants/
BugCycle.Application/
  Services/  DTOs/  Mapping/  Authorization/  Interfaces/
BugCycle.Infrastructure/
  Persistence/          AppDbContext, Configurations, Seed
  Repositories/
  Identity/
  Migrations/
BugCycle.Web/
  Controllers/          MVC
  Controllers/Api/      REST
  Views/
  Middleware/
  wwwroot/js/board/     Vue kanban (api.js, components.js, app.js)
  Program.cs
```

## What it does

**Accounts.** Register, login, logout, profile, change password. New accounts get the `User` role. `Admin` is granted from the admin panel. The `User` role cannot be removed. Accounts are not deleted, so reporters and comment authors stay in history.

**Projects.** Create a project with a unique key (`BUG`, `MOB`, ...). Issues are numbered per project and shown as `BUG-14`. Archive / unarchive. Only a system Admin can delete a project (cascade). Creator becomes Owner.

**Roles.** System: `Admin`, `User`. Per project: Owner, Member, Viewer.

- Owner - project settings, members, sprints, issue delete
- Member - create/edit issues and comments; drag on the board
- Viewer - read only; cannot be assigned issues
- System Admin - bypasses project roles; can open any project

A project must keep at least one Owner. Removing a member clears that person's assignee on issues in the project.

**Issues.** Types: Bug, Task, Story. Status: To Do, In Progress, In Review, Done. Priority: Low to Critical. Optional story points, assignee, labels, description. Filters on the backlog (status, type, assignee, label, search). An issue cannot move to Done without an assignee.

**Sprints.** Planned, Active, Completed. One active sprint per project. Completing a sprint sends unfinished issues back to the backlog (status To Do). Deleting a sprint does the same. The board shows the active sprint only.

**Board.** Vue 3 (CDN). Drag and drop between columns. `PATCH /api/issues/{id}/move`. The UI updates immediately, then confirms with the server. If the server rejects the move (Viewer, Done without assignee, archived project), the card snaps back.


**Comments.** Members and Owners can comment. Authors can edit their own comments. Authors or project Owners can delete.

**Labels.** Global. Admins create/edit/delete them. Deleting a label removes it from all issues.

**Dashboard.** Counts and open issues assigned to the current user, across all of their projects. Not a copy of the board columns.


**Admin panel.** Users and roles, labels, all projects. An Admin cannot drop their own Admin role. The last Admin in the system cannot be demoted.


**API**

| Prefix | Role |
|---|---|
| `/api/projects` | CRUD, archive, members |
| `/api/issues` | CRUD, move, set sprint |
| `/api/sprints` | CRUD, start, complete, board issues |
| `/api/issues/{id}/comments` | list, create, edit, delete |
| `/api/labels` | list; write is Admin-only |
| `/api/admin/users` | Admin-only |

Enums are serialized as strings (`InProgress`, not `1`).

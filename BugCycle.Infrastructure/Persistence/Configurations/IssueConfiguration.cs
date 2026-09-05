using BugCycle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BugCycle.Infrastructure.Persistence.Configurations;

public class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .HasMaxLength(4000);

        builder.Ignore(i => i.DisplayKey);

        // Identificatorul afisat (BUG-14) trebuie sa fie unic in proiect
        builder.HasIndex(i => new { i.ProjectId, i.Number })
            .IsUnique();

        // Indexul care serveste interogarea board-ului
        builder.HasIndex(i => new { i.SprintId, i.Status, i.BoardOrder });

        builder.HasOne(i => i.Project)
            .WithMany(p => p.Issues)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // NoAction, desi comportamentul dorit este "sprintul dispare, issue-ul
        // cade in backlog". SET NULL aici ar face o a doua cale de cascada de la
        // Projects spre Issues (Projects -> Sprints -> Issues, pe langa
        // Projects -> Issues). SQL Server refuza migratia si da.
        // Golirea lui SprintId se face explicit in SprintService.DeleteAsync
        builder.HasOne(i => i.Sprint)
            .WithMany(s => s.Issues)
            .HasForeignKey(i => i.SprintId)
            .OnDelete(DeleteBehavior.NoAction);

        // Reporter nu se poate pierde (istoricul trebuie sa ramana)
        builder.HasOne(i => i.Reporter)
            .WithMany(u => u.ReportedIssues)
            .HasForeignKey(i => i.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Daca utilizatorul dispare, issue-ul devine neasignat in loc sa
        // ramana cu o referinta invalida
        builder.HasOne(i => i.Assignee)
            .WithMany(u => u.AssignedIssues)
            .HasForeignKey(i => i.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

using BugCycle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BugCycle.Infrastructure.Persistence.Configurations;

public class IssueLabelConfiguration : IEntityTypeConfiguration<IssueLabel>
{
    public void Configure(EntityTypeBuilder<IssueLabel> builder)
    {
        builder.HasKey(il => new { il.IssueId, il.LabelId });

        builder.HasOne(il => il.Issue)
            .WithMany(i => i.IssueLabels)
            .HasForeignKey(il => il.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cand un administrator sterge o eticheta globala, ea dispare automat
        // de pe toate issue-urile pe care era aplicata
        builder.HasOne(il => il.Label)
            .WithMany(l => l.IssueLabels)
            .HasForeignKey(il => il.LabelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

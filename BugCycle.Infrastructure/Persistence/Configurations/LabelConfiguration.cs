using BugCycle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BugCycle.Infrastructure.Persistence.Configurations;

public class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(l => l.ColorHex)
            .IsRequired()
            .HasMaxLength(7);

        builder.HasIndex(l => l.Name)
            .IsUnique();
    }
}

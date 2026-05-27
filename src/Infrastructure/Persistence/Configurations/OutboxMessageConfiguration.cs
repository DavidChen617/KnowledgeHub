using CoreMesh.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.EventType).HasMaxLength(256).IsRequired();
        builder.Property(m => m.Payload).IsRequired();
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.OccurredAtUtc).IsRequired();

        builder.HasIndex(m => new { m.Status, m.NextRetryAtUtc });
    }
}

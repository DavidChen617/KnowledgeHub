using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UserIdentityConfiguration : IEntityTypeConfiguration<UserIdentity>
{
    public void Configure(EntityTypeBuilder<UserIdentity> builder)
    {
        builder.ToTable("user_identities");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => new UserIdentityId(value))
            .HasColumnName("id");

        builder.Property(i => i.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(i => i.Provider)
            .HasMaxLength(32)
            .HasColumnName("provider")
            .IsRequired();

        builder.Property(i => i.ProviderSub)
            .HasMaxLength(256)
            .HasColumnName("provider_sub")
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(i => new { i.Provider, i.ProviderSub })
            .IsUnique()
            .HasDatabaseName("ix_user_identities_provider_sub");
    }
}

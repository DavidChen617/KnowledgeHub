using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("id")
            .HasComment("使用者唯一識別碼");

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .HasColumnName("email")
            .IsRequired()
            .HasComment("使用者電子郵件（唯一）");

        builder.Property(u => u.Username)
            .HasMaxLength(64)
            .HasColumnName("username")
            .IsRequired()
            .HasComment("使用者顯示名稱");

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(1024)
            .HasColumnName("avatar_url")
            .HasComment("使用者大頭貼 URL");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("建立時間（UTC）");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");
    }
}

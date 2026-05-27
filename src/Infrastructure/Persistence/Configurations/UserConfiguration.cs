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
            .HasComment("使用者電子郵件，全域唯一");

        builder.Property(u => u.Name)
            .HasMaxLength(128)
            .HasColumnName("name")
            .IsRequired()
            .HasComment("顯示名稱");

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(512)
            .HasColumnName("avatar_url")
            .HasComment("大頭貼 URL；NULL 表示使用預設頭像");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("帳號建立時間（UTC）");

        builder.HasMany(u => u.Identities)
            .WithOne()
            .HasForeignKey("user_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(u => u.Identities).HasField("_identities");

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey("user_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(u => u.RefreshTokens).HasField("_refreshTokens");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");
    }
}

internal sealed class UserIdentityConfiguration : IEntityTypeConfiguration<UserIdentity>
{
    public void Configure(EntityTypeBuilder<UserIdentity> builder)
    {
        builder.ToTable("user_identities", t => t.HasComment("OAuth / SSO 第三方身份提供者記錄"));

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .HasComment("Identity 唯一識別碼");

        builder.Property<Guid>("user_id")
            .HasColumnName("user_id")
            .IsRequired()
            .HasComment("所屬使用者 ID");

        builder.Property(i => i.Provider)
            .HasMaxLength(64)
            .HasColumnName("provider")
            .IsRequired()
            .HasComment("身份提供者名稱（如 google、github）");

        builder.Property(i => i.ProviderId)
            .HasMaxLength(256)
            .HasColumnName("provider_id")
            .IsRequired()
            .HasComment("提供者側的使用者唯一 ID");

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("綁定時間（UTC）");

        builder.HasIndex(i => new { i.Provider, i.ProviderId })
            .IsUnique()
            .HasDatabaseName("ix_user_identities_provider");
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", t => t.HasComment("JWT refresh token，用於無感換發 access token"));

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasComment("Token 記錄唯一識別碼");

        builder.Property<Guid>("user_id")
            .HasColumnName("user_id")
            .IsRequired()
            .HasComment("所屬使用者 ID");

        builder.Property(t => t.Token)
            .HasMaxLength(512)
            .HasColumnName("token")
            .IsRequired()
            .HasComment("Refresh token 字串，全域唯一");

        builder.Property(t => t.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired()
            .HasComment("Token 到期時間（UTC），預設 7 天");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("發行時間（UTC）");

        builder.HasIndex(t => t.Token)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token");
    }
}

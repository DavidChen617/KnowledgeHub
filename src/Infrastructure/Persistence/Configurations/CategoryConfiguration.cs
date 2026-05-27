using Domain.Categories;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", t => t.HasComment("使用者自訂分類，每位使用者的分類名稱不可重複"));

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new CategoryId(value))
            .HasColumnName("id")
            .HasComment("分類唯一識別碼");

        builder.Property(c => c.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("user_id")
            .IsRequired()
            .HasComment("所屬使用者 ID");

        builder.Property(c => c.Name)
            .HasMaxLength(64)
            .HasColumnName("name")
            .IsRequired()
            .HasComment("分類名稱");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("建立時間（UTC）");

        builder.HasIndex(c => new { c.UserId, c.Name })
            .IsUnique()
            .HasDatabaseName("ix_categories_user_id_name");

        builder.HasIndex(c => c.UserId)
            .HasDatabaseName("ix_categories_user_id");
    }
}

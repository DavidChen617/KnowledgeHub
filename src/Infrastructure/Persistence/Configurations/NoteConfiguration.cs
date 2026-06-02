using Domain.Categories;
using Domain.Notes;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("notes");

        builder.Ignore(n => n.LinkedNoteIds);

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id)
            .HasConversion(id => id.Value, value => new NoteId(value))
            .HasColumnName("id")
            .HasComment("筆記唯一識別碼");

        builder.Property(n => n.UserId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("user_id")
            .IsRequired()
            .HasComment("所屬使用者 ID");

        builder.Property(n => n.Title)
            .HasMaxLength(256)
            .HasColumnName("title")
            .IsRequired()
            .HasComment("筆記標題");

        builder.Property(n => n.Content)
            .HasConversion(c => c.Value, v => new NoteContent(v))
            .HasColumnName("content")
            .IsRequired()
            .HasComment("筆記原始內容（Markdown）");

        builder.Property(n => n.CategoryId)
            .HasConversion(id => id!.Value, value => new CategoryId(value))
            .HasColumnName("category_id")
            .HasComment("所屬分類 ID；NULL 表示未分類");

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(n => n.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => n.CategoryId).HasDatabaseName("ix_notes_category_id");

        builder.Property(n => n.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasComment("最後更新時間（UTC）");

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("建立時間（UTC）");

        builder.Property(n => n.SharedLinkToken)
            .HasMaxLength(64)
            .HasColumnName("shared_link_token")
            .HasComment("分享連結 token");

        builder.HasMany(n => n.Structures)
            .WithOne()
            .HasForeignKey(s => s.NoteId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(n => n.Structures).HasField("_structures");

        builder.HasMany(n => n.Images)
            .WithOne()
            .HasForeignKey(i => i.NoteId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(n => n.Images).HasField("_images");

        builder.HasIndex(n => n.UserId).HasDatabaseName("ix_notes_user_id");
    }
}

using Domain.Notes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class NoteImageConfiguration : IEntityTypeConfiguration<NoteImage>
{
    public void Configure(EntityTypeBuilder<NoteImage> builder)
    {
        builder.ToTable("note_images", t => t.HasComment("筆記內嵌圖片追蹤表，用於協調 Cloudinary 資源清理"));

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .ValueGeneratedNever()
            .HasColumnName("id")
            .HasComment("圖片記錄唯一識別碼");

        builder.Property(i => i.NoteId)
            .HasConversion(id => id.Value, value => new NoteId(value))
            .HasColumnName("note_id")
            .IsRequired()
            .HasComment("所屬筆記 ID");

        builder.Property(i => i.PublicUrl)
            .HasMaxLength(1024)
            .HasColumnName("public_url")
            .IsRequired()
            .HasComment("Cloudinary 圖片公開 URL");

        builder.Property(i => i.Enable)
            .HasColumnName("enable")
            .HasDefaultValue(true)
            .IsRequired()
            .HasComment("false 表示圖片已從筆記內容移除，等待 Outbox Handler 清理 Cloudinary 資源");

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("建立時間（UTC）");

        builder.HasIndex(i => i.NoteId).HasDatabaseName("ix_note_images_note_id");
    }
}

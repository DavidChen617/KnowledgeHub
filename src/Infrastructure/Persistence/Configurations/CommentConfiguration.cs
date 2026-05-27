using Domain.Comments;
using Domain.Notes;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(id => id.Value, value => new CommentId(value))
            .HasColumnName("id")
            .HasComment("留言唯一識別碼");

        builder.Property(c => c.NoteId)
            .HasConversion(id => id.Value, value => new NoteId(value))
            .HasColumnName("note_id")
            .IsRequired()
            .HasComment("所屬筆記 ID");

        builder.Property(c => c.AuthorId)
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasColumnName("author_id")
            .IsRequired()
            .HasComment("留言作者的使用者 ID");

        builder.Property(c => c.Content)
            .HasColumnName("content")
            .IsRequired()
            .HasComment("留言內容");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired()
            .HasComment("最後更新時間（UTC）");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("建立時間（UTC）");

        builder.HasIndex(c => c.NoteId).HasDatabaseName("ix_comments_note_id");
        builder.HasIndex(c => c.AuthorId).HasDatabaseName("ix_comments_author_id");
    }
}

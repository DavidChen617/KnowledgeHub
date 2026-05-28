using Domain.Comments;
using Domain.Notes;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new CommentId(value));

        builder.Property(c => c.NoteId)
            .HasColumnName("note_id")
            .HasConversion(id => id.Value, value => new NoteId(value))
            .HasComment("所屬筆記");

        builder.Property(c => c.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasComment("留言者");

        builder.Property(c => c.ParentCommentId)
            .HasColumnName("parent_comment_id")
            .HasConversion(id => id == null ? (Guid?)null : id.Value, value => value == null ? null : new CommentId(value.Value))
            .HasComment("父留言；NULL 表示頂層留言");

        builder.Property(c => c.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .HasComment("留言內容");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasComment("建立時間（UTC）");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasComment("最後更新時間（UTC）");

        builder.HasOne<Comment>()
            .WithMany()
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

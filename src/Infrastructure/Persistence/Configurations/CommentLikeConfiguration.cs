using Domain.Comments;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CommentLikeConfiguration : IEntityTypeConfiguration<CommentLike>
{
    public void Configure(EntityTypeBuilder<CommentLike> builder)
    {
        builder.ToTable("comment_likes");

        builder.HasKey(cl => new { cl.CommentId, cl.UserId });

        builder.Property(cl => cl.CommentId)
            .HasColumnName("comment_id")
            .HasConversion(id => id.Value, value => new CommentId(value))
            .HasComment("留言 ID");

        builder.Property(cl => cl.UserId)
            .HasColumnName("user_id")
            .HasConversion(id => id.Value, value => new UserId(value))
            .HasComment("按讚者");

        builder.Property(cl => cl.CreatedAt)
            .HasColumnName("created_at")
            .HasComment("按讚時間（UTC）");

        builder.HasOne<Comment>()
            .WithMany()
            .HasForeignKey(cl => cl.CommentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

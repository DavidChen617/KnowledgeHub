using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DN = Domain.Notes;

namespace Infrastructure.Persistence.Configurations;

internal sealed class NoteStructureConfiguration : IEntityTypeConfiguration<DN.NoteStructure>
{
    public void Configure(EntityTypeBuilder<DN.NoteStructure> builder)
    {
        builder.ToTable("note_structures", t => t.HasComment("AI 結構化後的筆記版本，一篇筆記可對應多個結構化結果"));

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .ValueGeneratedNever()
            .HasColumnName("id")
            .HasComment("結構化記錄唯一識別碼");

        builder.Property(s => s.NoteId)
            .HasConversion(id => id.Value, value => new DN.NoteId(value))
            .HasColumnName("note_id")
            .IsRequired()
            .HasComment("所屬筆記 ID");

        builder.Property(s => s.Prompt)
            .HasColumnName("prompt")
            .IsRequired()
            .HasComment("使用者提供給 AI 的結構化指示");

        builder.Property(s => s.Content)
            .HasColumnName("content")
            .IsRequired()
            .HasComment("AI 輸出的結構化 Markdown 內容，段落以 ### 分隔");

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasComment("AI 生成的一句話摘要");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("結構化建立時間（UTC）");

        builder.HasMany(s => s.Chunks)
            .WithOne()
            .HasForeignKey("note_structure_id")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Chunks).HasField("_chunks");

        builder.HasIndex(s => s.NoteId).HasDatabaseName("ix_note_structures_note_id");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAI = Domain.AI;

namespace Infrastructure.Persistence.Configurations;

internal sealed class NoteStructureChunkConfiguration : IEntityTypeConfiguration<DAI.Chunk<string>>
{
    public void Configure(EntityTypeBuilder<DAI.Chunk<string>> builder)
    {
        builder.ToTable("note_structure_chunks", t => t.HasComment("結構化內容依 ### 標題切割的文字區塊，作為向量搜尋的基本單位"));

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .ValueGeneratedNever()
            .HasColumnName("id")
            .HasComment("Chunk 唯一識別碼");

        builder.Property<Guid>("note_structure_id")
            .HasColumnName("note_structure_id")
            .IsRequired()
            .HasComment("所屬結構化記錄 ID");

        builder.Property(c => c.Index)
            .HasColumnName("index")
            .IsRequired()
            .HasComment("Chunk 在結構化內容中的順序索引（從 0 開始）");

        builder.Property(c => c.Value)
            .HasColumnName("value")
            .IsRequired()
            .HasComment("Chunk 的純文字內容，用於 embedding");

        builder.Property(c => c.Source)
            .HasColumnName("source")
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(DAI.ChunkSource.Structured)
            .HasComment("Chunk 來源：Raw 或 Structured");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("建立時間（UTC）");

        builder.HasOne(c => c.Embedding)
            .WithOne()
            .HasForeignKey<DAI.Embedding>(e => e.ChunkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

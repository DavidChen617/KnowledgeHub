using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pgvector;
using DAI = Domain.NoteStructure;

namespace Infrastructure.Persistence.Configurations;

internal sealed class EmbeddingConfiguration : IEntityTypeConfiguration<DAI.Embedding>
{
    public void Configure(EntityTypeBuilder<DAI.Embedding> builder)
    {
        builder.ToTable("note_structure_chunk_embeddings", t => t.HasComment("Chunk 的向量 embedding，用於語意相似度搜尋"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id")
            .HasComment("Embedding 唯一識別碼");

        builder.Property(e => e.ChunkId)
            .HasColumnName("chunk_id")
            .IsRequired()
            .HasComment("對應的 Chunk ID（1:1）");

        builder.Property(e => e.Vector)
            .HasConversion(
                v => new Vector(v),
                v => v.ToArray(),
                new ValueComparer<float[]>(
                    (a, b) => a != null && b != null && a.SequenceEqual(b),
                    v => v.Aggregate(0, (h, e) => HashCode.Combine(h, e.GetHashCode())),
                    v => v.ToArray()))
            .HasColumnType("vector(1024)")
            .HasColumnName("vector")
            .IsRequired()
            .HasComment("1024 維浮點向量（多 provider 統一輸出維度）");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("建立時間（UTC）");

        builder.HasIndex(e => e.ChunkId)
            .IsUnique()
            .HasDatabaseName("ix_embeddings_chunk_id");
    }
}

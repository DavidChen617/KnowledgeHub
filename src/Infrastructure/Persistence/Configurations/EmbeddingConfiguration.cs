using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAI = Domain.AI;

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
            .HasColumnType("real[]")
            .HasColumnName("vector")
            .IsRequired()
            .HasComment("Cohere embed-v3 輸出的 1536 維浮點向量");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasComment("建立時間（UTC）");

        builder.HasIndex(e => e.ChunkId)
            .IsUnique()
            .HasDatabaseName("ix_embeddings_chunk_id");
    }
}

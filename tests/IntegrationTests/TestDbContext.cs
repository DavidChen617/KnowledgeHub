using Domain.AI;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests;

internal class TestDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Pgvector.Vector is not supported by InMemory provider.
        // Remove the value converter and column type so InMemory stores float[] natively.
        var vectorProp = modelBuilder.Entity<Embedding>()
            .Metadata.FindProperty(nameof(Embedding.Vector))!;
        vectorProp.SetValueConverter((Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter?)null);
        vectorProp.RemoveAnnotation("Relational:ColumnType");
    }

    public static TestDbContext Create() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}

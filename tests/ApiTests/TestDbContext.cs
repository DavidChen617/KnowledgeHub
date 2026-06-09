using Domain.NoteStructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ApiTests;

internal class TestDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var vectorProp = modelBuilder.Entity<Embedding>()
            .Metadata.FindProperty(nameof(Embedding.Vector))!;
        vectorProp.SetValueConverter((Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter?)null);
        vectorProp.RemoveAnnotation("Relational:ColumnType");
    }
}

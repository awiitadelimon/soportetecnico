using Microsoft.EntityFrameworkCore;
using SoporteTecnico.Domain.Entities;

namespace SoporteTecnico.Infrastructure.Data;

public class SoporteDbContext(DbContextOptions<SoporteDbContext> options)
    : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id)
                .HasColumnName("Id");

            entity.Property(c => c.Nombre)
                .HasColumnName("Nombre")
                .HasMaxLength(100)
                .IsRequired();
        });
    }
}

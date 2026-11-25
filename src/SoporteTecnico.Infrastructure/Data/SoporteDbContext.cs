
using Microsoft.EntityFrameworkCore;
using SoporteTecnico.Domain.Entities;
using SoporteTecnico.Domain.Enums;

namespace SoporteTecnico.Infrastructure.Data;

public class SoporteDbContext(DbContextOptions<SoporteDbContext> options)
    : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Tecnico> Tecnicos => Set<Tecnico>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Seguimiento> Seguimientos => Set<Seguimiento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CLIENTES
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes", table =>
            {
                table.HasCheckConstraint(
                    "CK_Clientes_Texto",
                    "LEN(TRIM([Nombre])) > 0 AND LEN(TRIM([Email])) > 0");
            });

            entity.HasKey(c => c.Id).HasName("PK_Clientes");

            entity.Property(c => c.Nombre)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(c => c.Email)
                .HasMaxLength(254)
                .UseCollation("Latin1_General_100_CI_AS_SC")
                .IsRequired();

            entity.HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("UQ_Clientes_Email");
        });

        // TÉCNICOS
        modelBuilder.Entity<Tecnico>(entity =>
        {
            entity.ToTable("Tecnicos", table =>
            {
                table.HasCheckConstraint(
                    "CK_Tecnicos_Texto",
                    "LEN(TRIM([Nombre])) > 0 AND LEN(TRIM([Email])) > 0");
            });

            entity.HasKey(t => t.Id).HasName("PK_Tecnicos");

            entity.Property(t => t.Nombre)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(t => t.Email)
                .HasMaxLength(254)
                .UseCollation("Latin1_General_100_CI_AS_SC")
                .IsRequired();

            entity.Property(t => t.Activo)
                .HasDefaultValue(true);

            entity.HasIndex(t => t.Email)
                .IsUnique()
                .HasDatabaseName("UQ_Tecnicos_Email");
        });

        // CATEGORÍAS
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias", table =>
            {
                table.HasCheckConstraint(
                    "CK_Categorias_Nombre",
                    "LEN(TRIM([Nombre])) > 0");
            });

            entity.HasKey(c => c.Id).HasName("PK_Categorias");

            entity.Property(c => c.Nombre)
                .HasMaxLength(100)
                .UseCollation("Latin1_General_100_CI_AS_SC")
                .IsRequired();

            entity.HasIndex(c => c.Nombre)
                .IsUnique()
                .HasDatabaseName("UQ_Categorias_Nombre");
        });

        // TICKETS
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets", table =>
            {
                table.HasCheckConstraint(
                    "CK_Tickets_Prioridad",
                    "[Prioridad] IN ('Baja', 'Media', 'Alta', 'Critica')");

                table.HasCheckConstraint(
                    "CK_Tickets_Texto",
                    "LEN(TRIM([Asunto])) > 0 AND LEN(TRIM([Descripcion])) > 0");

                table.HasCheckConstraint(
                    "CK_Tickets_EstadoYFechas",
                    """
                    ([Estado] = 'Abierto' AND [TecnicoId] IS NULL
                        AND [ResueltoUtc] IS NULL AND [CerradoUtc] IS NULL)
                    OR
                    ([Estado] = 'Asignado' AND [TecnicoId] IS NOT NULL
                        AND [ResueltoUtc] IS NULL AND [CerradoUtc] IS NULL)
                    OR
                    ([Estado] = 'Resuelto' AND [TecnicoId] IS NOT NULL
                        AND [ResueltoUtc] IS NOT NULL
                        AND [ResueltoUtc] >= [CreadoUtc] AND [CerradoUtc] IS NULL)
                    OR
                    ([Estado] = 'Cerrado' AND [TecnicoId] IS NOT NULL
                        AND [ResueltoUtc] IS NOT NULL
                        AND [ResueltoUtc] >= [CreadoUtc]
                        AND [CerradoUtc] IS NOT NULL
                        AND [CerradoUtc] >= [ResueltoUtc])
                    """);
            });

            entity.HasKey(t => t.Id).HasName("PK_Tickets");

            entity.Property(t => t.Asunto)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(t => t.Descripcion)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(t => t.Prioridad)
                .HasConversion<string>()
                .HasColumnType("varchar(12)")
                .UseCollation("Latin1_General_100_BIN2")
                .IsRequired();

                entity.Property(t => t.Estado)
                .HasConversion<string>()
                .HasColumnType("varchar(12)")
                .UseCollation("Latin1_General_100_BIN2")
                .HasDefaultValueSql("('Abierto')")
                .HasSentinel((EstadoTicket)(-1))
                .IsRequired();

            entity.Property(t => t.CreadoUtc)
                .HasColumnType("datetimeoffset(0)")
                .HasDefaultValueSql(
                    "(TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'))");

            entity.Property(t => t.ResueltoUtc)
                .HasColumnType("datetimeoffset(0)");

            entity.Property(t => t.CerradoUtc)
                .HasColumnType("datetimeoffset(0)");

            entity.HasOne<Cliente>()
                .WithMany()
                .HasForeignKey(t => t.ClienteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Tickets_Clientes");

            entity.HasOne<Categoria>()
                .WithMany()
                .HasForeignKey(t => t.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Tickets_Categorias");

            entity.HasOne<Tecnico>()
                .WithMany()
                .HasForeignKey(t => t.TecnicoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Tickets_Tecnicos");

            entity.HasIndex(t => t.ClienteId)
                .HasDatabaseName("IX_Tickets_ClienteId");

            entity.HasIndex(t => t.CategoriaId)
                .HasDatabaseName("IX_Tickets_CategoriaId");

            entity.HasIndex(t => new { t.TecnicoId, t.Estado })
                .HasDatabaseName("IX_Tickets_TecnicoEstado");
        });

        // SEGUIMIENTOS
        modelBuilder.Entity<Seguimiento>(entity =>
        {
            entity.ToTable("Seguimientos", table =>
            {
                table.HasCheckConstraint(
                    "CK_Seguimientos_TipoTecnico",
                    """
                    ([Tipo] = 'Creado' AND [TecnicoId] IS NULL)
                    OR
                    ([Tipo] IN ('Asignado', 'Reasignado', 'Resuelto', 'Cerrado')
                        AND [TecnicoId] IS NOT NULL)
                    OR
                    ([Tipo] = 'Nota')
                    """);

                table.HasCheckConstraint(
                    "CK_Seguimientos_Texto",
                    "LEN(TRIM([Detalle])) > 0 AND LEN(TRIM([Autor])) > 0");
            });

            entity.HasKey(s => s.Id).HasName("PK_Seguimientos");

            entity.Property(s => s.Tipo)
                .HasConversion<string>()
                .HasColumnType("varchar(12)")
                .UseCollation("Latin1_General_100_BIN2")
                .IsRequired();

            entity.Property(s => s.Detalle)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(s => s.Autor)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(s => s.FechaUtc)
                .HasColumnType("datetimeoffset(0)")
                .HasDefaultValueSql(
                    "(TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00'))");

            entity.HasOne(s => s.Ticket)
                .WithMany(t => t.Seguimientos)
                .HasForeignKey(s => s.TicketId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Seguimientos_Tickets");

            entity.HasOne<Tecnico>()
                .WithMany()
                .HasForeignKey(s => s.TecnicoId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Seguimientos_Tecnicos");

            entity.HasIndex(s => new { s.TicketId, s.FechaUtc, s.Id })
                .HasDatabaseName("IX_Seguimientos_TicketFecha");

            entity.HasIndex(s => s.TecnicoId)
                .HasDatabaseName("IX_Seguimientos_TecnicoId");

            entity.HasIndex(s => new { s.TicketId, s.Tipo })
                .IsUnique()
                .HasFilter("[Tipo] IN ('Creado', 'Resuelto', 'Cerrado')")
                .HasDatabaseName("UX_Seguimientos_EventosUnicos");
        });

        // Permite que EF Core utilice la colección privada del agregado.
        modelBuilder.Entity<Ticket>()
            .Navigation(t => t.Seguimientos)
            .HasField("_seguimientos")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

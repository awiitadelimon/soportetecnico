using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SoporteTecnico.Domain.Entities;
using SoporteTecnico.Domain.Enums;
using SoporteTecnico.Infrastructure.Data;
using SoporteTecnico.Infrastructure.Repositories;

namespace SoporteTecnico.Infrastructure.Tests;

public class SoporteRepositoryIntegrationTests
{
    private static string ObtenerConnectionString()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null &&
               !File.Exists(Path.Combine(dir.FullName, "src", "SoporteTecnico.Api", "appsettings.json")))
        {
            dir = dir.Parent;
        }

        if (dir is not null)
        {
            var jsonPath = Path.Combine(dir.FullName, "src", "SoporteTecnico.Api", "appsettings.json");
            using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
            if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                cs.TryGetProperty("SoporteTecnico", out var conn) &&
                conn.GetString() is { Length: > 0 } value)
            {
                return value;
            }
        }

        return "Server=localhost;Database=INF5130_TF15;Trusted_Connection=True;TrustServerCertificate=True;";
    }

    private static SoporteDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<SoporteDbContext>()
            .UseSqlServer(ObtenerConnectionString())
            .Options;

        return new SoporteDbContext(options);
    }

    [Fact]
    public async Task GuardarYRecuperarTicket_CicloCompleto_PersisteEnSqlServer()
    {
        await using var db = CrearContexto();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var cliente = new Cliente($"Cliente Test {sufijo}", $"cli.{sufijo}@tf15.local");
        var categoria = new Categoria($"Cat Test {sufijo}");
        var tecnico = new Tecnico($"Tec Test {sufijo}", $"tec.{sufijo}@tf15.local");

        db.Clientes.Add(cliente);
        db.Categorias.Add(categoria);
        db.Tecnicos.Add(tecnico);
        await db.SaveChangesAsync();

        var repo = new SoporteRepository(db);
        var baseUtc = DateTimeOffset.UtcNow;

        var ticket = new Ticket(
            cliente.Id,
            categoria.Id,
            "Fallo de red en laboratorio",
            "No hay conectividad hacia el servidor.",
            Prioridad.Alta,
            "mesa@tf15.local",
            baseUtc);

        repo.AgregarTicket(ticket);
        await repo.GuardarCambiosAsync();

        ticket.Asignar(tecnico, "admin@tf15.local", baseUtc.AddSeconds(1));
        ticket.AgregarNota("Cable verificado.", tecnico.Email, baseUtc.AddSeconds(2));
        ticket.Resolver("Se reemplazó el cable UTP.", tecnico.Email, baseUtc.AddSeconds(3));
        ticket.Cerrar("admin@tf15.local", baseUtc.AddSeconds(4));
        await repo.GuardarCambiosAsync();

        db.ChangeTracker.Clear();

        var recuperado = await repo.ObtenerTicketAsync(ticket.Id);

        Assert.NotNull(recuperado);
        Assert.Equal(EstadoTicket.Cerrado, recuperado.Estado);
        Assert.Equal(tecnico.Id, recuperado.TecnicoId);
        Assert.NotNull(recuperado.ResueltoUtc);
        Assert.NotNull(recuperado.CerradoUtc);
        Assert.Equal(5, recuperado.Seguimientos.Count);
        Assert.Contains(recuperado.Seguimientos, s => s.Tipo == TipoSeguimiento.Resuelto && s.TecnicoId == tecnico.Id);
        Assert.Contains(recuperado.Seguimientos, s => s.Tipo == TipoSeguimiento.Cerrado && s.TecnicoId == tecnico.Id);
    }

    [Fact]
    public async Task CorreoClienteDuplicado_RespetaIndiceUnicoEnSqlServer()
    {
        await using var db = CrearContexto();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var correo = $"duplicado.{sufijo}@tf15.local";

        db.Clientes.Add(new Cliente("Cliente Uno", correo));
        await db.SaveChangesAsync();

        db.Clientes.Add(new Cliente("Cliente Dos", correo.ToUpperInvariant()));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task SeguimientoResueltoSinTecnico_RespetaCheckConstraintEnSqlServer()
    {
        await using var db = CrearContexto();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var cliente = new Cliente($"Cliente CK {sufijo}", $"ck.{sufijo}@tf15.local");
        var categoria = new Categoria($"Cat CK {sufijo}");
        db.Clientes.Add(cliente);
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync();

        var ticket = new Ticket(
            cliente.Id,
            categoria.Id,
            "Prueba constraint",
            "Verificación de CK_Seguimientos_TipoTecnico.",
            Prioridad.Media,
            "mesa@tf15.local",
            DateTimeOffset.UtcNow);

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<SqlException>(() =>
            db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO dbo.Seguimientos (TicketId, Tipo, Detalle, Autor, FechaUtc, TecnicoId)
                VALUES ({ticket.Id}, 'Resuelto', 'Intento inválido', 'sistema', SYSUTCDATETIME(), NULL)
                """));
    }

    [Fact]
    public async Task ConsultasDeCatalogosYTickets_RetornanRegistrosPersistidos()
    {
        await using var db = CrearContexto();
        await using var tx = await db.Database.BeginTransactionAsync();

        var sufijo = Guid.NewGuid().ToString("N")[..8];
        var cliente = new Cliente($"Cliente Cat {sufijo}", $"cat.{sufijo}@tf15.local");
        var categoria = new Categoria($"Cat List {sufijo}");
        var tecnico = new Tecnico($"Tec List {sufijo}", $"teclist.{sufijo}@tf15.local");

        db.Clientes.Add(cliente);
        db.Categorias.Add(categoria);
        db.Tecnicos.Add(tecnico);
        await db.SaveChangesAsync();

        var repo = new SoporteRepository(db);
        repo.AgregarTicket(new Ticket(
            cliente.Id, categoria.Id, "Ticket consulta", "Detalle",
            Prioridad.Baja, "mesa@tf15.local", DateTimeOffset.UtcNow));
        await repo.GuardarCambiosAsync();

        Assert.Contains(await repo.ObtenerClientesAsync(), c => c.Id == cliente.Id);
        Assert.Contains(await repo.ObtenerCategoriasAsync(), c => c.Id == categoria.Id);
        Assert.Contains(await repo.ObtenerTecnicosAsync(), t => t.Id == tecnico.Id);
        Assert.NotEmpty(await repo.ObtenerTicketsAsync());
    }
}

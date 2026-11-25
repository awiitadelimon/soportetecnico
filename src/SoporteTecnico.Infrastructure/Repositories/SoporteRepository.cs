
using Microsoft.EntityFrameworkCore;
using SoporteTecnico.Application.Contracts;
using SoporteTecnico.Domain.Entities;
using SoporteTecnico.Infrastructure.Data;

namespace SoporteTecnico.Infrastructure.Repositories;

public class SoporteRepository(SoporteDbContext db)
    : ISoporteRepository
{
    public Task<Cliente?> ObtenerClienteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return db.Clientes.FirstOrDefaultAsync(
            c => c.Id == id, cancellationToken);
    }

    public Task<Tecnico?> ObtenerTecnicoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return db.Tecnicos.FirstOrDefaultAsync(
            t => t.Id == id, cancellationToken);
    }

    public Task<Categoria?> ObtenerCategoriaAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return db.Categorias.FirstOrDefaultAsync(
            c => c.Id == id, cancellationToken);
    }

    public Task<Ticket?> ObtenerTicketAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return db.Tickets
            .Include(t => t.Seguimientos)
            .FirstOrDefaultAsync(
                t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Cliente>> ObtenerClientesAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tecnico>> ObtenerTecnicosAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Tecnicos
            .AsNoTracking()
            .OrderBy(t => t.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Categoria>> ObtenerCategoriasAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> ObtenerTicketsAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Tickets
            .AsNoTracking()
            .Include(t => t.Seguimientos)
            .OrderByDescending(t => t.CreadoUtc)
            .ToListAsync(cancellationToken);
    }

    public void AgregarTicket(Ticket ticket)
    {
        db.Tickets.Add(ticket);
    }

    public async Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        await db.SaveChangesAsync(cancellationToken);
    }
}

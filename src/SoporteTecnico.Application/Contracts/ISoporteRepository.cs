using SoporteTecnico.Domain.Entities;

namespace SoporteTecnico.Application.Contracts;

public interface ISoporteRepository
{
    Task<Cliente?> ObtenerClienteAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Tecnico?> ObtenerTecnicoAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Categoria?> ObtenerCategoriaAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Ticket?> ObtenerTicketAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Cliente>> ObtenerClientesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tecnico>> ObtenerTecnicosAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Categoria>> ObtenerCategoriasAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> ObtenerTicketsAsync(
        CancellationToken cancellationToken = default);

    void AgregarTicket(Ticket ticket);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}
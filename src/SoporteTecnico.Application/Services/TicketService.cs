
using SoporteTecnico.Application.Contracts;
using SoporteTecnico.Application.DTOs;
using SoporteTecnico.Domain.Entities;

namespace SoporteTecnico.Application.Services;

public class TicketService(ISoporteRepository repository)
{
    public async Task<TicketDto> CrearAsync(
        CrearTicketRequest request,
        string autor,
        CancellationToken cancellationToken = default)
    {
        var cliente = await repository.ObtenerClienteAsync(
            request.ClienteId, cancellationToken);

        if (cliente is null)
            throw new KeyNotFoundException("El cliente no existe.");

        var categoria = await repository.ObtenerCategoriaAsync(
            request.CategoriaId, cancellationToken);

        if (categoria is null)
            throw new KeyNotFoundException("La categoría no existe.");

        var fechaUtc = DateTimeOffset.UtcNow;

        var ticket = new Ticket(
            request.ClienteId,
            request.CategoriaId,
            request.Asunto,
            request.Descripcion,
            request.Prioridad,
            autor,
            fechaUtc);

        repository.AgregarTicket(ticket);

        await repository.GuardarCambiosAsync(cancellationToken);

        return ConvertirADto(ticket);
    }

    public async Task<TicketDto> AsignarAsync(
        int ticketId,
        AsignarTicketRequest request,
        string autor,
        CancellationToken cancellationToken = default)
    {
        var ticket = await ObtenerTicketObligatorioAsync(
            ticketId, cancellationToken);

        var tecnico = await repository.ObtenerTecnicoAsync(
            request.TecnicoId, cancellationToken);

        if (tecnico is null)
            throw new KeyNotFoundException("El técnico no existe.");

        ticket.Asignar(tecnico, autor, DateTimeOffset.UtcNow);

        await repository.GuardarCambiosAsync(cancellationToken);

        return ConvertirADto(ticket);
    }

    public async Task<TicketDto> AgregarNotaAsync(
        int ticketId,
        AgregarNotaRequest request,
        string autor,
        CancellationToken cancellationToken = default)
    {
        var ticket = await ObtenerTicketObligatorioAsync(
            ticketId, cancellationToken);

        ticket.AgregarNota(
            request.Detalle,
            autor,
            DateTimeOffset.UtcNow);

        await repository.GuardarCambiosAsync(cancellationToken);

        return ConvertirADto(ticket);
    }

    public async Task<TicketDto> ResolverAsync(
    int ticketId,
    ResolverTicketRequest request,
    string autor,
    CancellationToken cancellationToken = default)
{
    var ticket = await ObtenerTicketObligatorioAsync(
        ticketId, cancellationToken);

    ticket.Resolver(
        request.Solucion,
        autor,
        DateTimeOffset.UtcNow);

    await repository.GuardarCambiosAsync(cancellationToken);

    return ConvertirADto(ticket);
}

    public async Task<TicketDto> CerrarAsync(
        int ticketId,
        string autor,
        CancellationToken cancellationToken = default)
    {
        var ticket = await ObtenerTicketObligatorioAsync(
            ticketId, cancellationToken);

        ticket.Cerrar(autor, DateTimeOffset.UtcNow);

        await repository.GuardarCambiosAsync(cancellationToken);

        return ConvertirADto(ticket);
    }

    public async Task<TicketDto> ObtenerAsync(
        int ticketId,
        CancellationToken cancellationToken = default)
    {
        var ticket = await ObtenerTicketObligatorioAsync(
            ticketId, cancellationToken);

        return ConvertirADto(ticket);
    }

    public async Task<IReadOnlyList<TicketDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var tickets = await repository.ObtenerTicketsAsync(
            cancellationToken);

        return tickets.Select(ConvertirADto).ToList();
    }

    private async Task<Ticket> ObtenerTicketObligatorioAsync(
        int ticketId,
        CancellationToken cancellationToken)
    {
        var ticket = await repository.ObtenerTicketAsync(
            ticketId, cancellationToken);

        return ticket ?? throw new KeyNotFoundException(
            $"No existe el ticket con ID {ticketId}.");
    }

    private static TicketDto ConvertirADto(Ticket ticket)
    {
        var seguimientos = ticket.Seguimientos
            .Select(s => new SeguimientoDto(
                s.Id,
                s.Tipo,
                s.Detalle,
                s.Autor,
                s.FechaUtc,
                s.TecnicoId))
            .ToList();

        return new TicketDto(
            ticket.Id,
            ticket.ClienteId,
            ticket.CategoriaId,
            ticket.TecnicoId,
            ticket.Asunto,
            ticket.Descripcion,
            ticket.Prioridad,
            ticket.Estado,
            ticket.CreadoUtc,
            ticket.ResueltoUtc,
            ticket.CerradoUtc,
            seguimientos);
    }
}

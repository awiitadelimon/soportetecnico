
using SoporteTecnico.Domain.Enums;

namespace SoporteTecnico.Application.DTOs;

public record CrearTicketRequest(
    int ClienteId,
    int CategoriaId,
    string Asunto,
    string Descripcion,
    Prioridad Prioridad);

public record AsignarTicketRequest(
    int TecnicoId);

public record AgregarNotaRequest(
    string Detalle);

public record ResolverTicketRequest(
    string Solucion);

public record SeguimientoDto(
    int Id,
    TipoSeguimiento Tipo,
    string Detalle,
    string Autor,
    DateTimeOffset FechaUtc,
    int? TecnicoId);

public record TicketDto(
    int Id,
    int ClienteId,
    int CategoriaId,
    int? TecnicoId,
    string Asunto,
    string Descripcion,
    Prioridad Prioridad,
    EstadoTicket Estado,
    DateTimeOffset CreadoUtc,
    DateTimeOffset? ResueltoUtc,
    DateTimeOffset? CerradoUtc,
    IReadOnlyList<SeguimientoDto> Seguimientos);

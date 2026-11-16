
using SoporteTecnico.Domain.Enums;
using SoporteTecnico.Domain.Exceptions;

namespace SoporteTecnico.Domain.Entities;

public class Seguimiento
{
    public int Id { get; private set; }

    public int TicketId { get; private set; }

    public TipoSeguimiento Tipo { get; private set; }

    public string Detalle { get; private set; } = string.Empty;

    public string Autor { get; private set; } = string.Empty;

    public DateTimeOffset FechaUtc { get; private set; }

    public int? TecnicoId { get; private set; }

    public Ticket Ticket { get; private set; } = null!;

    private Seguimiento()
    {
    }

    internal Seguimiento(
        Ticket ticket,
        TipoSeguimiento tipo,
        string detalle,
        string autor,
        DateTimeOffset fechaUtc,
        int? tecnicoId = null)
    {
        if (ticket is null)
            throw new DomainException("El ticket asociado es obligatorio.");

        if (!Enum.IsDefined(tipo))
            throw new DomainException("El tipo de seguimiento no es válido.");

        if (string.IsNullOrWhiteSpace(detalle) || detalle.Trim().Length > 1000)
            throw new DomainException(
                "El detalle debe tener entre 1 y 1000 caracteres.");

        if (string.IsNullOrWhiteSpace(autor) || autor.Trim().Length > 120)
            throw new DomainException(
                "El autor debe tener entre 1 y 120 caracteres.");

        if (fechaUtc == default)
            throw new DomainException("La fecha no es válida.");

        if (tecnicoId.HasValue && tecnicoId.Value <= 0)
            throw new DomainException("El identificador del técnico no es válido.");

        if ((tipo == TipoSeguimiento.Asignado ||
             tipo == TipoSeguimiento.Reasignado) && !tecnicoId.HasValue)
            throw new DomainException(
                "La asignación requiere identificar al técnico.");

        Ticket = ticket;
        Tipo = tipo;
        Detalle = detalle.Trim();
        Autor = autor.Trim();
        FechaUtc = fechaUtc.ToUniversalTime();
        TecnicoId = tecnicoId;
    }
}

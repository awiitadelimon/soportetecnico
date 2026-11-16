


using SoporteTecnico.Domain.Enums;
using SoporteTecnico.Domain.Exceptions;

namespace SoporteTecnico.Domain.Entities;

public class Ticket
{
    private readonly List<Seguimiento> _seguimientos = new();

    public int Id { get; private set; }
    public int ClienteId { get; private set; }
    public int CategoriaId { get; private set; }
    public int? TecnicoId { get; private set; }
    public string Asunto { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public Prioridad Prioridad { get; private set; }
    public EstadoTicket Estado { get; private set; }
    public DateTimeOffset CreadoUtc { get; private set; }
    public DateTimeOffset? ResueltoUtc { get; private set; }
    public DateTimeOffset? CerradoUtc { get; private set; }
    public IReadOnlyCollection<Seguimiento> Seguimientos => _seguimientos.AsReadOnly();

    private Ticket() { }

    public Ticket(int clienteId, int categoriaId, string asunto, string descripcion,
        Prioridad prioridad, string autor, DateTimeOffset fechaUtc)
    {
        if (clienteId <= 0) throw new DomainException("El cliente no es válido.");
        if (categoriaId <= 0) throw new DomainException("La categoría no es válida.");
        if (string.IsNullOrWhiteSpace(asunto) || asunto.Trim().Length > 150)
            throw new DomainException("El asunto debe tener entre 1 y 150 caracteres.");
        if (string.IsNullOrWhiteSpace(descripcion) || descripcion.Trim().Length > 1000)
            throw new DomainException("La descripción debe tener entre 1 y 1000 caracteres.");
        if (!Enum.IsDefined(prioridad)) throw new DomainException("La prioridad no es válida.");
        if (fechaUtc == default) throw new DomainException("La fecha de creación no es válida.");

        ClienteId = clienteId;
        CategoriaId = categoriaId;
        Asunto = asunto.Trim();
        Descripcion = descripcion.Trim();
        Prioridad = prioridad;
        Estado = EstadoTicket.Abierto;
        CreadoUtc = fechaUtc.ToUniversalTime();

        RegistrarSeguimiento(TipoSeguimiento.Creado, "Ticket creado.", autor, CreadoUtc);
    }

    public void Asignar(Tecnico tecnico, string autor, DateTimeOffset fechaUtc)
    {
        if (tecnico is null) throw new DomainException("El técnico es obligatorio.");
        if (!tecnico.Activo) throw new DomainException("No se puede asignar un técnico inactivo.");
        if (Estado != EstadoTicket.Abierto && Estado != EstadoTicket.Asignado)
            throw new TicketStateException("Solo se pueden asignar tickets abiertos o ya asignados.");
        if (TecnicoId == tecnico.Id)
            throw new DomainException("El ticket ya está asignado a ese técnico.");

        DateTimeOffset fecha = ValidarFecha(fechaUtc);
        TipoSeguimiento tipo = Estado == EstadoTicket.Abierto
            ? TipoSeguimiento.Asignado : TipoSeguimiento.Reasignado;

        RegistrarSeguimiento(tipo,
            tipo == TipoSeguimiento.Asignado
                ? "Ticket asignado a un técnico." : "Ticket reasignado a otro técnico.",
            autor, fecha, tecnico.Id);

        TecnicoId = tecnico.Id;
        Estado = EstadoTicket.Asignado;
    }

    public void AgregarNota(string detalle, string autor, DateTimeOffset fechaUtc)
    {
        if (Estado != EstadoTicket.Abierto && Estado != EstadoTicket.Asignado)
            throw new TicketStateException("No se pueden agregar notas a un ticket resuelto o cerrado.");

        DateTimeOffset fecha = ValidarFecha(fechaUtc);
        RegistrarSeguimiento(TipoSeguimiento.Nota, detalle, autor, fecha, TecnicoId);
    }

    public void Resolver(string solucion, string autor, DateTimeOffset fechaUtc)
    {
        if (Estado != EstadoTicket.Asignado)
            throw new TicketStateException("Solo se pueden resolver tickets asignados.");

        DateTimeOffset fecha = ValidarFecha(fechaUtc);

        RegistrarSeguimiento(
            TipoSeguimiento.Resuelto,
            solucion,
            autor,
            fecha,
            TecnicoId);

        Estado = EstadoTicket.Resuelto;
        ResueltoUtc = fecha;
    }

    public void Cerrar(string autor, DateTimeOffset fechaUtc)
    {
        if (Estado != EstadoTicket.Resuelto)
            throw new TicketStateException("Solo se pueden cerrar tickets resueltos.");

        DateTimeOffset fecha = ValidarFecha(fechaUtc);
        RegistrarSeguimiento(TipoSeguimiento.Cerrado, "Ticket cerrado.", autor, fecha, TecnicoId);
        Estado = EstadoTicket.Cerrado;
        CerradoUtc = fecha;
    }

    private DateTimeOffset ValidarFecha(DateTimeOffset fechaUtc)
    {
        if (fechaUtc == default) throw new DomainException("La fecha no es válida.");

        DateTimeOffset fecha = fechaUtc.ToUniversalTime();
        DateTimeOffset ultimaFecha = _seguimientos.Count > 0
            ? _seguimientos[^1].FechaUtc : CreadoUtc;

        if (fecha < ultimaFecha)
            throw new DomainException("La fecha no puede ser anterior al último seguimiento.");

        return fecha;
    }

    private void RegistrarSeguimiento(TipoSeguimiento tipo, string detalle, string autor,
        DateTimeOffset fechaUtc, int? tecnicoId = null)
    {
        var seguimiento = new Seguimiento(this, tipo, detalle, autor, fechaUtc, tecnicoId);
        _seguimientos.Add(seguimiento);
    }
}

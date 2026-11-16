
using System.Reflection;
using SoporteTecnico.Domain.Entities;
using SoporteTecnico.Domain.Enums;
using SoporteTecnico.Domain.Exceptions;

namespace SoporteTecnico.Domain.Tests;

public class TicketTests
{
    private static readonly DateTimeOffset FechaBase =
        new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

    private const string SolucionAplicada = "Se corrigió el problema.";

    private static Tecnico CrearTecnicoConId(int id, string nombre = "Ana Gómez", string email = "ana@soporte.com")
    {
        var tecnico = new Tecnico(nombre, email);
        typeof(Tecnico)
            .GetProperty(nameof(Tecnico.Id))!
            .SetValue(tecnico, id);
        return tecnico;
    }

    private static Ticket CrearTicketValido()
    {
        return new Ticket(
            clienteId: 1,
            categoriaId: 2,
            asunto: "Falla en la red",
            descripcion: "No hay conexión a internet en el segundo piso.",
            prioridad: Prioridad.Alta,
            autor: "cliente@empresa.com",
            fechaUtc: FechaBase);
    }

    [Fact]
    public void Constructor_DatosValidos_DebeCrearTicketAbiertoYSeguimientoInicial()
    {
        var ticket = CrearTicketValido();

        Assert.Equal(1, ticket.ClienteId);
        Assert.Equal(2, ticket.CategoriaId);
        Assert.Equal("Falla en la red", ticket.Asunto);
        Assert.Equal("No hay conexión a internet en el segundo piso.", ticket.Descripcion);
        Assert.Equal(Prioridad.Alta, ticket.Prioridad);
        Assert.Equal(EstadoTicket.Abierto, ticket.Estado);
        Assert.Null(ticket.TecnicoId);
        Assert.Equal(FechaBase, ticket.CreadoUtc);
        Assert.Null(ticket.ResueltoUtc);
        Assert.Null(ticket.CerradoUtc);

        var seguimiento = Assert.Single(ticket.Seguimientos);
        Assert.Equal(TipoSeguimiento.Creado, seguimiento.Tipo);
        Assert.Equal("Ticket creado.", seguimiento.Detalle);
        Assert.Equal("cliente@empresa.com", seguimiento.Autor);
        Assert.Equal(FechaBase, seguimiento.FechaUtc);
        Assert.Null(seguimiento.TecnicoId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ClienteIdInvalido_DebeLanzarDomainException(int clienteIdInvalido)
    {
        Assert.Throws<DomainException>(() => new Ticket(
            clienteId: clienteIdInvalido,
            categoriaId: 1,
            asunto: "Asunto válido",
            descripcion: "Descripción válida",
            prioridad: Prioridad.Media,
            autor: "usuario",
            fechaUtc: FechaBase));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_CategoriaIdInvalido_DebeLanzarDomainException(int categoriaIdInvalido)
    {
        Assert.Throws<DomainException>(() => new Ticket(
            clienteId: 1,
            categoriaId: categoriaIdInvalido,
            asunto: "Asunto válido",
            descripcion: "Descripción válida",
            prioridad: Prioridad.Media,
            autor: "usuario",
            fechaUtc: FechaBase));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_AsuntoVacio_DebeLanzarDomainException(string asuntoInvalido)
    {
        Assert.Throws<DomainException>(() => new Ticket(
            clienteId: 1,
            categoriaId: 1,
            asunto: asuntoInvalido,
            descripcion: "Descripción válida",
            prioridad: Prioridad.Media,
            autor: "usuario",
            fechaUtc: FechaBase));
    }

    [Fact]
    public void Constructor_AsuntoExcede150Caracteres_DebeLanzarDomainException()
    {
        string asuntoLargo = new('A', 151);

        Assert.Throws<DomainException>(() => new Ticket(
            clienteId: 1,
            categoriaId: 1,
            asunto: asuntoLargo,
            descripcion: "Descripción válida",
            prioridad: Prioridad.Media,
            autor: "usuario",
            fechaUtc: FechaBase));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_DescripcionVacia_DebeLanzarDomainException(string descripcionInvalida)
    {
        Assert.Throws<DomainException>(() => new Ticket(
            clienteId: 1,
            categoriaId: 1,
            asunto: "Asunto válido",
            descripcion: descripcionInvalida,
            prioridad: Prioridad.Media,
            autor: "usuario",
            fechaUtc: FechaBase));
    }

    [Fact]
    public void Constructor_DescripcionExcede1000Caracteres_DebeLanzarDomainException()
    {
        string descripcionLarga = new('D', 1001);

        Assert.Throws<DomainException>(() => new Ticket(
            clienteId: 1,
            categoriaId: 1,
            asunto: "Asunto válido",
            descripcion: descripcionLarga,
            prioridad: Prioridad.Media,
            autor: "usuario",
            fechaUtc: FechaBase));
    }

    [Fact]
    public void Constructor_PrioridadInvalida_DebeLanzarDomainException()
    {
        Assert.Throws<DomainException>(() => new Ticket(
            clienteId: 1,
            categoriaId: 1,
            asunto: "Asunto válido",
            descripcion: "Descripción válida",
            prioridad: (Prioridad)999,
            autor: "usuario",
            fechaUtc: FechaBase));
    }

    [Fact]
    public void Constructor_FechaDefault_DebeLanzarDomainException()
    {
        Assert.Throws<DomainException>(() => new Ticket(
            clienteId: 1,
            categoriaId: 1,
            asunto: "Asunto válido",
            descripcion: "Descripción válida",
            prioridad: Prioridad.Baja,
            autor: "usuario",
            fechaUtc: default));
    }

    [Fact]
    public void Asignar_TicketAbiertoATecnicoActivo_DebeCambiarEstadoYRegistrarSeguimiento()
    {
        var ticket = CrearTicketValido();
        var tecnico = CrearTecnicoConId(1);
        var fechaAsignacion = FechaBase.AddMinutes(10);

        ticket.Asignar(tecnico, "supervisor", fechaAsignacion);

        Assert.Equal(EstadoTicket.Asignado, ticket.Estado);
        Assert.Equal(1, ticket.TecnicoId);
        Assert.Equal(2, ticket.Seguimientos.Count);

        var ultimo = ticket.Seguimientos.Last();
        Assert.Equal(TipoSeguimiento.Asignado, ultimo.Tipo);
        Assert.Equal("Ticket asignado a un técnico.", ultimo.Detalle);
        Assert.Equal("supervisor", ultimo.Autor);
        Assert.Equal(fechaAsignacion, ultimo.FechaUtc);
        Assert.Equal(1, ultimo.TecnicoId);
    }

    [Fact]
    public void Asignar_TicketYaAsignadoAOtroTecnico_DebeReasignarYRegistrarSeguimiento()
    {
        var ticket = CrearTicketValido();
        var tecnico1 = CrearTecnicoConId(1, "Técnico 1", "t1@soporte.com");
        var tecnico2 = CrearTecnicoConId(2, "Técnico 2", "t2@soporte.com");

        ticket.Asignar(tecnico1, "supervisor", FechaBase.AddMinutes(10));
        ticket.Asignar(tecnico2, "supervisor", FechaBase.AddMinutes(20));

        Assert.Equal(EstadoTicket.Asignado, ticket.Estado);
        Assert.Equal(2, ticket.TecnicoId);
        Assert.Equal(3, ticket.Seguimientos.Count);

        var ultimo = ticket.Seguimientos.Last();
        Assert.Equal(TipoSeguimiento.Reasignado, ultimo.Tipo);
        Assert.Equal("Ticket reasignado a otro técnico.", ultimo.Detalle);
        Assert.Equal(2, ultimo.TecnicoId);
    }

    [Fact]
    public void Asignar_MismoTecnico_DebeLanzarDomainException()
    {
        var ticket = CrearTicketValido();
        var tecnico = CrearTecnicoConId(1);
        ticket.Asignar(tecnico, "supervisor", FechaBase.AddMinutes(10));

        Assert.Throws<DomainException>(() =>
            ticket.Asignar(tecnico, "supervisor", FechaBase.AddMinutes(20)));
    }

    [Fact]
    public void Asignar_TecnicoNulo_DebeLanzarDomainException()
    {
        var ticket = CrearTicketValido();

        Assert.Throws<DomainException>(() =>
            ticket.Asignar(null!, "supervisor", FechaBase.AddMinutes(10)));
    }

    [Fact]
    public void Asignar_TecnicoInactivo_DebeLanzarDomainException()
    {
        var ticket = CrearTicketValido();
        var tecnico = CrearTecnicoConId(1);
        tecnico.Desactivar();

        Assert.Throws<DomainException>(() =>
            ticket.Asignar(tecnico, "supervisor", FechaBase.AddMinutes(10)));
    }

    [Fact]
    public void Asignar_TicketResuelto_DebeLanzarTicketStateException()
    {
        var ticket = CrearTicketValido();
        var tecnico1 = CrearTecnicoConId(1);
        var tecnico2 = CrearTecnicoConId(2, "Otro", "otro@soporte.com");

        ticket.Asignar(tecnico1, "supervisor", FechaBase.AddMinutes(10));
        ticket.Resolver(SolucionAplicada, "tecnico1", FechaBase.AddMinutes(30));

        Assert.Throws<TicketStateException>(() =>
            ticket.Asignar(tecnico2, "supervisor", FechaBase.AddMinutes(40)));
    }

    [Fact]
    public void AgregarNota_TicketAbiertoOAsignado_DebeRegistrarSeguimiento()
    {
        var ticket = CrearTicketValido();
        var tecnico = CrearTecnicoConId(1);
        ticket.Asignar(tecnico, "supervisor", FechaBase.AddMinutes(5));

        var fechaNota = FechaBase.AddMinutes(15);
        ticket.AgregarNota("Se revisó el router.", "tecnico1", fechaNota);

        Assert.Equal(3, ticket.Seguimientos.Count);
        var nota = ticket.Seguimientos.Last();
        Assert.Equal(TipoSeguimiento.Nota, nota.Tipo);
        Assert.Equal("Se revisó el router.", nota.Detalle);
        Assert.Equal("tecnico1", nota.Autor);
        Assert.Equal(fechaNota, nota.FechaUtc);
        Assert.Equal(1, nota.TecnicoId);
    }

    [Fact]
    public void AgregarNota_TicketResueltoOCerrado_DebeLanzarTicketStateException()
    {
        var ticket = CrearTicketValido();
        var tecnico = CrearTecnicoConId(1);

        ticket.Asignar(tecnico, "supervisor", FechaBase.AddMinutes(5));
        ticket.Resolver(SolucionAplicada, "tecnico1", FechaBase.AddMinutes(20));

        Assert.Throws<TicketStateException>(() =>
            ticket.AgregarNota("Nota tardía", "tecnico1", FechaBase.AddMinutes(25)));
    }

    [Fact]
    public void Resolver_TicketAsignado_DebeCambiarEstado()
    {
        var ticket = CrearTicketValido();
        var tecnico = CrearTecnicoConId(1);
        ticket.Asignar(tecnico, "supervisor", FechaBase.AddMinutes(10));

        var fechaResolucion = FechaBase.AddMinutes(45);
        ticket.Resolver(SolucionAplicada, "tecnico1", fechaResolucion);

        Assert.Equal(EstadoTicket.Resuelto, ticket.Estado);
        Assert.Equal(fechaResolucion, ticket.ResueltoUtc);

        var ultimo = ticket.Seguimientos.Last();
        Assert.Equal(TipoSeguimiento.Resuelto, ultimo.Tipo);
        Assert.Equal(SolucionAplicada, ultimo.Detalle);
        Assert.Equal("tecnico1", ultimo.Autor);
        Assert.Equal(fechaResolucion, ultimo.FechaUtc);
        Assert.Equal(1, ultimo.TecnicoId);
    }

    [Fact]
    public void Resolver_TicketNoAsignado_DebeLanzarTicketStateException()
    {
        var ticket = CrearTicketValido();

        Assert.Throws<TicketStateException>(() =>
            ticket.Resolver(SolucionAplicada, "tecnico1", FechaBase.AddMinutes(10)));
    }

    [Fact]
    public void Cerrar_TicketResuelto_DebeCambiarEstado()
    {
        var ticket = CrearTicketValido();
        var tecnico = CrearTecnicoConId(1);
        ticket.Asignar(tecnico, "supervisor", FechaBase.AddMinutes(10));
        ticket.Resolver(SolucionAplicada, "tecnico1", FechaBase.AddMinutes(30));

        var fechaCierre = FechaBase.AddHours(1);
        ticket.Cerrar("supervisor", fechaCierre);

        Assert.Equal(EstadoTicket.Cerrado, ticket.Estado);
        Assert.Equal(fechaCierre, ticket.CerradoUtc);

        var ultimo = ticket.Seguimientos.Last();
        Assert.Equal(TipoSeguimiento.Cerrado, ultimo.Tipo);
        Assert.Equal("Ticket cerrado.", ultimo.Detalle);
        Assert.Equal("supervisor", ultimo.Autor);
        Assert.Equal(fechaCierre, ultimo.FechaUtc);
        Assert.Equal(1, ultimo.TecnicoId);
    }

    [Fact]
    public void Cerrar_TicketNoResuelto_DebeLanzarTicketStateException()
    {
        var ticket = CrearTicketValido();
        var tecnico = CrearTecnicoConId(1);
        ticket.Asignar(tecnico, "supervisor", FechaBase.AddMinutes(10));

        Assert.Throws<TicketStateException>(() =>
            ticket.Cerrar("supervisor", FechaBase.AddMinutes(20)));
    }

    [Fact]
    public void Operaciones_FechaAnteriorAlUltimoSeguimiento_DebeLanzarDomainException()
    {
        var ticket = CrearTicketValido();
        var tecnico = CrearTecnicoConId(1);
        var fechaAsignacion = FechaBase.AddMinutes(20);
        ticket.Asignar(tecnico, "supervisor", fechaAsignacion);

        Assert.Throws<DomainException>(() =>
            ticket.AgregarNota("Nota fuera de tiempo", "tecnico1", FechaBase.AddMinutes(10)));
    }
}

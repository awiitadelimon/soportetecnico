using System.ComponentModel.DataAnnotations;
using SoporteTecnico.Frontend.Models;

namespace SoporteTecnico.Frontend.Services;

public sealed class MockTf15Service : ITf15Service
{
    public const string CorreoAdministrador = "admin@soportetecnico.test";
    public const string CorreoGestor = "gestor@soportetecnico.test";
    public const string ClaveDemostracion = "Tf15Demo2026!";

    private const string Abierto = "Abierto";
    private const string Asignado = "Asignado";
    private const string Resuelto = "Resuelto";
    private const string Cerrado = "Cerrado";

    private readonly List<Categoria> categorias = [new(1, "Conectividad"), new(2, "Equipos"), new(3, "Aplicaciones")];
    private readonly List<Cliente> clientes =
    [
        new(1, "MarÃƒÂ­a Reyes", "maria@example.invalid"),
        new(2, "Luis PeÃƒÂ±a", "luis@example.invalid"),
        new(3, "SofÃƒÂ­a DÃƒÂ­az", "sofia@example.invalid")
    ];
    private readonly List<Tecnico> tecnicos =
    [
        new(1, "Ana Mora", "ana@soporte.example.invalid", true),
        new(2, "Mateo NÃƒÂºÃƒÂ±ez", "mateo@soporte.example.invalid", true),
        new(3, "Elena Cruz", "elena@soporte.example.invalid", false)
    ];
    private readonly Dictionary<int, Ticket> tickets = new()
    {
        [1] = new(new(1, 1, "MarÃƒÂ­a Reyes", 1, "Conectividad", 1, "Ana Mora", "Acceso a la red interna",
            "Alta", Cerrado, Fecha(2, 13)), "El equipo perdiÃƒÂ³ la conexiÃƒÂ³n al cambiar de puesto.",
            Fecha(2, 15), Fecha(2, 16),
            [
                new(1, "Creado", "Ticket registrado.", "Mesa de ayuda", Fecha(2, 13), null),
                new(2, Asignado, "TÃƒÂ©cnico asignado: Ana Mora.", "Mesa de ayuda", Fecha(2, 14), 1),
                new(3, Resuelto, "Se sustituyÃƒÂ³ el cable de red y se verificÃƒÂ³ la conexiÃƒÂ³n.", "Mesa de ayuda", Fecha(2, 15), null),
                new(4, Cerrado, "Ticket cerrado.", "Mesa de ayuda", Fecha(2, 16), null)
            ]),
        [2] = new(new(2, 2, "Luis PeÃƒÂ±a", 2, "Equipos", 2, "Mateo NÃƒÂºÃƒÂ±ez", "Impresora sin respuesta",
            "Media", Asignado, Fecha(3, 13)), "La impresora aparece desconectada en el equipo del cliente.", null, null,
            [
                new(5, "Creado", "Ticket registrado.", "Mesa de ayuda", Fecha(3, 13), null),
                new(6, Asignado, "TÃƒÂ©cnico asignado: Mateo NÃƒÂºÃƒÂ±ez.", "Mesa de ayuda", Fecha(3, 14), 2)
            ]),
        [3] = new(new(3, 3, "SofÃƒÂ­a DÃƒÂ­az", 3, "Aplicaciones", null, null, "Error al abrir el portal",
            "Critica", Abierto, Fecha(4, 13)), "El portal interno muestra un error al abrir la pantalla de inicio.", null, null,
            [new(7, "Creado", "Ticket registrado.", "Mesa de ayuda", Fecha(4, 13), null)]),
        [4] = new(new(4, 1, "MarÃƒÂ­a Reyes", 3, "Aplicaciones", 1, "Ana Mora", "ActualizaciÃƒÂ³n de aplicaciÃƒÂ³n",
            "Baja", Resuelto, Fecha(4, 14)), "La estaciÃƒÂ³n de trabajo usa una versiÃƒÂ³n anterior.", Fecha(4, 16), null,
            [
                new(8, "Creado", "Ticket registrado.", "Mesa de ayuda", Fecha(4, 14), null),
                new(9, Asignado, "TÃƒÂ©cnico asignado: Ana Mora.", "Mesa de ayuda", Fecha(4, 15), 1),
                new(10, Resuelto, "Se instalÃƒÂ³ la versiÃƒÂ³n autorizada y se comprobÃƒÂ³ el inicio.", "Mesa de ayuda", Fecha(4, 16), null)
            ])
    };

    private int siguienteCategoria = 4, siguienteCliente = 4, siguienteTecnico = 4, siguienteTicket = 5, siguienteSeguimiento = 11;

    public Sesion? SesionActual { get; private set; }

    public Task IniciarSesionAsync(LoginInput input)
    {
        var rol = input.Email.Trim().ToLowerInvariant() switch
        {
            CorreoAdministrador => "Administrador",
            CorreoGestor => "Gestor",
            _ => null
        };
        if (rol is null || input.Contrasena != ClaveDemostracion)
            throw new InvalidOperationException("Correo o contraseÃƒÂ±a incorrectos.");

        SesionActual = new Sesion(string.Empty, rol + " de prueba", [rol]);
        return Task.CompletedTask;
    }

    public void CerrarSesion() => SesionActual = null;

    public Task<Resumen> ResumenAsync() => Task.FromResult(new Resumen(
        Contar(Abierto), Contar(Asignado), Contar(Resuelto), Contar(Cerrado), tecnicos.Count(t => t.Activo)));

    public Task<Categoria[]> CategoriasAsync() => Task.FromResult(categorias.ToArray());
    public Task<Cliente[]> ClientesAsync() => Task.FromResult(clientes.ToArray());
    public Task<Tecnico[]> TecnicosAsync() => Task.FromResult(tecnicos.ToArray());
    public Task<TicketResumen[]> TicketsAsync() =>
        Task.FromResult(tickets.Values.OrderByDescending(t => t.Datos.Id).Select(t => ActualizarNombres(t.Datos)).ToArray());

    public Task<Ticket> TicketAsync(int id) => Task.FromResult(ParaMostrar(BuscarTicket(id)));

    public Task<Categoria> GuardarCategoriaAsync(int? id, CategoriaInput input)
    {
        var anterior = id is int value ? BuscarCategoria(value) : null;
        var nombre = Texto(input.Nombre, 100, "El nombre");
        if (categorias.Any(c => c.Id != id && c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ya existe una categorÃƒÂ­a con ese nombre.");

        var categoria = new Categoria(anterior?.Id ?? siguienteCategoria++, nombre);
        Guardar(categorias, anterior, categoria);
        return Task.FromResult(categoria);
    }

    public Task EliminarCategoriaAsync(int id)
    {
        var categoria = BuscarCategoria(id);
        if (tickets.Values.Any(t => t.Datos.CategoriaId == id))
            throw new InvalidOperationException("La categorÃƒÂ­a tiene tickets asociados.");
        categorias.Remove(categoria);
        return Task.CompletedTask;
    }

    public Task<Cliente> GuardarClienteAsync(int? id, ClienteInput input)
    {
        var anterior = id is int value ? BuscarCliente(value) : null;
        var nombre = Texto(input.Nombre, 120, "El nombre");
        var correo = Correo(input.Email);
        if (clientes.Any(c => c.Id != id && c.Email.Equals(correo, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ya existe un cliente con ese correo.");

        var cliente = new Cliente(anterior?.Id ?? siguienteCliente++, nombre, correo);
        Guardar(clientes, anterior, cliente);
        return Task.FromResult(cliente);
    }

    public Task EliminarClienteAsync(int id)
    {
        var cliente = BuscarCliente(id);
        if (tickets.Values.Any(t => t.Datos.ClienteId == id))
            throw new InvalidOperationException("El cliente tiene tickets asociados.");
        clientes.Remove(cliente);
        return Task.CompletedTask;
    }

    public Task<Tecnico> GuardarTecnicoAsync(int? id, TecnicoInput input)
    {
        var anterior = id is int value ? BuscarTecnico(value) : null;
        var nombre = Texto(input.Nombre, 120, "El nombre");
        var correo = Correo(input.Email);
        if (tecnicos.Any(t => t.Id != id && t.Email.Equals(correo, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ya existe un tÃƒÂ©cnico con ese correo.");
        if (!input.Activo && tickets.Values.Any(t => t.Datos.TecnicoId == id && t.Datos.Estado == Asignado))
            throw new InvalidOperationException("El tÃƒÂ©cnico tiene tickets pendientes de resolver.");

        var tecnico = new Tecnico(anterior?.Id ?? siguienteTecnico++, nombre, correo, input.Activo);
        Guardar(tecnicos, anterior, tecnico);
        return Task.FromResult(tecnico);
    }

    public Task EliminarTecnicoAsync(int id)
    {
        var tecnico = BuscarTecnico(id);
        if (tickets.Values.Any(t => t.Datos.TecnicoId == id || t.Seguimientos.Any(s => s.TecnicoId == id)))
            throw new InvalidOperationException("El tÃƒÂ©cnico tiene historial de asignaciones.");
        tecnicos.Remove(tecnico);
        return Task.CompletedTask;
    }

    public Task<Ticket> CrearTicketAsync(TicketInput input)
    {
        var cliente = BuscarCliente(input.ClienteId);
        var categoria = BuscarCategoria(input.CategoriaId);
        var asunto = Texto(input.Asunto, 150, "El asunto");
        var descripcion = Texto(input.Descripcion, 1000, "La descripciÃƒÂ³n");
        if (!Prioridades.Todas.Contains(input.Prioridad))
            throw new InvalidOperationException("Seleccione una prioridad vÃƒÂ¡lida.");

        var ahora = DateTimeOffset.UtcNow;
        var datos = new TicketResumen(siguienteTicket++, cliente.Id, cliente.Nombre, categoria.Id, categoria.Nombre,
            null, null, asunto, input.Prioridad, Abierto, ahora);
        return Registrar(new Ticket(datos, descripcion, null, null, []), "Creado", "Ticket registrado.", ahora);
    }

    public Task<Ticket> AsignarAsync(int id, AsignacionInput input)
    {
        var ticket = BuscarEditable(id);
        var tecnico = BuscarTecnico(input.TecnicoId);
        if (!tecnico.Activo)
            throw new InvalidOperationException("Seleccione un tÃƒÂ©cnico activo.");
        if (ticket.Datos.TecnicoId == tecnico.Id)
            throw new InvalidOperationException("El ticket ya estÃƒÂ¡ asignado a ese tÃƒÂ©cnico.");

        var tipo = ticket.Datos.Estado == Abierto ? Asignado : "Reasignado";
        var datos = ticket.Datos with { Estado = Asignado, TecnicoId = tecnico.Id, TecnicoNombre = tecnico.Nombre };
        return Registrar(ticket with { Datos = datos }, tipo, $"TÃƒÂ©cnico asignado: {tecnico.Nombre}.", Momento(ticket), tecnico.Id);
    }

    public Task<Ticket> AnotarAsync(int id, NotaInput input)
    {
        var ticket = BuscarEditable(id);
        return Registrar(ticket, "Nota", Texto(input.Detalle, 1000, "La nota"), Momento(ticket));
    }

    public Task<Ticket> ResolverAsync(int id, ResolucionInput input)
    {
        var ticket = BuscarTicket(id);
        if (ticket.Datos.Estado != Asignado)
            throw new InvalidOperationException("Solo se resuelve un ticket asignado.");
        var solucion = Texto(input.Solucion, 1000, "La soluciÃƒÂ³n");
        var ahora = Momento(ticket);
        return Registrar(ticket with { Datos = ticket.Datos with { Estado = Resuelto }, ResueltoUtc = ahora },
            Resuelto, solucion, ahora);
    }

    public Task<Ticket> CerrarAsync(int id)
    {
        var ticket = BuscarTicket(id);
        if (ticket.Datos.Estado != Resuelto)
            throw new InvalidOperationException("Solo se cierra un ticket resuelto.");
        var ahora = Momento(ticket);
        return Registrar(ticket with { Datos = ticket.Datos with { Estado = Cerrado }, CerradoUtc = ahora },
            Cerrado, "Ticket cerrado.", ahora);
    }

    private Task<Ticket> Registrar(Ticket ticket, string tipo, string detalle, DateTimeOffset fecha, int? tecnicoId = null)
    {
        var autor = SesionActual?.Nombre ?? throw new InvalidOperationException("Inicie sesiÃƒÂ³n para registrar el seguimiento.");
        var evento = new Seguimiento(siguienteSeguimiento++, tipo, detalle, autor, fecha, tecnicoId);
        var actual = ticket with { Seguimientos = [.. ticket.Seguimientos, evento] };
        tickets[actual.Datos.Id] = actual;
        return Task.FromResult(ParaMostrar(actual));
    }

    private int Contar(string estado) => tickets.Values.Count(t => t.Datos.Estado == estado);
    private Ticket ParaMostrar(Ticket ticket) =>
        ticket with { Datos = ActualizarNombres(ticket.Datos), Seguimientos = [.. ticket.Seguimientos] };

    private TicketResumen ActualizarNombres(TicketResumen ticket) => ticket with
    {
        ClienteNombre = BuscarCliente(ticket.ClienteId).Nombre,
        CategoriaNombre = BuscarCategoria(ticket.CategoriaId).Nombre,
        TecnicoNombre = ticket.TecnicoId is int id ? BuscarTecnico(id).Nombre : null
    };

    private Ticket BuscarEditable(int id)
    {
        var ticket = BuscarTicket(id);
        if (ticket.Datos.Estado is not (Abierto or Asignado))
            throw new InvalidOperationException("El ticket ya no admite asignaciones ni notas.");
        return ticket;
    }

    private Ticket BuscarTicket(int id) => tickets.TryGetValue(id, out var ticket)
        ? ticket : throw new InvalidOperationException("No se encontrÃƒÂ³ el ticket.");
    private Cliente BuscarCliente(int id) => clientes.Find(c => c.Id == id)
        ?? throw new InvalidOperationException("No se encontrÃƒÂ³ el cliente.");
    private Tecnico BuscarTecnico(int id) => tecnicos.Find(t => t.Id == id)
        ?? throw new InvalidOperationException("No se encontrÃƒÂ³ el tÃƒÂ©cnico.");
    private Categoria BuscarCategoria(int id) => categorias.Find(c => c.Id == id)
        ?? throw new InvalidOperationException("No se encontrÃƒÂ³ la categorÃƒÂ­a.");

    private static DateTimeOffset Fecha(int dia, int hora) => new(2026, 11, dia, hora, 0, 0, TimeSpan.Zero);

    private static DateTimeOffset Momento(Ticket ticket)
    {
        var ahora = DateTimeOffset.UtcNow;
        var anterior = ticket.Seguimientos[^1].FechaUtc;
        return ahora >= anterior ? ahora : anterior;
    }

    private static void Guardar<T>(List<T> lista, T? anterior, T actual) where T : class
    {
        if (anterior is null)
            lista.Add(actual);
        else
            lista[lista.IndexOf(anterior)] = actual;
    }

    private static string Correo(string entrada)
    {
        var valor = Texto(entrada, 254, "El correo");
        if (!new EmailAddressAttribute().IsValid(valor))
            throw new InvalidOperationException("Indique un correo vÃƒÂ¡lido.");
        return valor;
    }

    private static string Texto(string entrada, int maximo, string campo)
    {
        var valor = entrada.Trim();
        if (valor.Length == 0 || valor.Length > maximo)
            throw new InvalidOperationException($"{campo} requiere entre 1 y {maximo} caracteres.");
        return valor;
    }
}

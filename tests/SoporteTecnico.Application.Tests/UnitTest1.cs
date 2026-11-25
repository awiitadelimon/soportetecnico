using SoporteTecnico.Application.Contracts;
using SoporteTecnico.Application.DTOs;
using SoporteTecnico.Application.Services;
using SoporteTecnico.Domain.Entities;
using SoporteTecnico.Domain.Enums;
using SoporteTecnico.Domain.Exceptions;

namespace SoporteTecnico.Application.Tests;

public class TicketServiceTests
{
    private static readonly DateTimeOffset Fecha =
        new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CrearAsync_DatosValidos_CreaTicket()
    {
        var repository = CrearRepositorioCompleto();
        var service = new TicketService(repository);

        var request = new CrearTicketRequest(
            1, 1, "Problema de red",
            "No hay conexión a Internet.",
            Prioridad.Alta);

        var resultado = await service.CrearAsync(
            request, "Administrador");

        Assert.Equal("Problema de red", resultado.Asunto);
        Assert.Equal(EstadoTicket.Abierto, resultado.Estado);
        Assert.Single(resultado.Seguimientos);
        Assert.True(repository.Guardado);
    }

    [Fact]
    public async Task CrearAsync_ClienteInexistente_LanzaExcepcion()
    {
        var repository = new FakeSoporteRepository();
        repository.AgregarCategoria(new Categoria("Conectividad"));

        var service = new TicketService(repository);

        var request = new CrearTicketRequest(
            1, 1, "Problema de red",
            "Descripción del problema.",
            Prioridad.Media);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CrearAsync(request, "Administrador"));
    }

    [Fact]
    public async Task CrearAsync_CategoriaInexistente_LanzaExcepcion()
    {
        var repository = new FakeSoporteRepository();
        repository.AgregarCliente(
            new Cliente("María Reyes", "maria@gmail.com"));

        var service = new TicketService(repository);

        var request = new CrearTicketRequest(
            1, 1, "Problema de red",
            "Descripción del problema.",
            Prioridad.Media);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CrearAsync(request, "Administrador"));
    }

    [Fact]
    public async Task AsignarAsync_TecnicoValido_AsignaTicket()
    {
        var repository = CrearRepositorioCompleto();
        var ticket = CrearTicket();
        var tecnico = new Tecnico("Ana Mora", "ana@gmail.com");

        repository.AgregarTicket(ticket);
        repository.AgregarTecnico(tecnico);

        var service = new TicketService(repository);

        var resultado = await service.AsignarAsync(
            1,
            new AsignarTicketRequest(1),
            "Administrador");

        Assert.Equal(EstadoTicket.Asignado, resultado.Estado);
        Assert.Equal(1, resultado.TecnicoId);
        Assert.True(repository.Guardado);
    }

    [Fact]
    public async Task AsignarAsync_TicketInexistente_LanzaExcepcion()
    {
        var repository = CrearRepositorioCompleto();
        repository.AgregarTecnico(
            new Tecnico("Ana Mora", "ana@gmail.com"));

        var service = new TicketService(repository);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.AsignarAsync(
                99,
                new AsignarTicketRequest(1),
                "Administrador"));
    }

    [Fact]
    public async Task AsignarAsync_TecnicoInexistente_LanzaExcepcion()
    {
        var repository = CrearRepositorioCompleto();
        repository.AgregarTicket(CrearTicket());

        var service = new TicketService(repository);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.AsignarAsync(
                1,
                new AsignarTicketRequest(99),
                "Administrador"));
    }

    [Fact]
    public async Task ResolverAsync_TicketSinAsignar_LanzaTicketStateException()
    {
        var repository = CrearRepositorioCompleto();
        repository.AgregarTicket(CrearTicket());

        var service = new TicketService(repository);

        await Assert.ThrowsAsync<TicketStateException>(
            () => service.ResolverAsync(
                1,
                new ResolverTicketRequest("Solución aplicada."),
                "Administrador"));
    }

    [Fact]
    public async Task FlujoCompleto_AsignarNotaResolverYCerrar_ActualizaEstadoYSeguimientos()
    {
        var repository = CrearRepositorioCompleto();
        repository.AgregarTicket(CrearTicket());
        repository.AgregarTecnico(new Tecnico("Ana Mora", "ana@gmail.com"));

        var service = new TicketService(repository);

        await service.AsignarAsync(1, new AsignarTicketRequest(1), "Administrador");
        await service.AgregarNotaAsync(1, new AgregarNotaRequest("Revisando router."), "Ana Mora");
        await service.ResolverAsync(1, new ResolverTicketRequest("Puerto restablecido."), "Ana Mora");
        var cerrado = await service.CerrarAsync(1, "Administrador");

        Assert.Equal(EstadoTicket.Cerrado, cerrado.Estado);
        Assert.NotNull(cerrado.ResueltoUtc);
        Assert.NotNull(cerrado.CerradoUtc);
        Assert.Equal(5, cerrado.Seguimientos.Count);
    }

    private static FakeSoporteRepository CrearRepositorioCompleto()
    {
        var repository = new FakeSoporteRepository();

        repository.AgregarCliente(
            new Cliente("María Reyes", "maria@gmail.com"));

        repository.AgregarCategoria(
            new Categoria("Conectividad"));

        return repository;
    }

    private static Ticket CrearTicket()
    {
        return new Ticket(
            1,
            1,
            "Problema de red",
            "No hay conexión a Internet.",
            Prioridad.Media,
            "Administrador",
            Fecha);
    }

    private sealed class FakeSoporteRepository : ISoporteRepository
    {
        private readonly List<Cliente> clientes = [];
        private readonly List<Tecnico> tecnicos = [];
        private readonly List<Categoria> categorias = [];
        private readonly List<Ticket> tickets = [];

        public bool Guardado { get; private set; }

        public void AgregarCliente(Cliente cliente)
        {
            AsignarId(cliente, clientes.Count + 1);
            clientes.Add(cliente);
        }

        public void AgregarTecnico(Tecnico tecnico)
        {
            AsignarId(tecnico, tecnicos.Count + 1);
            tecnicos.Add(tecnico);
        }

        public void AgregarCategoria(Categoria categoria)
        {
            AsignarId(categoria, categorias.Count + 1);
            categorias.Add(categoria);
        }

        public Task<Cliente?> ObtenerClienteAsync(
            int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(clientes.FirstOrDefault(x => x.Id == id));

        public Task<Tecnico?> ObtenerTecnicoAsync(
            int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(tecnicos.FirstOrDefault(x => x.Id == id));

        public Task<Categoria?> ObtenerCategoriaAsync(
            int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(categorias.FirstOrDefault(x => x.Id == id));

        public Task<Ticket?> ObtenerTicketAsync(
            int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(tickets.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<Cliente>> ObtenerClientesAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Cliente>>(clientes);

        public Task<IReadOnlyList<Tecnico>> ObtenerTecnicosAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Tecnico>>(tecnicos);

        public Task<IReadOnlyList<Categoria>> ObtenerCategoriasAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Categoria>>(categorias);

        public Task<IReadOnlyList<Ticket>> ObtenerTicketsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Ticket>>(tickets);

        public void AgregarTicket(Ticket ticket)
        {
            if (ticket.Id == 0)
                AsignarId(ticket, tickets.Count + 1);
            tickets.Add(ticket);
        }

        public Task GuardarCambiosAsync(
            CancellationToken cancellationToken = default)
        {
            Guardado = true;
            return Task.CompletedTask;
        }

        private static void AsignarId<T>(T entidad, int id) where T : class =>
            typeof(T).GetProperty("Id")!.SetValue(entidad, id);
    }
}

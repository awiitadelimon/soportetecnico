using SoporteTecnico.Frontend.Models;

namespace SoporteTecnico.Frontend.Services;

public sealed record FrontendOptions(bool UseMockData);

public interface ITf15Service
{
    Sesion? SesionActual { get; }
    Task IniciarSesionAsync(LoginInput input);
    void CerrarSesion();
    Task<Resumen> ResumenAsync();
    Task<Categoria[]> CategoriasAsync();
    Task<Categoria> GuardarCategoriaAsync(int? id, CategoriaInput input);
    Task EliminarCategoriaAsync(int id);
    Task<Cliente[]> ClientesAsync();
    Task<Cliente> GuardarClienteAsync(int? id, ClienteInput input);
    Task EliminarClienteAsync(int id);
    Task<Tecnico[]> TecnicosAsync();
    Task<Tecnico> GuardarTecnicoAsync(int? id, TecnicoInput input);
    Task EliminarTecnicoAsync(int id);
    Task<TicketResumen[]> TicketsAsync();
    Task<Ticket> TicketAsync(int id);
    Task<Ticket> CrearTicketAsync(TicketInput input);
    Task<Ticket> AsignarAsync(int id, AsignacionInput input);
    Task<Ticket> AnotarAsync(int id, NotaInput input);
    Task<Ticket> ResolverAsync(int id, ResolucionInput input);
    Task<Ticket> CerrarAsync(int id);
}

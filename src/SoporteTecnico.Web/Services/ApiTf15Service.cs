using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SoporteTecnico.Frontend.Models;

namespace SoporteTecnico.Frontend.Services;

public sealed class ApiTf15Service(HttpClient client) : ITf15Service
{
    public Sesion? SesionActual { get; private set; }

    public async Task IniciarSesionAsync(LoginInput input)
    {
        var sesion = await Leer<Sesion>(HttpMethod.Post, "/api/auth/login", input, requiereSesion: false);
        if (string.IsNullOrWhiteSpace(sesion.AccessToken) || string.IsNullOrWhiteSpace(sesion.Nombre) ||
            sesion.Roles is not { Length: > 0 })
            throw new InvalidOperationException("La respuesta de inicio de sesión no coincide con el contrato.");
        SesionActual = sesion;
    }

    public void CerrarSesion() => SesionActual = null;
    public Task<Resumen> ResumenAsync() => Leer<Resumen>(HttpMethod.Get, "/api/dashboard");
    public Task<Categoria[]> CategoriasAsync() => Leer<Categoria[]>(HttpMethod.Get, "/api/categorias");
    public Task<Categoria> GuardarCategoriaAsync(int? id, CategoriaInput input) => id is int value
        ? Leer<Categoria>(HttpMethod.Put, $"/api/categorias/{value}", input)
        : Leer<Categoria>(HttpMethod.Post, "/api/categorias", input);

    public async Task EliminarCategoriaAsync(int id)
    {
        using var response = await Enviar(HttpMethod.Delete, $"/api/categorias/{id}");
    }

    public Task<Tecnico[]> TecnicosAsync() => Leer<Tecnico[]>(HttpMethod.Get, "/api/tecnicos");
    public Task<Tecnico> GuardarTecnicoAsync(int? id, TecnicoInput input) => id is int value
        ? Leer<Tecnico>(HttpMethod.Put, $"/api/tecnicos/{value}", input)
        : Leer<Tecnico>(HttpMethod.Post, "/api/tecnicos", input);

    public async Task EliminarTecnicoAsync(int id)
    {
        using var response = await Enviar(HttpMethod.Delete, $"/api/tecnicos/{id}");
    }

    public Task<Cliente[]> ClientesAsync() => Leer<Cliente[]>(HttpMethod.Get, "/api/clientes");
    public Task<Cliente> GuardarClienteAsync(int? id, ClienteInput input) => id is int value
        ? Leer<Cliente>(HttpMethod.Put, $"/api/clientes/{value}", input)
        : Leer<Cliente>(HttpMethod.Post, "/api/clientes", input);

    public async Task EliminarClienteAsync(int id)
    {
        using var response = await Enviar(HttpMethod.Delete, $"/api/clientes/{id}");
    }

    public Task<TicketResumen[]> TicketsAsync() => Leer<TicketResumen[]>(HttpMethod.Get, "/api/tickets");
    public Task<Ticket> TicketAsync(int id) => Leer<Ticket>(HttpMethod.Get, $"/api/tickets/{id}");
    public Task<Ticket> CrearTicketAsync(TicketInput input) => Leer<Ticket>(HttpMethod.Post, "/api/tickets", input);
    public Task<Ticket> AsignarAsync(int id, AsignacionInput input) =>
        Leer<Ticket>(HttpMethod.Post, $"/api/tickets/{id}/asignacion", input);
    public Task<Ticket> AnotarAsync(int id, NotaInput input) =>
        Leer<Ticket>(HttpMethod.Post, $"/api/tickets/{id}/seguimientos", input);
    public Task<Ticket> ResolverAsync(int id, ResolucionInput input) =>
        Leer<Ticket>(HttpMethod.Post, $"/api/tickets/{id}/resolucion", input);
    public Task<Ticket> CerrarAsync(int id) => Leer<Ticket>(HttpMethod.Post, $"/api/tickets/{id}/cierre");

    private async Task<T> Leer<T>(HttpMethod method, string path, object? body = null, bool requiereSesion = true)
    {
        using var response = await Enviar(method, path, body, requiereSesion);
        return await response.Content.ReadFromJsonAsync<T>()
            ?? throw new InvalidOperationException("La API devolvió una respuesta vacía.");
    }

    private async Task<HttpResponseMessage> Enviar(HttpMethod method, string path,
        object? body = null, bool requiereSesion = true)
    {
        using var request = new HttpRequestMessage(method, path);
        var sesion = requiereSesion
            ? SesionActual ?? throw new InvalidOperationException("Inicie sesión para usar la API.")
            : null;
        if (sesion is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sesion.AccessToken);
        if (body is not null)
            request.Content = JsonContent.Create(body);

        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
            return response;

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized && sesion is not null && ReferenceEquals(SesionActual, sesion))
                CerrarSesion();
            var mensaje = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized when requiereSesion => "Sesión expirada. Inicie sesión nuevamente.",
                HttpStatusCode.Unauthorized => "Credenciales incorrectas.",
                HttpStatusCode.Forbidden => "Su rol carece de permiso para esta operación.",
                _ => $"La API devolvió HTTP {(int)response.StatusCode}."
            };
            if (response.StatusCode is not HttpStatusCode.Unauthorized and not HttpStatusCode.Forbidden
                && (int)response.StatusCode < 500)
            {
                try
                {
                    var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (problem.ValueKind == JsonValueKind.Object &&
                        problem.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                        mensaje = detail.GetString() ?? mensaje;
                }
                catch (JsonException) { }
                catch (NotSupportedException) { }
            }
            throw new InvalidOperationException(mensaje);
        }
    }
}

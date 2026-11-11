using SoporteTecnico.Frontend.Services;
using Microsoft.AspNetCore.Components;

namespace SoporteTecnico.Frontend.Pages;

public abstract class Tf15Page : ComponentBase
{
    [Inject] protected ITf15Service Servicio { get; set; } = default!;
    [Inject] protected FrontendOptions Opciones { get; set; } = default!;
    [Inject] protected NavigationManager Navegacion { get; set; } = default!;
    protected bool Ocupado { get; private set; }
    protected string? Error { get; private set; }
    protected string? Confirmacion { get; private set; }
    protected bool Acceso => Servicio.SesionActual is not null;
    protected bool Admin => TieneRol("Administrador");
    protected bool Gestion => Admin || TieneRol("Gestor");

    private bool TieneRol(string rol) => Servicio.SesionActual?.Roles.Contains(rol, StringComparer.Ordinal) == true;

    protected async Task EjecutarAsync(Func<Task> accion)
    {
        if (Ocupado)
            return;
        var teniaSesion = Acceso;
        var paginaActual = Navegacion.Uri;
        Ocupado = true;
        Error = Confirmacion = null;
        try
        {
            await accion();
        }
        catch (HttpRequestException)
        {
            Error = "No se pudo conectar con la API. Revise la URL, el certificado local y CORS.";
        }
        catch (System.Text.Json.JsonException)
        {
            Error = "La respuesta de la API no coincide con el contrato JSON.";
        }
        catch (InvalidOperationException ex)
        {
            Error = ex.Message;
            if (teniaSesion && !Acceso && Navegacion.Uri == paginaActual)
                Navegacion.NavigateTo("login?expirada=true", replace: true);
        }
        finally
        {
            Ocupado = false;
        }
    }

    protected void Confirmar(string texto) => Confirmacion = texto;

}

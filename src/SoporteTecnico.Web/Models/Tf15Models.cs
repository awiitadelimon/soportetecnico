namespace SoporteTecnico.Frontend.Models;

public sealed record Categoria(int Id, string Nombre);
public sealed record Cliente(int Id, string Nombre, string Email);
public sealed record Tecnico(int Id, string Nombre, string Email, bool Activo);

public sealed record TicketResumen(int Id, int ClienteId, string ClienteNombre, int CategoriaId, string CategoriaNombre,
    int? TecnicoId, string? TecnicoNombre, string Asunto, string Prioridad, string Estado, DateTimeOffset CreadoUtc);
public sealed record Seguimiento(int Id, string Tipo, string Detalle, string Autor, DateTimeOffset FechaUtc, int? TecnicoId);
public sealed record Ticket(TicketResumen Datos, string Descripcion, DateTimeOffset? ResueltoUtc,
    DateTimeOffset? CerradoUtc, Seguimiento[] Seguimientos);
public sealed record Resumen(int Abiertos, int Asignados, int Resueltos, int Cerrados, int TecnicosActivos);

public sealed record CategoriaInput(string Nombre);
public sealed record ClienteInput(string Nombre, string Email);
public sealed record TecnicoInput(string Nombre, string Email, bool Activo);
public sealed record TicketInput(int ClienteId, int CategoriaId, string Asunto, string Descripcion, string Prioridad);
public sealed record AsignacionInput(int TecnicoId);
public sealed record NotaInput(string Detalle);
public sealed record ResolucionInput(string Solucion);
public sealed record LoginInput(string Email, string Contrasena);
public sealed record Sesion(string AccessToken, string Nombre, string[] Roles);

public static class Prioridades
{
    public static IReadOnlyList<string> Todas { get; } = Array.AsReadOnly(new[] { "Baja", "Media", "Alta", "Critica" });

    public static string Etiqueta(string prioridad) => prioridad == "Critica" ? "Crítica" : prioridad;
}

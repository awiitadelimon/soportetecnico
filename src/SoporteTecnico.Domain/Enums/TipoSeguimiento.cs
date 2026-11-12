namespace SoporteTecnico.Domain.Enums;

/// <summary>
/// Tipos de eventos registrados en el historial de un ticket.
/// </summary>
public enum TipoSeguimiento
{
    Creado,
    Asignado,
    Reasignado,
    Nota,
    Resuelto,
    Cerrado
}

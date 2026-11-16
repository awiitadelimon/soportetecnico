
namespace SoporteTecnico.Domain.Exceptions;

/// <summary>
/// Excepción para incumplimientos generales de las reglas del dominio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

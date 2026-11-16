
namespace SoporteTecnico.Domain.Exceptions;

/// <summary>
/// Excepción para operaciones no permitidas
/// según el estado actual de un ticket.
/// </summary>
public sealed class TicketStateException : DomainException
{
    public TicketStateException(string message) : base(message)
    {
    }
}

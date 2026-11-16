
using SoporteTecnico.Domain.Exceptions;

namespace SoporteTecnico.Domain.Entities;

public class Categoria
{
    public int Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    private Categoria()
    {
    }

    public Categoria(string nombre)
    {
        ActualizarNombre(nombre);
    }

    public void ActualizarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la categoría es obligatorio.");

        nombre = nombre.Trim();

        if (nombre.Length > 100)
            throw new DomainException(
                "El nombre de la categoría no puede superar los 100 caracteres.");

        Nombre = nombre;
    }
}

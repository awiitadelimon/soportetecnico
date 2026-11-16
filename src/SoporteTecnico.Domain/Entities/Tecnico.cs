
using System.ComponentModel.DataAnnotations;
using SoporteTecnico.Domain.Exceptions;

namespace SoporteTecnico.Domain.Entities;

public class Tecnico
{
    public int Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public bool Activo { get; private set; } = true;

    private Tecnico()
    {
    }

    public Tecnico(string nombre, string email)
    {
        ActualizarDatos(nombre, email);
        Activo = true;
    }

    public void ActualizarDatos(string nombre, string email)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El correo electrónico es obligatorio.");

        nombre = nombre.Trim();
        email = email.Trim();

        if (nombre.Length > 120)
            throw new DomainException(
                "El nombre no puede superar los 120 caracteres.");

        if (email.Length > 254)
            throw new DomainException(
                "El correo no puede superar los 254 caracteres.");

        if (!new EmailAddressAttribute().IsValid(email))
            throw new DomainException(
                "El correo electrónico no es válido.");

        Nombre = nombre;
        Email = email;
    }

    public void Activar()
    {
        Activo = true;
    }

    public void Desactivar()
    {
        Activo = false;
    }
}

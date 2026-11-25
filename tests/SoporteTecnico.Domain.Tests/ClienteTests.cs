
using SoporteTecnico.Domain.Entities;
using SoporteTecnico.Domain.Exceptions;

namespace SoporteTecnico.Domain.Tests;

public class ClienteTests
{
    [Fact]
    public void Constructor_DatosValidos_CreaCliente()
    {
        var cliente = new Cliente("María Reyes", "maria@gmail.com");

        Assert.Equal("María Reyes", cliente.Nombre);
        Assert.Equal("maria@gmail.com", cliente.Email);
    }

    [Fact]
    public void Constructor_NombreVacio_LanzaExcepcion()
    {
        Assert.Throws<DomainException>(
            () => new Cliente("", "maria@gmail.com"));
    }

    [Fact]
    public void Constructor_EmailVacio_LanzaExcepcion()
    {
        Assert.Throws<DomainException>(
            () => new Cliente("María Reyes", ""));
    }

    [Fact]
    public void Constructor_EmailInvalido_LanzaExcepcion()
    {
        Assert.Throws<DomainException>(
            () => new Cliente("María Reyes", "correo-invalido"));
    }

    [Fact]
    public void ActualizarDatos_DatosValidos_ActualizaCliente()
    {
        var cliente = new Cliente("María Reyes", "maria@gmail.com");

        cliente.ActualizarDatos("Luis Peña", "luis@gmail.com");

        Assert.Equal("Luis Peña", cliente.Nombre);
        Assert.Equal("luis@gmail.com", cliente.Email);
    }
}

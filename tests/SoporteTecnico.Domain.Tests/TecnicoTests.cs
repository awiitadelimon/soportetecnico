
using SoporteTecnico.Domain.Entities;
using SoporteTecnico.Domain.Exceptions;

namespace SoporteTecnico.Domain.Tests;

public class TecnicoTests
{
    [Fact]
    public void Constructor_DatosValidos_CreaTecnicoActivo()
    {
        var tecnico = new Tecnico("Ana Mora", "ana@gmail.com");

        Assert.Equal("Ana Mora", tecnico.Nombre);
        Assert.Equal("ana@gmail.com", tecnico.Email);
        Assert.True(tecnico.Activo);
    }

    [Fact]
    public void Constructor_NombreVacio_LanzaDomainException()
    {
        Assert.Throws<DomainException>(
            () => new Tecnico(" ", "ana@gmail.com"));
    }

    [Fact]
    public void Constructor_NombreDemasiadoLargo_LanzaDomainException()
    {
        string nombre = new string('A', 121);

        Assert.Throws<DomainException>(
            () => new Tecnico(nombre, "ana@gmail.com"));
    }

    [Fact]
    public void Constructor_CorreoInvalido_LanzaDomainException()
    {
        Assert.Throws<DomainException>(
            () => new Tecnico("Ana Mora", "correo-invalido"));
    }

    [Fact]
    public void ActualizarDatos_DatosValidos_ModificaDatos()
    {
        var tecnico = new Tecnico("Ana Mora", "ana@gmail.com");

        tecnico.ActualizarDatos("Elena Cruz", "elena@gmail.com");

        Assert.Equal("Elena Cruz", tecnico.Nombre);
        Assert.Equal("elena@gmail.com", tecnico.Email);
    }

    [Fact]
    public void Desactivar_TecnicoActivo_CambiaEstadoAInactivo()
    {
        var tecnico = new Tecnico("Ana Mora", "ana@gmail.com");

        tecnico.Desactivar();

        Assert.False(tecnico.Activo);
    }

    [Fact]
    public void Activar_TecnicoInactivo_CambiaEstadoAActivo()
    {
        var tecnico = new Tecnico("Ana Mora", "ana@gmail.com");
        tecnico.Desactivar();

        tecnico.Activar();

        Assert.True(tecnico.Activo);
    }
}

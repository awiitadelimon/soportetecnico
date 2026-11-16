
using SoporteTecnico.Domain.Entities;
using SoporteTecnico.Domain.Exceptions;

namespace SoporteTecnico.Domain.Tests;

public class CategoriaTests
{
    [Fact]
    public void Constructor_NombreValido_CreaCategoria()
    {
        var categoria = new Categoria("Conectividad");

        Assert.Equal("Conectividad", categoria.Nombre);
    }

    [Fact]
    public void Constructor_NombreConEspacios_EliminaEspaciosExternos()
    {
        var categoria = new Categoria("  Equipos  ");

        Assert.Equal("Equipos", categoria.Nombre);
    }

    [Fact]
    public void Constructor_NombreVacio_LanzaDomainException()
    {
        Assert.Throws<DomainException>(
            () => new Categoria(" "));
    }

    [Fact]
    public void Constructor_NombreDemasiadoLargo_LanzaDomainException()
    {
        string nombre = new string('A', 101);

        Assert.Throws<DomainException>(
            () => new Categoria(nombre));
    }

    [Fact]
    public void ActualizarNombre_NombreValido_ModificaNombre()
    {
        var categoria = new Categoria("Conectividad");

        categoria.ActualizarNombre("Aplicaciones");

        Assert.Equal("Aplicaciones", categoria.Nombre);
    }
}

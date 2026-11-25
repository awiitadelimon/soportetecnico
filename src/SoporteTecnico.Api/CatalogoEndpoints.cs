using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SoporteTecnico.Domain.Entities;
using SoporteTecnico.Domain.Enums;
using SoporteTecnico.Domain.Exceptions;
using SoporteTecnico.Infrastructure.Data;

namespace SoporteTecnico.Api;

public static class CatalogoEndpoints
{
    public static IEndpointRouteBuilder MapCatalogoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard", async (SoporteDbContext db, CancellationToken ct) =>
        {
            var abiertos = await db.Tickets.CountAsync(t => t.Estado == EstadoTicket.Abierto, ct);
            var asignados = await db.Tickets.CountAsync(t => t.Estado == EstadoTicket.Asignado, ct);
            var resueltos = await db.Tickets.CountAsync(t => t.Estado == EstadoTicket.Resuelto, ct);
            var cerrados = await db.Tickets.CountAsync(t => t.Estado == EstadoTicket.Cerrado, ct);
            var tecnicosActivos = await db.Tecnicos.CountAsync(t => t.Activo, ct);

            return Results.Ok(new { abiertos, asignados, resueltos, cerrados, tecnicosActivos });
        }).RequireAuthorization();

        var categorias = app.MapGroup("/api/categorias").RequireAuthorization();

        categorias.MapGet("/", async (SoporteDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Categorias.AsNoTracking()
                .OrderBy(c => c.Nombre)
                .Select(c => new { c.Id, c.Nombre })
                .ToListAsync(ct)));

        categorias.MapPost("/", [Authorize(Roles = "Administrador")] async (
            CategoriaRequest req, SoporteDbContext db, CancellationToken ct) =>
        {
            try
            {
                var nombre = (req.Nombre ?? string.Empty).Trim();
                if (await db.Categorias.AnyAsync(c => c.Nombre == nombre, ct))
                    return Results.Problem(detail: "Ya existe una categoría con ese nombre.", statusCode: 409);

                var categoria = new Categoria(req.Nombre!);
                db.Categorias.Add(categoria);
                await db.SaveChangesAsync(ct);
                return Results.Created($"/api/categorias/{categoria.Id}", new { categoria.Id, categoria.Nombre });
            }
            catch (DomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400);
            }
        });

        categorias.MapPut("/{id:int}", [Authorize(Roles = "Administrador")] async (
            int id, CategoriaRequest req, SoporteDbContext db, CancellationToken ct) =>
        {
            var categoria = await db.Categorias.FindAsync([id], ct);
            if (categoria is null)
                return Results.Problem(detail: "La categoría no existe.", statusCode: 404);

            try
            {
                var nombre = (req.Nombre ?? string.Empty).Trim();
                if (await db.Categorias.AnyAsync(c => c.Id != id && c.Nombre == nombre, ct))
                    return Results.Problem(detail: "Ya existe otra categoría con ese nombre.", statusCode: 409);

                categoria.ActualizarNombre(req.Nombre!);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { categoria.Id, categoria.Nombre });
            }
            catch (DomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400);
            }
        });

        categorias.MapDelete("/{id:int}", [Authorize(Roles = "Administrador")] async (
            int id, SoporteDbContext db, CancellationToken ct) =>
        {
            var categoria = await db.Categorias.FindAsync([id], ct);
            if (categoria is null)
                return Results.Problem(detail: "La categoría no existe.", statusCode: 404);

            if (await db.Tickets.AnyAsync(t => t.CategoriaId == id, ct))
                return Results.Problem(detail: "No se puede eliminar la categoría porque tiene tickets asociados.", statusCode: 409);

            db.Categorias.Remove(categoria);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
        var tecnicos = app.MapGroup("/api/tecnicos").RequireAuthorization();

        tecnicos.MapGet("/", async (SoporteDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Tecnicos.AsNoTracking()
                .OrderBy(t => t.Nombre)
                .Select(t => new { t.Id, t.Nombre, t.Email, t.Activo })
                .ToListAsync(ct)));

        tecnicos.MapPost("/", [Authorize(Roles = "Administrador")] async (
            TecnicoRequest req, SoporteDbContext db, CancellationToken ct) =>
        {
            try
            {
                var email = (req.Email ?? string.Empty).Trim();
                if (await db.Tecnicos.AnyAsync(t => t.Email == email, ct))
                    return Results.Problem(detail: "Ya existe un técnico registrado con ese correo.", statusCode: 409);

                var tecnico = new Tecnico(req.Nombre!, req.Email!);
                if (!req.Activo) tecnico.Desactivar();

                db.Tecnicos.Add(tecnico);
                await db.SaveChangesAsync(ct);
                return Results.Created($"/api/tecnicos/{tecnico.Id}", new { tecnico.Id, tecnico.Nombre, tecnico.Email, tecnico.Activo });
            }
            catch (DomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400);
            }
        });

        tecnicos.MapPut("/{id:int}", [Authorize(Roles = "Administrador")] async (
            int id, TecnicoRequest req, SoporteDbContext db, CancellationToken ct) =>
        {
            var tecnico = await db.Tecnicos.FindAsync([id], ct);
            if (tecnico is null)
                return Results.Problem(detail: "El técnico no existe.", statusCode: 404);

            try
            {
                var email = (req.Email ?? string.Empty).Trim();
                if (await db.Tecnicos.AnyAsync(t => t.Id != id && t.Email == email, ct))
                    return Results.Problem(detail: "Ya existe otro técnico con ese correo.", statusCode: 409);

                tecnico.ActualizarDatos(req.Nombre!, req.Email!);
                if (req.Activo) tecnico.Activar(); else tecnico.Desactivar();

                await db.SaveChangesAsync(ct);
                return Results.Ok(new { tecnico.Id, tecnico.Nombre, tecnico.Email, tecnico.Activo });
            }
            catch (DomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400);
            }
        });

        tecnicos.MapDelete("/{id:int}", [Authorize(Roles = "Administrador")] async (
            int id, SoporteDbContext db, CancellationToken ct) =>
        {
            var tecnico = await db.Tecnicos.FindAsync([id], ct);
            if (tecnico is null)
                return Results.Problem(detail: "El técnico no existe.", statusCode: 404);

            if (await db.Tickets.AnyAsync(t => t.TecnicoId == id, ct) ||
                await db.Seguimientos.AnyAsync(s => s.TecnicoId == id, ct))
                return Results.Problem(detail: "No se puede eliminar el técnico porque tiene tickets o seguimientos asociados.", statusCode: 409);

            db.Tecnicos.Remove(tecnico);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        var clientes = app.MapGroup("/api/clientes").RequireAuthorization();

        clientes.MapGet("/", async (SoporteDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Clientes.AsNoTracking()
                .OrderBy(c => c.Nombre)
                .Select(c => new { c.Id, c.Nombre, c.Email })
                .ToListAsync(ct)));

        clientes.MapPost("/", [Authorize(Roles = "Administrador")] async (
            ClienteRequest req, SoporteDbContext db, CancellationToken ct) =>
        {
            try
            {
                var email = (req.Email ?? string.Empty).Trim();
                if (await db.Clientes.AnyAsync(c => c.Email == email, ct))
                    return Results.Problem(detail: "Ya existe un cliente registrado con ese correo.", statusCode: 409);

                var cliente = new Cliente(req.Nombre!, req.Email!);
                db.Clientes.Add(cliente);
                await db.SaveChangesAsync(ct);
                return Results.Created($"/api/clientes/{cliente.Id}", new { cliente.Id, cliente.Nombre, cliente.Email });
            }
            catch (DomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400);
            }
        });

        clientes.MapPut("/{id:int}", [Authorize(Roles = "Administrador")] async (
            int id, ClienteRequest req, SoporteDbContext db, CancellationToken ct) =>
        {
            var cliente = await db.Clientes.FindAsync([id], ct);
            if (cliente is null)
                return Results.Problem(detail: "El cliente no existe.", statusCode: 404);

            try
            {
                var email = (req.Email ?? string.Empty).Trim();
                if (await db.Clientes.AnyAsync(c => c.Id != id && c.Email == email, ct))
                    return Results.Problem(detail: "Ya existe otro cliente con ese correo.", statusCode: 409);

                cliente.ActualizarDatos(req.Nombre!, req.Email!);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { cliente.Id, cliente.Nombre, cliente.Email });
            }
            catch (DomainException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 400);
            }
        });

        clientes.MapDelete("/{id:int}", [Authorize(Roles = "Administrador")] async (
            int id, SoporteDbContext db, CancellationToken ct) =>
        {
            var cliente = await db.Clientes.FindAsync([id], ct);
            if (cliente is null)
                return Results.Problem(detail: "El cliente no existe.", statusCode: 404);

            if (await db.Tickets.AnyAsync(t => t.ClienteId == id, ct))
                return Results.Problem(detail: "No se puede eliminar el cliente porque tiene tickets asociados.", statusCode: 409);

            db.Clientes.Remove(cliente);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        return app;
    }

    public sealed record CategoriaRequest(string? Nombre);
    public sealed record TecnicoRequest(string? Nombre, string? Email, bool Activo = true);
    public sealed record ClienteRequest(string? Nombre, string? Email);
}


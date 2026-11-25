using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SoporteTecnico.Application.DTOs;
using SoporteTecnico.Application.Services;
using SoporteTecnico.Domain.Exceptions;
using SoporteTecnico.Infrastructure.Data;

namespace SoporteTecnico.Api;

public static class TicketEndpoints
{
    public static IEndpointRouteBuilder MapTicketEndpoints(
        this IEndpointRouteBuilder app)
    {
        var tickets = app.MapGroup("/api/tickets")
            .RequireAuthorization();

        tickets.MapGet("/", (
            TicketService service,
            SoporteDbContext db,
            CancellationToken cancellationToken) =>
            EjecutarAsync(async () =>
            {
                var lista = await service.ObtenerTodosAsync(cancellationToken);
                return await MapearResumenesAsync(lista, db, cancellationToken);
            }));

        tickets.MapGet("/{id:int}", (
            int id,
            TicketService service,
            SoporteDbContext db,
            CancellationToken cancellationToken) =>
            EjecutarAsync(async () =>
            {
                var dto = await service.ObtenerAsync(id, cancellationToken);
                return await MapearDetalleAsync(dto, db, cancellationToken);
            }));

        tickets.MapPost("/", [Authorize(Roles = "Administrador,Gestor")] (
            CrearTicketRequest request,
            TicketService service,
            SoporteDbContext db,
            ClaimsPrincipal usuario,
            CancellationToken cancellationToken) =>
            EjecutarAsync(
                async () =>
                {
                    var dto = await service.CrearAsync(
                        request,
                        usuario.Identity!.Name!,
                        cancellationToken);
                    return await MapearDetalleAsync(dto, db, cancellationToken);
                },
                ticket => Results.Created(
                    $"/api/tickets/{ticket.Datos.Id}", ticket)));

        tickets.MapPost("/{id:int}/asignacion", [Authorize(Roles = "Administrador,Gestor")] (
            int id,
            AsignarTicketRequest request,
            TicketService service,
            SoporteDbContext db,
            ClaimsPrincipal usuario,
            CancellationToken cancellationToken) =>
            EjecutarAsync(async () =>
            {
                var dto = await service.AsignarAsync(
                    id, request, usuario.Identity!.Name!, cancellationToken);
                return await MapearDetalleAsync(dto, db, cancellationToken);
            }));

        tickets.MapPost("/{id:int}/seguimientos", [Authorize(Roles = "Administrador,Gestor")] (
            int id,
            AgregarNotaRequest request,
            TicketService service,
            SoporteDbContext db,
            ClaimsPrincipal usuario,
            CancellationToken cancellationToken) =>
            EjecutarAsync(async () =>
            {
                var dto = await service.AgregarNotaAsync(
                    id, request, usuario.Identity!.Name!, cancellationToken);
                return await MapearDetalleAsync(dto, db, cancellationToken);
            }));

        tickets.MapPost("/{id:int}/resolucion", [Authorize(Roles = "Administrador,Gestor")] (
            int id,
            ResolverTicketRequest request,
            TicketService service,
            SoporteDbContext db,
            ClaimsPrincipal usuario,
            CancellationToken cancellationToken) =>
            EjecutarAsync(async () =>
            {
                var dto = await service.ResolverAsync(
                    id, request, usuario.Identity!.Name!, cancellationToken);
                return await MapearDetalleAsync(dto, db, cancellationToken);
            }));

        tickets.MapPost("/{id:int}/cierre", [Authorize(Roles = "Administrador,Gestor")] (
            int id,
            TicketService service,
            SoporteDbContext db,
            ClaimsPrincipal usuario,
            CancellationToken cancellationToken) =>
            EjecutarAsync(async () =>
            {
                var dto = await service.CerrarAsync(
                    id, usuario.Identity!.Name!, cancellationToken);
                return await MapearDetalleAsync(dto, db, cancellationToken);
            }));

        return app;
    }

    private static async Task<IReadOnlyList<TicketResumenResponse>> MapearResumenesAsync(
        IReadOnlyList<TicketDto> dtos,
        SoporteDbContext db,
        CancellationToken ct)
    {
        var clientes = await db.Clientes.AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => c.Nombre, ct);
        var categorias = await db.Categorias.AsNoTracking()
            .ToDictionaryAsync(c => c.Id, c => c.Nombre, ct);
        var tecnicos = await db.Tecnicos.AsNoTracking()
            .ToDictionaryAsync(t => t.Id, t => t.Nombre, ct);

        return dtos.Select(dto => new TicketResumenResponse(
            dto.Id,
            dto.ClienteId,
            clientes.GetValueOrDefault(dto.ClienteId, $"Cliente #{dto.ClienteId}"),
            dto.CategoriaId,
            categorias.GetValueOrDefault(dto.CategoriaId, $"Categoría #{dto.CategoriaId}"),
            dto.TecnicoId,
            dto.TecnicoId is int tid ? tecnicos.GetValueOrDefault(tid, $"Técnico #{tid}") : null,
            dto.Asunto,
            dto.Prioridad.ToString(),
            dto.Estado.ToString(),
            dto.CreadoUtc)).ToList();
    }

    private static async Task<TicketDetalleResponse> MapearDetalleAsync(
        TicketDto dto,
        SoporteDbContext db,
        CancellationToken ct)
    {
        var clienteNombre = await db.Clientes.AsNoTracking()
            .Where(c => c.Id == dto.ClienteId)
            .Select(c => c.Nombre)
            .FirstOrDefaultAsync(ct) ?? $"Cliente #{dto.ClienteId}";

        var categoriaNombre = await db.Categorias.AsNoTracking()
            .Where(c => c.Id == dto.CategoriaId)
            .Select(c => c.Nombre)
            .FirstOrDefaultAsync(ct) ?? $"Categoría #{dto.CategoriaId}";

        string? tecnicoNombre = null;
        if (dto.TecnicoId is int tid)
        {
            tecnicoNombre = await db.Tecnicos.AsNoTracking()
                .Where(t => t.Id == tid)
                .Select(t => t.Nombre)
                .FirstOrDefaultAsync(ct) ?? $"Técnico #{tid}";
        }

        var datos = new TicketResumenResponse(
            dto.Id,
            dto.ClienteId,
            clienteNombre,
            dto.CategoriaId,
            categoriaNombre,
            dto.TecnicoId,
            tecnicoNombre,
            dto.Asunto,
            dto.Prioridad.ToString(),
            dto.Estado.ToString(),
            dto.CreadoUtc);

        var seguimientos = dto.Seguimientos
            .Select(s => new SeguimientoResponse(
                s.Id,
                s.Tipo.ToString(),
                s.Detalle,
                s.Autor,
                s.FechaUtc,
                s.TecnicoId))
            .ToList();

        return new TicketDetalleResponse(
            datos,
            dto.Descripcion,
            dto.ResueltoUtc,
            dto.CerradoUtc,
            seguimientos);
    }

    private static async Task<IResult> EjecutarAsync<T>(
        Func<Task<T>> accion,
        Func<T, IResult>? respuesta = null)
    {
        try
        {
            var resultado = await accion();

            return respuesta is null
                ? Results.Ok(resultado)
                : respuesta(resultado);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status404NotFound);
        }
        catch (TicketStateException ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
        catch (DomainException ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private sealed record TicketResumenResponse(
        int Id,
        int ClienteId,
        string ClienteNombre,
        int CategoriaId,
        string CategoriaNombre,
        int? TecnicoId,
        string? TecnicoNombre,
        string Asunto,
        string Prioridad,
        string Estado,
        DateTimeOffset CreadoUtc);

    private sealed record SeguimientoResponse(
        int Id,
        string Tipo,
        string Detalle,
        string Autor,
        DateTimeOffset FechaUtc,
        int? TecnicoId);

    private sealed record TicketDetalleResponse(
        TicketResumenResponse Datos,
        string Descripcion,
        DateTimeOffset? ResueltoUtc,
        DateTimeOffset? CerradoUtc,
        IReadOnlyList<SeguimientoResponse> Seguimientos);
}

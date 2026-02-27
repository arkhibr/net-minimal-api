using ProdutosAPI.Features.Common;

namespace ProdutosAPI.Features.Pedidos.ListPedidos;

public class ListPedidosEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/pedidos", async (
            ListPedidosHandler handler,
            CancellationToken ct,
            int page = 1,
            int pageSize = 20,
            string? status = null) =>
        {
            var result = await handler.HandleAsync(
                new ListPedidosQuery(page, pageSize, status), ct);
            return Results.Ok(result.Value);
        })
        .WithName("ListarPedidos")
        .WithTags("Pedidos")
        .WithSummary("Listar pedidos com paginação")
        .Produces<ListPedidosResponse>(StatusCodes.Status200OK)
        .RequireAuthorization();
    }
}

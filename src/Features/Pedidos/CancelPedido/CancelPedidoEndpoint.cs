using ProdutosAPI.Features.Common;
using ProdutosAPI.Features.Pedidos.Common;

namespace ProdutosAPI.Features.Pedidos.CancelPedido;

public class CancelPedidoEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/pedidos/{id:int}/cancelar", async (
            int id,
            CancelPedidoRequest request,
            CancelPedidoHandler handler,
            CancellationToken ct) =>
        {
            var cmd = new CancelPedidoCommand(id, request.Motivo);
            var result = await handler.HandleAsync(cmd, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("CancelarPedido")
        .WithTags("Pedidos")
        .WithSummary("Cancelar pedido")
        .Produces<PedidoResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .RequireAuthorization();
    }
}

public record CancelPedidoRequest(string Motivo);

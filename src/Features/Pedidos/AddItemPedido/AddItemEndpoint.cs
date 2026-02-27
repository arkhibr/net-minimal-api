using FluentValidation;
using ProdutosAPI.Features.Common;
using ProdutosAPI.Features.Pedidos.Common;

namespace ProdutosAPI.Features.Pedidos.AddItemPedido;

public class AddItemEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/pedidos/{id:int}/itens", async (
            int id,
            AddItemRequest request,
            AddItemHandler handler,
            IValidator<AddItemRequest> validator,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var cmd = new AddItemCommand(id, request.ProdutoId, request.Quantidade);
            var result = await handler.HandleAsync(cmd, ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .WithName("AdicionarItemPedido")
        .WithTags("Pedidos")
        .WithSummary("Adicionar item a pedido em rascunho")
        .Produces<PedidoResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .RequireAuthorization();
    }
}

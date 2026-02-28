using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Shared.Data;
using ProdutosAPI.Shared.Common;
using ProdutosAPI.Pedidos.Common;

namespace ProdutosAPI.Pedidos.GetPedido;

public record GetPedidoQuery(int Id);

public class GetPedidoHandler(AppDbContext db)
{
    public async Task<Result<PedidoResponse>> HandleAsync(
        GetPedidoQuery query, CancellationToken ct = default)
    {
        var pedido = await db.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == query.Id, ct);

        if (pedido is null)
            return Result<PedidoResponse>.Fail("Pedido não encontrado.");

        return Result<PedidoResponse>.Ok(PedidoResponse.From(pedido));
    }
}

using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Shared.Data;
using ProdutosAPI.Shared.Common;
using ProdutosAPI.Pedidos.Common;

namespace ProdutosAPI.Pedidos.AddItemPedido;

public record AddItemCommand(int PedidoId, int ProdutoId, int Quantidade);

public class AddItemHandler(AppDbContext db)
{
    public async Task<Result<PedidoResponse>> HandleAsync(
        AddItemCommand cmd, CancellationToken ct = default)
    {
        var pedido = await db.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == cmd.PedidoId, ct);

        if (pedido is null)
            return Result<PedidoResponse>.Fail("Pedido não encontrado.");

        var produto = await db.Produtos.FindAsync([cmd.ProdutoId], ct);
        if (produto is null)
            return Result<PedidoResponse>.Fail($"Produto {cmd.ProdutoId} não encontrado.");

        var resultado = pedido.AdicionarItem(produto, cmd.Quantidade);
        if (!resultado.IsSuccess)
            return Result<PedidoResponse>.Fail(resultado.Error!);

        await db.SaveChangesAsync(ct);
        return Result<PedidoResponse>.Ok(PedidoResponse.From(pedido));
    }
}

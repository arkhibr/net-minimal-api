using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Data;
using ProdutosAPI.Features.Common;
using ProdutosAPI.Features.Pedidos.Common;

namespace ProdutosAPI.Features.Pedidos.CancelPedido;

public record CancelPedidoRequest(string Motivo);

public record CancelPedidoCommand(int PedidoId, string Motivo);

public class CancelPedidoHandler(AppDbContext db)
{
    public async Task<Result<PedidoResponse>> HandleAsync(
        CancelPedidoCommand cmd, CancellationToken ct = default)
    {
        var pedido = await db.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == cmd.PedidoId, ct);

        if (pedido is null)
            return Result<PedidoResponse>.Fail("Pedido não encontrado.");

        var resultado = pedido.Cancelar(cmd.Motivo);
        if (!resultado.IsSuccess)
            return Result<PedidoResponse>.Fail(resultado.Error!);

        await db.SaveChangesAsync(ct);
        return Result<PedidoResponse>.Ok(PedidoResponse.From(pedido));
    }
}

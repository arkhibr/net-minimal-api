using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Data;
using ProdutosAPI.Features.Common;
using ProdutosAPI.Features.Pedidos.Common;
using ProdutosAPI.Features.Pedidos.Domain;

namespace ProdutosAPI.Features.Pedidos.CreatePedido;

public record CreatePedidoCommand(List<CreatePedidoItemDto> Itens);
public record CreatePedidoItemDto(int ProdutoId, int Quantidade);

public class CreatePedidoHandler(AppDbContext db)
{
    public async Task<Result<PedidoResponse>> HandleAsync(
        CreatePedidoCommand cmd, CancellationToken ct = default)
    {
        var pedido = Pedido.Criar();

        foreach (var itemDto in cmd.Itens)
        {
            var produto = await db.Produtos.FindAsync([itemDto.ProdutoId], ct);
            if (produto is null)
                return Result<PedidoResponse>.Fail($"Produto {itemDto.ProdutoId} não encontrado.");

            var resultado = pedido.AdicionarItem(produto, itemDto.Quantidade);
            if (!resultado.IsSuccess)
                return Result<PedidoResponse>.Fail(resultado.Error!);
        }

        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync(ct);

        return Result<PedidoResponse>.Ok(PedidoResponse.From(pedido));
    }
}

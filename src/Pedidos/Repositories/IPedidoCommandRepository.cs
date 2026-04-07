using ProdutosAPI.Pedidos.Domain;
using ProdutosAPI.Produtos.Domain;

namespace ProdutosAPI.Pedidos.Repositories;

public interface IPedidoCommandRepository
{
    /// <summary>Carrega pedido com itens rastreado pelo EF para posterior mutação.</summary>
    Task<Pedido?> ObterPorIdAsync(int id, CancellationToken ct = default);
    /// <summary>Carrega produto rastreado pelo EF — necessário para pedido.AdicionarItem().</summary>
    Task<Produto?> ObterProdutoParaItemAsync(int produtoId, CancellationToken ct = default);
    Task AdicionarAsync(Pedido pedido, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Pedidos.Domain;
using ProdutosAPI.Pedidos.Repositories;
using ProdutosAPI.Catalogo.Domain;
using ProdutosAPI.Shared.Data;

namespace ProdutosAPI.Pedidos.Infrastructure;

public class PedidoCommandRepository(AppDbContext db) : IPedidoCommandRepository
{
    public Task<Pedido?> ObterPorIdAsync(int id, CancellationToken ct = default) =>
        db.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Produto?> ObterProdutoParaItemAsync(int produtoId, CancellationToken ct = default) =>
        db.Produtos.FindAsync([produtoId], ct).AsTask();

    public Task AdicionarAsync(Pedido pedido, CancellationToken ct = default)
    {
        db.Pedidos.Add(pedido);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}

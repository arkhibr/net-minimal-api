using ProdutosAPI.Produtos.Domain;

namespace ProdutosAPI.Produtos.Application.Repositories;

public interface IProdutoRepository
{
    Task<(IReadOnlyList<Produto> Items, int Total)> ListarAsync(
        int page, int pageSize, string? categoria = null, string? search = null);
    Task<Produto?> ObterPorIdAsync(int id);
    Task<Produto> AdicionarAsync(Produto produto);
    Task AtualizarAsync(Produto produto);
    Task<bool> DeletarAsync(int id);
}

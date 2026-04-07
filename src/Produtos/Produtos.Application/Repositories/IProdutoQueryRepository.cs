using ProdutosAPI.Produtos.Application.DTOs;

namespace ProdutosAPI.Produtos.Application.Repositories;

public interface IProdutoQueryRepository
{
    Task<ProdutoResponse?> ObterPorIdAsync(int id);
    Task<(IReadOnlyList<ProdutoResponse> Items, int Total)> ListarAsync(
        int page, int pageSize, string? categoria = null, string? search = null);
}

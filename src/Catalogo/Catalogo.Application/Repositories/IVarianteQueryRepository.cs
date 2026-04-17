using ProdutosAPI.Catalogo.Application.DTOs.Variante;

namespace ProdutosAPI.Catalogo.Application.Repositories;

public interface IVarianteQueryRepository
{
    Task<List<VarianteResponse>> ListarPorProdutoAsync(int produtoId);
    Task<VarianteResponse?> ObterPorIdAsync(int id);
}

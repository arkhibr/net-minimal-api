using ProdutosAPI.Catalogo.Application.DTOs.Atributo;
using ProdutosAPI.Catalogo.Domain;

namespace ProdutosAPI.Catalogo.Application.Repositories;

public interface IAtributoRepository
{
    Task<List<AtributoResponse>> ListarPorProdutoAsync(int produtoId);
    Task<Atributo?> ObterPorIdAsync(int id);
    Task<Atributo> AdicionarAsync(Atributo atributo);
    Task RemoverAsync(int id);
    Task SaveChangesAsync();
}

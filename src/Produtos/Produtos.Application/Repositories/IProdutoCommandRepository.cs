using ProdutosAPI.Produtos.Domain;

namespace ProdutosAPI.Produtos.Application.Repositories;

public interface IProdutoCommandRepository
{
    /// <summary>Carrega entidade rastreada pelo EF para posterior mutação.</summary>
    Task<Produto?> ObterPorIdAsync(int id);
    Task<Produto> AdicionarAsync(Produto produto);
    /// <summary>Soft delete: marca Ativo = false.</summary>
    Task<bool> DeletarAsync(int id);
    Task SaveChangesAsync();
}

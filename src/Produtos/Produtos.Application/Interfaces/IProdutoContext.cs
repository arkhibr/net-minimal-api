using ProdutosAPI.Produtos.Domain;

namespace ProdutosAPI.Produtos.Application.Interfaces;

/// <summary>
/// Abstração do contexto de banco para o feature de Produtos.
/// Implementada por AppDbContext no projeto principal via DI.
/// Usando IQueryable<T> (System.Linq) para evitar dependência direta do EF Core em Application.
/// </summary>
public interface IProdutoContext
{
    IQueryable<Produto> Produtos { get; }
    void AddProduto(Produto produto);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

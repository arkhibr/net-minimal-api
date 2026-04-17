using ProdutosAPI.Catalogo.Domain;

namespace ProdutosAPI.Catalogo.Application.Interfaces;

public interface ICatalogoContext
{
    IQueryable<Produto> Produtos { get; }
    void AddProduto(Produto produto);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

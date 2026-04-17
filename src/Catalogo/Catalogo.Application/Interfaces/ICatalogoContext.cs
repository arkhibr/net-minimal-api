using ProdutosAPI.Catalogo.Domain;

namespace ProdutosAPI.Catalogo.Application.Interfaces;

public interface ICatalogoContext
{
    IQueryable<Produto> Produtos { get; }
    IQueryable<Categoria> Categorias { get; }
    void AddProduto(Produto produto);
    void AddCategoria(Categoria categoria);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

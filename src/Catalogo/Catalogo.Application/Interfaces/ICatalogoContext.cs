using ProdutosAPI.Catalogo.Domain;

namespace ProdutosAPI.Catalogo.Application.Interfaces;

public interface ICatalogoContext
{
    IQueryable<Produto> Produtos { get; }
    IQueryable<Categoria> Categorias { get; }
    IQueryable<Variante> Variantes { get; }
    void AddProduto(Produto produto);
    void AddCategoria(Categoria categoria);
    void AddVariante(Variante variante);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

using ProdutosAPI.Catalogo.Application.DTOs.Categoria;

namespace ProdutosAPI.Catalogo.Application.Repositories;

public interface ICategoriaQueryRepository
{
    Task<List<CategoriaResponse>> ListarRaizComSubcategoriasAsync();
    Task<CategoriaResponse?> ObterPorIdAsync(int id);
}

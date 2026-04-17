using ProdutosAPI.Catalogo.Application.DTOs.Midia;
using ProdutosAPI.Catalogo.Domain.Common;

namespace ProdutosAPI.Catalogo.Application.Services;

public interface IMidiaService
{
    Task<List<MidiaResponse>> ListarPorProdutoAsync(int produtoId);
    Task<Result<MidiaResponse>> CriarAsync(CriarMidiaRequest request);
    Task<Result<MidiaResponse>> AtualizarOrdemAsync(int id, AtualizarOrdemMidiaRequest request);
    Task<Result> RemoverAsync(int id);
}

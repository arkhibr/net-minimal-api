using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProdutosAPI.Produtos.Application.Interfaces;
using ProdutosAPI.Produtos.Application.Repositories;
using ProdutosAPI.Produtos.Domain;

namespace ProdutosAPI.Produtos.Infrastructure.Repositories;

public class EfProdutoRepository : IProdutoRepository
{
    private readonly IProdutoContext _context;
    private readonly ILogger<EfProdutoRepository> _logger;

    public EfProdutoRepository(IProdutoContext context, ILogger<EfProdutoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(IReadOnlyList<Produto> Items, int Total)> ListarAsync(
        int page, int pageSize, string? categoria = null, string? search = null)
    {
        var query = _context.Produtos.Where(p => p.Ativo);

        if (!string.IsNullOrEmpty(categoria))
            query = query.Where(p => p.Categoria == categoria);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Nome.Contains(search) || p.Descricao.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.DataCriacao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Produto?> ObterPorIdAsync(int id)
    {
        return await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
    }

    public async Task<Produto> AdicionarAsync(Produto produto)
    {
        _context.AddProduto(produto);
        await _context.SaveChangesAsync();
        return produto;
    }

    public async Task AtualizarAsync(Produto produto)
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeletarAsync(int id)
    {
        var produto = await _context.Produtos
            .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);

        if (produto is null)
        {
            _logger.LogWarning("Produto {Id} não encontrado", id);
            return false;
        }

        var result = produto.Desativar();
        if (!result.IsSuccess) return false;

        await _context.SaveChangesAsync();
        return true;
    }
}

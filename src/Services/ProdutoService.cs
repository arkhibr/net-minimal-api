using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Data;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;

namespace ProdutosAPI.Services;

/// <summary>
/// Interface do serviço de produtos
/// Contém a assinatura dos métodos para operações com produtos
/// </summary>
public interface IProdutoService
{
    Task<PaginatedResponse<ProdutoResponse>> ListarProdutosAsync(int page, int pageSize, string? categoria = null, string? search = null);
    Task<ProdutoResponse?> ObterProdutoAsync(int id);
    Task<ProdutoResponse> CriarProdutoAsync(CriarProdutoRequest request);
    Task<ProdutoResponse?> AtualizarProdutoAsync(int id, AtualizarProdutoRequest request);
    Task<ProdutoResponse?> AtualizarCompletoProdutoAsync(int id, CriarProdutoRequest request);
    Task<bool> DeletarProdutoAsync(int id);
}

/// <summary>
/// Serviço de produtos - implementação
/// Referência: Melhores-Praticas-API.md - Seção "Validação de Dados e Lógica de Negócio"
/// Contém toda a lógica de negócio relacionada a produtos
/// </summary>
public class ProdutoService : IProdutoService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ProdutoService> _logger;

    public ProdutoService(AppDbContext context, IMapper mapper, ILogger<ProdutoService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Lista produtos com suporte a paginação, filtros e busca
    /// Referência: Melhores-Praticas-API.md - Seção "Paginação"
    /// </summary>
    public async Task<PaginatedResponse<ProdutoResponse>> ListarProdutosAsync(
        int page,
        int pageSize,
        string? categoria = null,
        string? search = null)
    {
        try
        {
            _logger.LogInformation("Listando produtos - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            // Validar paginação
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20; // Limite máximo de 100 itens

            // Query base - apenas produtos ativos
            // Referência: Melhores-Praticas-API.md - Seção "RESTful Design"
            var query = _context.Produtos
                .Where(p => p.Ativo)
                .AsQueryable();

            // Aplicar filtro de categoria
            // Referência: Melhores-Praticas-API.md - Seção "Filtros e Busca"
            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(p => p.Categoria == categoria);
                _logger.LogInformation("Filtro de categoria aplicado: {Categoria}", categoria);
            }

            // Aplicar filtro de busca
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Nome.Contains(search) || p.Descricao.Contains(search));
                _logger.LogInformation("Filtro de busca aplicado: {Search}", search);
            }

            // Contar total de itens
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Aplicar paginação e ordenação
            var produtos = await query
                .OrderByDescending(p => p.DataCriacao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PaginatedResponse<ProdutoResponse>
            {
                Data = _mapper.Map<List<ProdutoResponse>>(produtos),
                Pagination = new PaginationInfo
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };

            _logger.LogInformation("Produtos listados com sucesso. Total: {Total}", totalItems);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar produtos");
            throw;
        }
    }

    /// <summary>
    /// Obtém um produto específico pelo ID
    /// </summary>
    public async Task<ProdutoResponse?> ObterProdutoAsync(int id)
    {
        try
        {
            _logger.LogInformation("Obtendo produto com ID: {ProductId}", id);

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);

            if (produto == null)
            {
                _logger.LogWarning("Produto com ID {ProductId} não encontrado", id);
                return null;
            }

            return _mapper.Map<ProdutoResponse>(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter produto com ID {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Cria um novo produto
    /// POST /api/v1/produtos
    /// Referência: Melhores-Praticas-API.md - Seção "HTTP Status Codes"
    /// Retorna 201 Created com a localização do recurso
    /// </summary>
    public async Task<ProdutoResponse> CriarProdutoAsync(CriarProdutoRequest request)
    {
        try
        {
            _logger.LogInformation("Criando novo produto: {ProdutoNome}", request.Nome);

            var resultado = Produto.Criar(
                request.Nome, request.Descricao, request.Preco,
                request.Categoria, request.Estoque, request.ContatoEmail);
            if (!resultado.IsSuccess)
                throw new InvalidOperationException(resultado.Error);
            var produto = resultado.Value!;

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Produto criado com sucesso. ID: {ProductId}", produto.Id);
            return _mapper.Map<ProdutoResponse>(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto");
            throw;
        }
    }

    /// <summary>
    /// Atualiza parcialmente um produto (PATCH)
    /// Referência: Melhores-Praticas-API.md - Seção "Design de Endpoints - PATCH"
    /// Apenas os campos fornecidos são atualizados
    /// </summary>
    public async Task<ProdutoResponse?> AtualizarProdutoAsync(int id, AtualizarProdutoRequest request)
    {
        try
        {
            _logger.LogInformation("Atualizando produto com ID: {ProductId}", id);

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);

            if (produto == null)
            {
                _logger.LogWarning("Produto com ID {ProductId} não encontrado", id);
                return null;
            }

            if (request.Preco.HasValue)
            {
                var r = produto.AtualizarPreco(request.Preco.Value);
                if (!r.IsSuccess) throw new InvalidOperationException(r.Error);
            }
            if (request.Estoque.HasValue)
            {
                produto.AjustarEstoque(request.Estoque.Value);
            }
            produto.AtualizarDados(request.Nome, request.Descricao, request.Categoria, request.ContatoEmail);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Produto com ID {ProductId} atualizado com sucesso", id);
            return _mapper.Map<ProdutoResponse>(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar produto com ID {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Atualiza completamente um produto (PUT)
    /// Referência: Melhores-Praticas-API.md - Seção "Design de Endpoints - PUT"
    /// Substitui todos os campos do produto
    /// </summary>
    public async Task<ProdutoResponse?> AtualizarCompletoProdutoAsync(int id, CriarProdutoRequest request)
    {
        try
        {
            _logger.LogInformation("Atualizando completamente produto com ID: {ProductId}", id);

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);

            if (produto == null)
            {
                _logger.LogWarning("Produto com ID {ProductId} não encontrado", id);
                return null;
            }

            if (request.Preco != produto.Preco)
            {
                var rPreco = produto.AtualizarPreco(request.Preco);
                if (!rPreco.IsSuccess) throw new InvalidOperationException(rPreco.Error);
            }
            produto.AjustarEstoque(request.Estoque);
            produto.AtualizarDados(request.Nome, request.Descricao, request.Categoria, request.ContatoEmail);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Produto com ID {ProductId} atualizado completamente", id);
            return _mapper.Map<ProdutoResponse>(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar completamente produto com ID {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deleta um produto (soft delete)
    /// Referência: Melhores-Praticas-API.md - Seção "RESTful Design"
    /// Mantém a integridade referencial usando soft delete
    /// </summary>
    public async Task<bool> DeletarProdutoAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deletando produto com ID: {ProductId}", id);

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);

            if (produto == null)
            {
                _logger.LogWarning("Produto com ID {ProductId} não encontrado", id);
                return false;
            }

            // Soft delete - apenas marca como inativo
            var result = produto.Desativar();
            if (!result.IsSuccess) return false; // already inactive

            await _context.SaveChangesAsync();

            _logger.LogInformation("Produto com ID {ProductId} deletado com sucesso", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar produto com ID {ProductId}", id);
            throw;
        }
    }
}

using AutoMapper;
using Microsoft.Extensions.Logging;
using ProdutosAPI.Produtos.Application.DTOs;
using ProdutosAPI.Produtos.Application.Repositories;
using ProdutosAPI.Produtos.Domain;

namespace ProdutosAPI.Produtos.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProdutoService> _logger;

    public ProdutoService(IProdutoRepository repository, IMapper mapper, ILogger<ProdutoService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ProdutoResponse>> ListarProdutosAsync(
        int page, int pageSize, string? categoria = null, string? search = null)
    {
        _logger.LogInformation("Listando produtos - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var (produtos, total) = await _repository.ListarAsync(page, pageSize, categoria, search);
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new PaginatedResponse<ProdutoResponse>
        {
            Data = _mapper.Map<List<ProdutoResponse>>(produtos),
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = totalPages
            }
        };
    }

    public async Task<ProdutoResponse?> ObterProdutoAsync(int id)
    {
        _logger.LogInformation("Obtendo produto com ID: {ProductId}", id);
        var produto = await _repository.ObterPorIdAsync(id);
        if (produto is null)
        {
            _logger.LogWarning("Produto com ID {ProductId} não encontrado", id);
            return null;
        }
        return _mapper.Map<ProdutoResponse>(produto);
    }

    public async Task<ProdutoResponse> CriarProdutoAsync(CriarProdutoRequest request)
    {
        _logger.LogInformation("Criando novo produto: {Nome}", request.Nome);

        var resultado = Produto.Criar(
            request.Nome, request.Descricao, request.Preco,
            request.Categoria, request.Estoque, request.ContatoEmail);

        if (!resultado.IsSuccess)
            throw new InvalidOperationException(resultado.Error);

        var produto = await _repository.AdicionarAsync(resultado.Value!);
        _logger.LogInformation("Produto criado com sucesso. ID: {ProductId}", produto.Id);
        return _mapper.Map<ProdutoResponse>(produto);
    }

    public async Task<ProdutoResponse?> AtualizarProdutoAsync(int id, AtualizarProdutoRequest request)
    {
        _logger.LogInformation("Atualizando produto com ID: {ProductId}", id);

        var produto = await _repository.ObterPorIdAsync(id);
        if (produto is null)
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
            produto.AjustarEstoque(request.Estoque.Value);

        produto.AtualizarDados(request.Nome, request.Descricao, request.Categoria, request.ContatoEmail);
        await _repository.AtualizarAsync(produto);

        _logger.LogInformation("Produto {ProductId} atualizado com sucesso", id);
        return _mapper.Map<ProdutoResponse>(produto);
    }

    public async Task<ProdutoResponse?> AtualizarCompletoProdutoAsync(int id, CriarProdutoRequest request)
    {
        _logger.LogInformation("Atualizando completamente produto com ID: {ProductId}", id);

        var produto = await _repository.ObterPorIdAsync(id);
        if (produto is null)
        {
            _logger.LogWarning("Produto com ID {ProductId} não encontrado", id);
            return null;
        }

        if (request.Preco != produto.Preco)
        {
            var r = produto.AtualizarPreco(request.Preco);
            if (!r.IsSuccess) throw new InvalidOperationException(r.Error);
        }
        produto.AjustarEstoque(request.Estoque);
        produto.AtualizarDados(request.Nome, request.Descricao, request.Categoria, request.ContatoEmail);
        await _repository.AtualizarAsync(produto);

        _logger.LogInformation("Produto {ProductId} atualizado completamente", id);
        return _mapper.Map<ProdutoResponse>(produto);
    }

    public async Task<bool> DeletarProdutoAsync(int id)
    {
        _logger.LogInformation("Deletando produto com ID: {ProductId}", id);
        var deletado = await _repository.DeletarAsync(id);
        if (!deletado)
            _logger.LogWarning("Produto {ProductId} não encontrado para deleção", id);
        return deletado;
    }
}

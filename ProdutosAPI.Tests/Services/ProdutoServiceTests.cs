using Xunit;
using Moq;
using FluentAssertions;
using ProdutosAPI.Models;
using ProdutosAPI.DTOs;
using ProdutosAPI.Services;
using ProdutosAPI.Data;
using ProdutosAPI.Validators;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;

namespace ProdutosAPI.Tests.Services
{
    /// <summary>
    /// Unit tests for ProdutoService
    /// Tests all business logic methods using EF Core InMemory database
    /// References: REST API Best Practices - Error Handling & Validation
    /// </summary>
    public class ProdutoServiceTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ProdutoService>> _mockLogger;
        private readonly CriarProdutoValidator _criarValidator;
        private readonly AtualizarProdutoValidator _atualizarValidator;
        private readonly ProdutoService _service;

    public ProdutoServiceTests()
    {
        // Use InMemory database - AppDbContext requires constructor arguments
        // so Mock<AppDbContext> does not work
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProdutoService>>();

        // Initialize validators
        _criarValidator = new CriarProdutoValidator();
        _atualizarValidator = new AtualizarProdutoValidator();

        // Create service with real DbContext (InMemory) and mocked mapper/logger
        _service = new ProdutoService(
            _dbContext,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region ListarProdutosAsync Tests

    /// <summary>
    /// Test: ListarProdutosAsync returns paginated results successfully
    /// Scenario: Valid pagination parameters with sorted results
    /// Expected: Returns PaginatedResponse with correct count and metadata
    /// </summary>
    [Fact]
    public async Task ListarProdutosAsync_WithValidPagination_ReturnsPaginatedProducts()
    {
        // Arrange
        var pagina = 1;
        var itensPorPagina = 10;

        var produtos = CreateMockProdutos(10);
        await _dbContext.Produtos.AddRangeAsync(produtos);
        await _dbContext.SaveChangesAsync();

        _mockMapper
            .Setup(m => m.Map<List<ProdutoResponse>>(It.IsAny<List<Produto>>()))
            .Returns(CreateMockProdutoResponses(10));

        // Act
        var result = await _service.ListarProdutosAsync(pagina, itensPorPagina);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(10);
        result.Pagination.Should().NotBeNull();
        result.Pagination.Page.Should().Be(1);
        result.Pagination.PageSize.Should().Be(10);
    }

    /// <summary>
    /// Test: ListarProdutosAsync with page < 1 normalizes to page 1
    /// Scenario: Page number less than 1 (the service normalizes it to 1)
    /// Expected: Returns results (service normalizes, does not throw)
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ListarProdutosAsync_WithInvalidPageNumber_NormalizesToPage1(int pageNumber)
    {
        // Arrange
        var itensPorPagina = 10;

        _mockMapper
            .Setup(m => m.Map<List<ProdutoResponse>>(It.IsAny<List<Produto>>()))
            .Returns(new List<ProdutoResponse>());

        // Act - the service normalizes page < 1 to page 1 (no exception thrown)
        var result = await _service.ListarProdutosAsync(pageNumber, itensPorPagina);

        // Assert
        result.Should().NotBeNull();
        result.Pagination.Page.Should().Be(1); // normalized
    }

    /// <summary>
    /// Test: ListarProdutosAsync with empty database
    /// Scenario: No products exist
    /// Expected: Returns empty paginated response
    /// </summary>
    [Fact]
    public async Task ListarProdutosAsync_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var pagina = 1;
        var itensPorPagina = 10;

        _mockMapper
            .Setup(m => m.Map<List<ProdutoResponse>>(It.IsAny<List<Produto>>()))
            .Returns(new List<ProdutoResponse>());

        // Act
        var result = await _service.ListarProdutosAsync(pagina, itensPorPagina);

        // Assert
        result.Data.Should().BeEmpty();
        result.Pagination.TotalItems.Should().Be(0);
    }

    #endregion

    #region ObterProdutoAsync Tests

    /// <summary>
    /// Test: ObterProdutoAsync with valid ID
    /// Scenario: Product exists in database
    /// Expected: Returns ProdutoResponse with correct data
    /// </summary>
    [Fact]
    public async Task ObterProdutoAsync_WithValidId_ReturnsProduto()
    {
        // Arrange
        var produto = new Produto
        {
            Id = 1,
            Nome = "Notebook Dell",
            Descricao = "Intel i7, 16GB RAM",
            Preco = 5500.00m,
            Estoque = 10,
            Ativo = true,
            Categoria = "Eletrônicos",
            ContatoEmail = "dell@example.com"
        };
        await _dbContext.Produtos.AddAsync(produto);
        await _dbContext.SaveChangesAsync();

        var response = new ProdutoResponse
        {
            Id = 1,
            Nome = "Notebook Dell",
            Descricao = "Intel i7, 16GB RAM",
            Preco = 5500.00m,
            Estoque = 10
        };

        _mockMapper
            .Setup(m => m.Map<ProdutoResponse>(It.IsAny<Produto>()))
            .Returns(response);

        // Act
        var result = await _service.ObterProdutoAsync(produto.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Nome.Should().Be("Notebook Dell");
        result.Preco.Should().Be(5500.00m);
    }

    /// <summary>
    /// Test: ObterProdutoAsync with non-existent ID
    /// Scenario: Product does not exist
    /// Expected: Returns null (service returns null for not-found)
    /// </summary>
    [Fact]
    public async Task ObterProdutoAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var id = 999;

        // Act
        var result = await _service.ObterProdutoAsync(id);

        // Assert - service returns null for not found (does not throw)
        result.Should().BeNull();
    }

    /// <summary>
    /// Test: ObterProdutoAsync excludes inactive products
    /// Scenario: Product exists but is inactive (soft delete)
    /// Expected: Returns null (inactive products are filtered out)
    /// </summary>
    [Fact]
    public async Task ObterProdutoAsync_WithInactiveProduct_ReturnsNull()
    {
        // Arrange
        var produto = new Produto
        {
            Id = 10,
            Nome = "Produto Inativo",
            Descricao = "descricao",
            Preco = 10m,
            Categoria = "Outros",
            ContatoEmail = "a@b.com",
            Ativo = false
        };
        await _dbContext.Produtos.AddAsync(produto);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.ObterProdutoAsync(produto.Id);

        // Assert - inactive product returns null
        result.Should().BeNull();
    }

    #endregion

    #region CriarProdutoAsync Tests

    /// <summary>
    /// Test: CriarProdutoAsync with valid request
    /// Scenario: All required fields provided, validation passes
    /// Expected: Product created and returned with ID
    /// </summary>
    [Fact]
    public async Task CriarProdutoAsync_WithValidRequest_CreatesProduto()
    {
        // Arrange
        var request = new CriarProdutoRequest
        {
            Nome = "Mouse Logitech",
            Descricao = "Wireless USB",
            Preco = 150.00m,
            Categoria = "Eletrônicos",
            Estoque = 50,
            ContatoEmail = "vendor@example.com"
        };

        var produto = new Produto
        {
            Id = 1,
            Nome = request.Nome,
            Descricao = request.Descricao,
            Preco = request.Preco,
            Categoria = request.Categoria,
            Estoque = request.Estoque,
            ContatoEmail = request.ContatoEmail,
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };

        var response = new ProdutoResponse
        {
            Id = 1,
            Nome = request.Nome,
            Preco = request.Preco,
            Estoque = request.Estoque
        };

        _mockMapper
            .Setup(m => m.Map<Produto>(request))
            .Returns(produto);

        _mockMapper
            .Setup(m => m.Map<ProdutoResponse>(It.IsAny<Produto>()))
            .Returns(response);

        // Act
        var result = await _service.CriarProdutoAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Mouse Logitech");
    }

    #endregion

    #region AtualizarProdutoAsync Tests

    /// <summary>
    /// Test: AtualizarProdutoAsync (PATCH) with partial update
    /// Scenario: Only Nome is updated, other fields remain unchanged via mapper
    /// Expected: Service calls SaveChangesAsync and returns response
    /// </summary>
    [Fact]
    public async Task AtualizarProdutoAsync_WithPartialUpdate_UpdatesProduct()
    {
        // Arrange
        var produto = new Produto
        {
            Id = 1,
            Nome = "Notebook Original",
            Descricao = "descricao",
            Preco = 5000m,
            Estoque = 5,
            Categoria = "Eletrônicos",
            ContatoEmail = "test@example.com",
            Ativo = true
        };
        await _dbContext.Produtos.AddAsync(produto);
        await _dbContext.SaveChangesAsync();

        var request = new AtualizarProdutoRequest
        {
            Nome = "Notebook Atualizado"
        };

        var response = new ProdutoResponse { Id = 1, Nome = "Notebook Atualizado", Preco = 5000m };
        _mockMapper
            .Setup(m => m.Map(request, produto, typeof(AtualizarProdutoRequest), typeof(Produto)))
            .Returns(produto);
        _mockMapper
            .Setup(m => m.Map<ProdutoResponse>(It.IsAny<Produto>()))
            .Returns(response);

        // Act
        var result = await _service.AtualizarProdutoAsync(produto.Id, request);

        // Assert
        result.Should().NotBeNull();
        result!.Nome.Should().Be("Notebook Atualizado");
    }

    /// <summary>
    /// Test: AtualizarProdutoAsync with non-existent product
    /// Scenario: Product ID does not exist
    /// Expected: Returns null (service returns null for not found)
    /// </summary>
    [Fact]
    public async Task AtualizarProdutoAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var id = 999;
        var request = new AtualizarProdutoRequest { Nome = "Test" };

        // Act
        var result = await _service.AtualizarProdutoAsync(id, request);

        // Assert - service returns null for not found
        result.Should().BeNull();
    }

    #endregion

    #region AtualizarCompletoProdutoAsync Tests

    /// <summary>
    /// Test: AtualizarCompletoProdutoAsync (PUT) with full update
    /// Scenario: All product fields are provided
    /// Expected: Complete product replacement with new values
    /// </summary>
    [Fact]
    public async Task AtualizarCompletoProdutoAsync_WithValidRequest_ReplacesAllFields()
    {
        // Arrange
        var produto = new Produto
        {
            Id = 1,
            Nome = "Mouse Antigo",
            Descricao = "Wireless",
            Preco = 150m,
            Categoria = "Eletrônicos",
            Estoque = 50,
            ContatoEmail = "old@example.com",
            Ativo = true
        };
        await _dbContext.Produtos.AddAsync(produto);
        await _dbContext.SaveChangesAsync();

        var request = new CriarProdutoRequest
        {
            Nome = "Mouse Atualizado",
            Descricao = "Wireless + Bluetooth",
            Preco = 200m,
            Categoria = "Eletrônicos",
            Estoque = 75,
            ContatoEmail = "newvendor@example.com"
        };

        var response = new ProdutoResponse
        {
            Id = 1,
            Nome = request.Nome,
            Preco = request.Preco,
            Estoque = request.Estoque
        };

        _mockMapper
            .Setup(m => m.Map(request, produto))
            .Callback<CriarProdutoRequest, Produto>((src, dest) =>
            {
                dest.Nome = src.Nome;
                dest.Preco = src.Preco;
                dest.Estoque = src.Estoque;
            })
            .Returns(produto);

        _mockMapper
            .Setup(m => m.Map<ProdutoResponse>(It.IsAny<Produto>()))
            .Returns(response);

        // Act
        var result = await _service.AtualizarCompletoProdutoAsync(produto.Id, request);

        // Assert
        result.Should().NotBeNull();
        result!.Nome.Should().Be("Mouse Atualizado");
        result.Preco.Should().Be(200m);
    }

    #endregion

    #region DeletarProdutoAsync Tests

    /// <summary>
    /// Test: DeletarProdutoAsync with valid ID
    /// Scenario: Products exists, soft delete is performed
    /// Expected: Returns true, product marked as inactive
    /// </summary>
    [Fact]
    public async Task DeletarProdutoAsync_WithValidId_PerformsSoftDelete()
    {
        // Arrange
        var produto = new Produto
        {
            Id = 1,
            Nome = "Produto a Deletar",
            Descricao = "desc",
            Preco = 10m,
            Categoria = "Outros",
            ContatoEmail = "a@b.com",
            Ativo = true
        };
        await _dbContext.Produtos.AddAsync(produto);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DeletarProdutoAsync(produto.Id);

        // Assert
        result.Should().BeTrue();

        // Verify product is now inactive
        var produtoAtualizado = await _dbContext.Produtos.FindAsync(produto.Id);
        produtoAtualizado!.Ativo.Should().BeFalse();
    }

    /// <summary>
    /// Test: DeletarProdutoAsync with non-existent ID
    /// Scenario: Product does not exist
    /// Expected: Returns false (service returns false for not found)
    /// </summary>
    [Fact]
    public async Task DeletarProdutoAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var id = 999;

        // Act
        var result = await _service.DeletarProdutoAsync(id);

        // Assert - service returns false for not found
        result.Should().BeFalse();
    }

    /// <summary>
    /// Test: DeletarProdutoAsync with already deleted product
    /// Scenario: Product already soft-deleted (Ativo = false)
    /// Expected: Returns false (inactive products not found by query)
    /// </summary>
    [Fact]
    public async Task DeletarProdutoAsync_WithAlreadyDeletedProduct_ReturnsFalse()
    {
        // Arrange
        var produto = new Produto
        {
            Id = 2,
            Nome = "Já Deletado",
            Descricao = "desc",
            Preco = 10m,
            Categoria = "Outros",
            ContatoEmail = "a@b.com",
            Ativo = false
        };
        await _dbContext.Produtos.AddAsync(produto);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DeletarProdutoAsync(produto.Id);

        // Assert - inactive products are not found, returns false
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a list of mock Produto objects for testing
    /// </summary>
    private List<Produto> CreateMockProdutos(int count)
    {
        var produtos = new List<Produto>();
        for (int i = 1; i <= count; i++)
        {
            produtos.Add(new Produto
            {
                Nome = $"Produto {i}",
                Descricao = $"Descrição do Produto {i}",
                Preco = 100m * i,
                Categoria = "Eletrônicos",
                Estoque = 10 * i,
                Ativo = true,
                ContatoEmail = $"vendor{i}@example.com",
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            });
        }
        return produtos;
    }

    /// <summary>
    /// Creates a list of mock ProdutoResponse objects
    /// </summary>
    private List<ProdutoResponse> CreateMockProdutoResponses(int count)
    {
        var responses = new List<ProdutoResponse>();
        for (int i = 1; i <= count; i++)
        {
            responses.Add(new ProdutoResponse
            {
                Id = i,
                Nome = $"Produto {i}",
                Descricao = $"Descrição do Produto {i}",
                Preco = 100m * i,
                Categoria = "Eletrônicos",
                Estoque = 10 * i
            });
        }
        return responses;
    }

    #endregion
}
}

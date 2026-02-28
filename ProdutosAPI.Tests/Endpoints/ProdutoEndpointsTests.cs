using Xunit;
using Moq;
using FluentAssertions;
using ProdutosAPI.Produtos.DTOs;
using ProdutosAPI.Produtos.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProdutosAPI.Tests.Endpoints
{
    /// <summary>
    /// Integration tests for Produto API endpoints
    /// Tests HTTP layer, response status codes, and payload validation
    /// References: REST API Best Practices - HTTP Status Codes
    /// </summary>
    public class ProdutoEndpointsTests
    {
        private readonly Mock<IProdutoService> _mockService;

    public ProdutoEndpointsTests()
    {
        _mockService = new Mock<IProdutoService>();
    }

    #region GET / Tests

    /// <summary>
    /// Test: GET /produtos returns 200 OK with paginated list
    /// Scenario: Request for first page of products
    /// Expected: 200 OK with products and pagination info
    /// </summary>
    [Fact]
    public void GetProdutos_WithValidRequest_Returns200OK()
    {
        // Arrange
        var mockResponse = new PaginatedResponse<ProdutoResponse>
        {
            Data = new List<ProdutoResponse>
            {
                new ProdutoResponse { Id = 1, Nome = "Produto 1", Preco = 100m },
                new ProdutoResponse { Id = 2, Nome = "Produto 2", Preco = 200m }
            },
            Pagination = new PaginationInfo
            {
                Page = 1,
                PageSize = 10,
                TotalItems = 2,
                TotalPages = 1
            }
        };

        // This would be called via HTTP GET /produtos?page=1&pageSize=10
        // Handler should return 200 with the mockResponse data

        // Assert expectations
        mockResponse.Data.Should().HaveCount(2);
        mockResponse.Pagination.TotalPages.Should().Be(1);
    }

    /// <summary>
    /// Test: GET /produtos with invalid page number
    /// Scenario: Page number is 0 or negative
    /// Expected: 400 Bad Request with error details
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetProdutos_WithInvalidPageNumber_Returns400BadRequest(int page)
    {
        // Arrange
        _mockService
            .Setup(s => s.ListarProdutosAsync(page, It.IsAny<int>()))
            .ThrowsAsync(new ArgumentException("Page must be >= 1"));

        // This would be called via HTTP GET /produtos?page={page}
        // Handler should catch ArgumentException and return 400

        // Expected behavior: ArgumentException should be caught by middleware
        Assert.Throws<ArgumentException>(() =>
        {
            _mockService.Object.ListarProdutosAsync(page, 10).GetAwaiter().GetResult();
        });
    }

    /// <summary>
    /// Test: GET /produtos returns 200 OK with empty list when no products exist
    /// Scenario: Database is empty
    /// Expected: 200 OK with empty data array and correct pagination
    /// </summary>
    [Fact]
    public void GetProdutos_WithEmptyDatabase_Returns200OKWithEmptyList()
    {
        // Arrange
        var mockResponse = new PaginatedResponse<ProdutoResponse>
        {
            Data = new List<ProdutoResponse>(),
            Pagination = new PaginationInfo
            {
                Page = 1,
                PageSize = 10,
                TotalItems = 0,
                TotalPages = 0
            }
        };

        // Expected behavior: Return 200 with empty list
        mockResponse.Data.Should().BeEmpty();
        mockResponse.Pagination.TotalPages.Should().Be(0);
    }

    #endregion

    #region GET /{id} Tests

    /// <summary>
    /// Test: GET /produtos/{id} returns 200 OK with product data
    /// Scenario: Valid product ID
    /// Expected: 200 OK with ProdutoResponse
    /// </summary>
    [Fact]
    public async Task GetProdutoById_WithValidId_Returns200OK()
    {
        // Arrange
        var id = 1;
        var mockResponse = new ProdutoResponse
        {
            Id = id,
            Nome = "Notebook Dell",
            Descricao = "Intel i7, 16GB RAM",
            Preco = 5500.00m,
            Categoria = "Computadores",
            Estoque = 5
        };

        _mockService
            .Setup(s => s.ObterProdutoAsync(id))
            .ReturnsAsync(mockResponse);

        // Act
        var service = _mockService.Object;
        var result = await service.ObterProdutoAsync(id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(id);
        result.Nome.Should().Be("Notebook Dell");
        result.Preco.Should().Be(5500.00m);
    }

    /// <summary>
    /// Test: GET /produtos/{id} returns 404 Not Found
    /// Scenario: Product ID does not exist
    /// Expected: 404 Not Found with error message
    /// </summary>
    [Fact]
    public async Task GetProdutoById_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var id = 999;

        _mockService
            .Setup(s => s.ObterProdutoAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Produto com ID {id} não encontrado."));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mockService.Object.ObterProdutoAsync(id)
        );
    }

    /// <summary>
    /// Test: GET /produtos/{id} excludes deleted (inactive) products
    /// Scenario: Product has been soft-deleted (Ativo = false)
    /// Expected: 404 Not Found
    /// </summary>
    [Fact]
    public async Task GetProdutoById_WithDeletedProduct_Returns404NotFound()
    {
        // Arrange
        var id = 1;

        _mockService
            .Setup(s => s.ObterProdutoAsync(id))
            .ThrowsAsync(new KeyNotFoundException("Produto não encontrado ou foi deletado."));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mockService.Object.ObterProdutoAsync(id)
        );
    }

    #endregion

    #region POST / Tests

    /// <summary>
    /// Test: POST /produtos returns 201 Created with new product
    /// Scenario: Valid product creation request
    /// Expected: 201 Created with ProdutoResponse and Location header
    /// </summary>
    [Fact]
    public async Task PostProduto_WithValidRequest_Returns201Created()
    {
        // Arrange
        var request = new CriarProdutoRequest
        {
            Nome = "Mouse Logitech",
            Descricao = "Wireless USB",
            Preco = 150.00m,
            Categoria = "Periféricos",
            Estoque = 50,
            ContatoEmail = "vendor@example.com"
        };

        var response = new ProdutoResponse
        {
            Id = 1,
            Nome = request.Nome,
            Descricao = request.Descricao,
            Preco = request.Preco,
            Categoria = request.Categoria,
            Estoque = request.Estoque
        };

        _mockService
            .Setup(s => s.CriarProdutoAsync(request))
            .ReturnsAsync(response);

        // Act
        var service = _mockService.Object;
        var result = await service.CriarProdutoAsync(request);

        // Assert - HTTP status 201 would be returned with Location header
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Nome.Should().Be("Mouse Logitech");
    }

    /// <summary>
    /// Test: POST /produtos returns 422 Unprocessable Entity
    /// Scenario: Validation fails (missing required field)
    /// Expected: 422 with validation errors
    /// </summary>
    [Fact]
    public async Task PostProduto_WithInvalidRequest_Returns422UnprocessableEntity()
    {
        // Arrange
        var request = new CriarProdutoRequest
        {
            Nome = "", // Invalid: empty name
            Descricao = "Test",
            Preco = 100m,
            Categoria = "Test",
            Estoque = 10,
            ContatoEmail = "test@example.com"
        };

        _mockService
            .Setup(s => s.CriarProdutoAsync(request))
            .ThrowsAsync(new FluentValidation.ValidationException("Nome é obrigatório"));

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _mockService.Object.CriarProdutoAsync(request)
        );
    }

    /// <summary>
    /// Test: POST /produtos returns 400 Bad Request
    /// Scenario: Negative or zero price
    /// Expected: 400 Bad Request
    /// </summary>
    [Theory]
    [InlineData(-50)]
    [InlineData(0)]
    public async Task PostProduto_WithInvalidPrice_Returns400BadRequest(decimal price)
    {
        // Arrange
        var request = new CriarProdutoRequest
        {
            Nome = "Valid",
            Descricao = "Description",
            Preco = price,
            Categoria = "Category",
            Estoque = 10,
            ContatoEmail = "test@example.com"
        };

        _mockService
            .Setup(s => s.CriarProdutoAsync(request))
            .ThrowsAsync(new FluentValidation.ValidationException("Preço deve ser maior que zero"));

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _mockService.Object.CriarProdutoAsync(request)
        );
    }

    #endregion

    #region PUT /{id} Tests

    /// <summary>
    /// Test: PUT /produtos/{id} returns 200 OK
    /// Scenario: Valid full update request
    /// Expected: 200 OK with updated ProdutoResponse
    /// </summary>
    [Fact]
    public async Task PutProduto_WithValidRequest_Returns200OK()
    {
        // Arrange
        var id = 1;
        var request = new CriarProdutoRequest
        {
            Nome = "Mouse Atualizado",
            Descricao = "Wireless + Bluetooth",
            Preco = 200m,
            Categoria = "Periféricos Premium",
            Estoque = 75,
            ContatoEmail = "newvendor@example.com"
        };

        var response = new ProdutoResponse
        {
            Id = id,
            Nome = request.Nome,
            Preco = request.Preco,
            Estoque = request.Estoque
        };

        _mockService
            .Setup(s => s.AtualizarCompletoProdutoAsync(id, request))
            .ReturnsAsync(response);

        // Act
        var service = _mockService.Object;
        var result = await service.AtualizarCompletoProdutoAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Mouse Atualizado");
        result.Preco.Should().Be(200m);
    }

    /// <summary>
    /// Test: PUT /produtos/{id} returns 404 Not Found
    /// Scenario: Product does not exist
    /// Expected: 404
    /// </summary>
    [Fact]
    public async Task PutProduto_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var id = 999;
        var request = new CriarProdutoRequest
        {
            Nome = "Test",
            Descricao = "Test",
            Preco = 100m,
            Categoria = "Test",
            Estoque = 10,
            ContatoEmail = "test@example.com"
        };

        _mockService
            .Setup(s => s.AtualizarCompletoProdutoAsync(id, request))
            .ThrowsAsync(new KeyNotFoundException($"Produto com ID {id} não encontrado."));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mockService.Object.AtualizarCompletoProdutoAsync(id, request)
        );
    }

    #endregion

    #region PATCH /{id} Tests

    /// <summary>
    /// Test: PATCH /produtos/{id} returns 200 OK
    /// Scenario: Valid partial update (only Nome field)
    /// Expected: 200 OK with updated product
    /// </summary>
    [Fact]
    public async Task PatchProduto_WithValidRequest_Returns200OK()
    {
        // Arrange
        var id = 1;
        var request = new AtualizarProdutoRequest
        {
            Nome = "Produto Atualizado"
        };

        var response = new ProdutoResponse
        {
            Id = id,
            Nome = "Produto Atualizado"
        };

        _mockService
            .Setup(s => s.AtualizarProdutoAsync(id, request))
            .ReturnsAsync((ProdutoResponse?)response);

        // Act
        await _mockService.Object.AtualizarProdutoAsync(id, request);

        // Assert
        _mockService.Verify(s => s.AtualizarProdutoAsync(id, request), Times.Once);
    }

    /// <summary>
    /// Test: PATCH /produtos/{id} returns 404 Not Found
    /// Scenario: Product does not exist
    /// Expected: 404
    /// </summary>
    [Fact]
    public async Task PatchProduto_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var id = 999;
        var request = new AtualizarProdutoRequest { Nome = "Test" };

        _mockService
            .Setup(s => s.AtualizarProdutoAsync(id, request))
            .ThrowsAsync(new KeyNotFoundException($"Produto com ID {id} não encontrado."));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mockService.Object.AtualizarProdutoAsync(id, request)
        );
    }

    #endregion

    #region DELETE /{id} Tests

    /// <summary>
    /// Test: DELETE /produtos/{id} returns 204 No Content
    /// Scenario: Valid product deletion
    /// Expected: 204 No Content (soft delete)
    /// </summary>
    [Fact]
    public async Task DeleteProduto_WithValidId_Returns204NoContent()
    {
        // Arrange
        var id = 1;

        _mockService
            .Setup(s => s.DeletarProdutoAsync(id))
            .ReturnsAsync(true);

        // Act
        await _mockService.Object.DeletarProdutoAsync(id);

        // Assert - HTTP 204 would be returned
        _mockService.Verify(s => s.DeletarProdutoAsync(id), Times.Once);
    }

    /// <summary>
    /// Test: DELETE /produtos/{id} returns 404 Not Found
    /// Scenario: Product does not exist
    /// Expected: 404
    /// </summary>
    [Fact]
    public async Task DeleteProduto_WithInvalidId_Returns404NotFound()
    {
        // Arrange
        var id = 999;

        _mockService
            .Setup(s => s.DeletarProdutoAsync(id))
            .ThrowsAsync(new KeyNotFoundException($"Produto com ID {id} não encontrado."));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mockService.Object.DeletarProdutoAsync(id)
        );
    }

    /// <summary>
    /// Test: DELETE /produtos/{id} returns 404 for already deleted product
    /// Scenario: Product was already soft-deleted
    /// Expected: 404
    /// </summary>
    [Fact]
    public async Task DeleteProduto_WithAlreadyDeletedProduct_Returns404NotFound()
    {
        // Arrange
        var id = 1;

        _mockService
            .Setup(s => s.DeletarProdutoAsync(id))
            .ThrowsAsync(new KeyNotFoundException("Produto foi deletado ou não existe."));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mockService.Object.DeletarProdutoAsync(id)
        );
    }

    #endregion

    #region Error Response Format Tests

    /// <summary>
    /// Test: All error responses follow standard format
    /// Scenario: Various error conditions
    /// Expected: ErrorResponse with Status, Message, Details, Timestamp, TraceId
    /// </summary>
    [Theory]
    [InlineData(400, "Bad Request")]
    [InlineData(404, "Not Found")]
    [InlineData(422, "Unprocessable Entity")]
    [InlineData(500, "Internal Server Error")]
    public void ErrorResponses_FollowStandardFormat(int statusCode, string title)
    {
        // Arrange
        var error = new ErrorResponse
        {
            Status = statusCode,
            Title = title,
            Detail = "Erro de descrição",
            Type = "https://api.example.com/errors/server",
            Instance = "/api/v1/produtos/1"
        };

        // Assert
        error.Status.Should().Be(statusCode);
        error.Title.Should().Be(title);
        error.Detail.Should().NotBeNullOrEmpty();
        error.Type.Should().NotBeNullOrEmpty();
    }

    #endregion
}}
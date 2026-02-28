using FluentValidation;
using Microsoft.AspNetCore.Http;
using ProdutosAPI.Produtos.DTOs;
using ProdutosAPI.Produtos.Services;

namespace ProdutosAPI.Produtos.Endpoints;

/// <summary>
/// Extensão para registrar todos os endpoints da API
/// Referência: Melhores-Praticas-API.md - Seção "Design de Endpoints"
/// Implementa todos os endpoints RESTful seguindo as melhores práticas do .NET 10 Minimal API
/// </summary>
public static class ProdutoEndpoints
{
    /// <summary>
    /// Registra todos os endpoints de produtos
    /// </summary>
    public static void MapProdutoEndpoints(this WebApplication app)
    {
        const string Tag = "Produtos";
        const string BaseRoute = "/api/v1/produtos";

        var group = app.MapGroup(BaseRoute)
            .WithName("Produtos")
            .WithTags(Tag)
            .WithDescription("Endpoints para gerenciamento de produtos");

        // GET - Listar produtos com paginação
        group.MapGet("/", ListarProdutos)
            .WithName("ListarProdutos")
            .WithDescription("Lista todos os produtos com paginação e filtros opcionais")
            .WithSummary("Listar produtos")
            .Produces<PaginatedResponse<ProdutoResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        // GET - Obter um produto específico
        group.MapGet("/{id}", ObterProduto)
            .WithName("ObterProduto")
            .WithDescription("Obtém um produto específico pelo ID")
            .WithSummary("Obter produto (GET)")
            .Produces<ProdutoResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        // POST - Criar novo produto
        group.MapPost("/", CriarProduto)
            .WithName("CriarProduto")
            .WithDescription("Cria um novo produto com validação completa")
            .WithSummary("Criar produto")
            .Accepts<CriarProdutoRequest>("application/json")
            .Produces<ProdutoResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .RequireAuthorization();

        // PUT - Atualizar produto completamente
        group.MapPut("/{id}", AtualizarCompletoProduto)
            .WithName("AtualizarCompletoProduto")
            .WithDescription("Atualiza COMPLETAMENTE um produto (substitui todos os campos)")
            .WithSummary("Atualizar produto (PUT)")
            .Accepts<CriarProdutoRequest>("application/json")
            .Produces<ProdutoResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .RequireAuthorization();

        // PATCH - Atualizar produto parcialmente
        group.MapPatch("/{id}", AtualizarParcialProduto)
            .WithName("AtualizarParcialProduto")
            .WithDescription("Atualiza PARCIALMENTE um produto (apenas campos fornecidos)")
            .WithSummary("Atualizar produto (PATCH)")
            .Accepts<AtualizarProdutoRequest>("application/json")
            .Produces<ProdutoResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .RequireAuthorization();

        // DELETE - Deletar produto
        group.MapDelete("/{id}", DeletarProduto)
            .WithName("DeletarProduto")
            .WithDescription("Deleta um produto (soft delete - marca como inativo)")
            .WithSummary("Deletar produto")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    /// <summary>
    /// GET /api/v1/produtos
    /// </summary>
    private static async Task<IResult> ListarProdutos(
        IProdutoService produtoService,
        int page = 1,
        int pageSize = 20,
        ILogger<Program> logger = null!)
    {
        logger?.LogInformation("GET /api/v1/produtos - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        try
        {
            var resultado = await produtoService.ListarProdutosAsync(page, pageSize);
            return Results.Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            logger?.LogWarning("Validação falhou: {Message}", ex.Message);
            return Results.BadRequest(new ErrorResponse
            {
                Status = 400,
                Title = "Parâmetros inválidos",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/validation"
            });
        }
    }

    /// <summary>
    /// GET /api/v1/produtos/{id}
    /// </summary>
    private static async Task<IResult> ObterProduto(
        int id,
        IProdutoService produtoService,
        ILogger<Program> logger = null!)
    {
        logger?.LogInformation("GET /api/v1/produtos/{Id}", id);

        try
        {
            var produto = await produtoService.ObterProdutoAsync(id);
            return Results.Ok(produto);
        }
        catch (KeyNotFoundException ex)
        {
            logger?.LogWarning("Produto {ProductId} não encontrado", id);
            return Results.NotFound(new ErrorResponse
            {
                Status = 404,
                Title = "Produto não encontrado",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/not-found",
                Instance = $"/api/v1/produtos/{id}"
            });
        }
    }

    /// <summary>
    /// POST /api/v1/produtos
    /// </summary>
    private static async Task<IResult> CriarProduto(
        CriarProdutoRequest request,
        IProdutoService produtoService,
        IValidator<CriarProdutoRequest> validator,
        ILogger<Program> logger = null!)
    {
        logger?.LogInformation("POST /api/v1/produtos - Criando novo produto: {Nome}", request.Nome);

        try
        {
            // Validar request
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                logger?.LogWarning("Validação falhou ao criar produto");
                return Results.UnprocessableEntity(new ErrorResponse
                {
                    Status = 422,
                    Title = "Validação falhou",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Type = "https://api.example.com/errors/validation"
                });
            }

            var produto = await produtoService.CriarProdutoAsync(request);
            logger?.LogInformation("Produto criado com sucesso. ID: {ProductId}", produto.Id);
            
            return Results.Created($"/api/v1/produtos/{produto.Id}", produto);
        }
        catch (ValidationException ex)
        {
            logger?.LogWarning("Validação falhou: {Message}", ex.Message);
            return Results.UnprocessableEntity(new ErrorResponse
            {
                Status = 422,
                Title = "Validação falhou",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/validation"
            });
        }
    }

    /// <summary>
    /// PUT /api/v1/produtos/{id}
    /// </summary>
    private static async Task<IResult> AtualizarCompletoProduto(
        int id,
        CriarProdutoRequest request,
        IProdutoService produtoService,
        IValidator<CriarProdutoRequest> validator,
        ILogger<Program> logger = null!)
    {
        logger?.LogInformation("PUT /api/v1/produtos/{Id} - Atualizando completamente", id);

        try
        {
            // Validar request
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                logger?.LogWarning("Validação falhou ao atualizar produto {ProductId}", id);
                return Results.UnprocessableEntity(new ErrorResponse
                {
                    Status = 422,
                    Title = "Validação falhou",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Type = "https://api.example.com/errors/validation"
                });
            }

            var produto = await produtoService.AtualizarCompletoProdutoAsync(id, request);
            logger?.LogInformation("Produto {ProductId} atualizado completamente com sucesso", id);
            return Results.Ok(produto);
        }
        catch (KeyNotFoundException ex)
        {
            logger?.LogWarning("Produto {ProductId} não encontrado", id);
            return Results.NotFound(new ErrorResponse
            {
                Status = 404,
                Title = "Produto não encontrado",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/not-found",
                Instance = $"/api/v1/produtos/{id}"
            });
        }
        catch (ValidationException ex)
        {
            logger?.LogWarning("Validação falhou: {Message}", ex.Message);
            return Results.UnprocessableEntity(new ErrorResponse
            {
                Status = 422,
                Title = "Validação falhou",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/validation"
            });
        }
    }

    /// <summary>
    /// PATCH /api/v1/produtos/{id}
    /// </summary>
    private static async Task<IResult> AtualizarParcialProduto(
        int id,
        AtualizarProdutoRequest request,
        IProdutoService produtoService,
        IValidator<AtualizarProdutoRequest> validator,
        ILogger<Program> logger = null!)
    {
        logger?.LogInformation("PATCH /api/v1/produtos/{Id} - Atualizando parcialmente", id);

        try
        {
            // Validar request
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                logger?.LogWarning("Validação falhou ao atualizar parcialmente produto {ProductId}", id);
                return Results.UnprocessableEntity(new ErrorResponse
                {
                    Status = 422,
                    Title = "Validação falhou",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Type = "https://api.example.com/errors/validation"
                });
            }

            await produtoService.AtualizarProdutoAsync(id, request);
            var produtoAtualizado = await produtoService.ObterProdutoAsync(id);
            logger?.LogInformation("Produto {ProductId} atualizado parcialmente com sucesso", id);
            return Results.Ok(produtoAtualizado);
        }
        catch (KeyNotFoundException ex)
        {
            logger?.LogWarning("Produto {ProductId} não encontrado", id);
            return Results.NotFound(new ErrorResponse
            {
                Status = 404,
                Title = "Produto não encontrado",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/not-found",
                Instance = $"/api/v1/produtos/{id}"
            });
        }
        catch (ValidationException ex)
        {
            logger?.LogWarning("Validação falhou: {Message}", ex.Message);
            return Results.UnprocessableEntity(new ErrorResponse
            {
                Status = 422,
                Title = "Validação falhou",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/validation"
            });
        }
    }

    /// <summary>
    /// DELETE /api/v1/produtos/{id}
    /// </summary>
    private static async Task<IResult> DeletarProduto(
        int id,
        IProdutoService produtoService,
        ILogger<Program> logger = null!)
    {
        logger?.LogInformation("DELETE /api/v1/produtos/{Id} - Deletando", id);

        try
        {
            await produtoService.DeletarProdutoAsync(id);
            logger?.LogInformation("Produto {ProductId} deletado com sucesso", id);
            return Results.NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            logger?.LogWarning("Produto {ProductId} não encontrado", id);
            return Results.NotFound(new ErrorResponse
            {
                Status = 404,
                Title = "Produto não encontrado",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/not-found",
                Instance = $"/api/v1/produtos/{id}"
            });
        }
    }
}

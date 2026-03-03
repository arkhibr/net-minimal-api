using FluentValidation;
using Microsoft.AspNetCore.Http;
using ProdutosAPI.Produtos.Application.DTOs;
using ProdutosAPI.Produtos.Application.Services;

namespace ProdutosAPI.Produtos.API.Endpoints;

public static class ProdutoEndpoints
{
    public static void MapProdutoEndpoints(this WebApplication app)
    {
        const string Tag = "Produtos";
        const string BaseRoute = "/api/v1/produtos";

        var group = app.MapGroup(BaseRoute)
            .WithName("Produtos")
            .WithTags(Tag)
            .WithDescription("Endpoints para gerenciamento de produtos");

        group.MapGet("/", ListarProdutos)
            .WithName("ListarProdutos")
            .WithDescription("Lista todos os produtos com paginação e filtros opcionais")
            .WithSummary("Listar produtos")
            .Produces<PaginatedResponse<ProdutoResponse>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapGet("/{id}", ObterProduto)
            .WithName("ObterProduto")
            .WithDescription("Obtém um produto específico pelo ID")
            .WithSummary("Obter produto (GET)")
            .Produces<ProdutoResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapPost("/", CriarProduto)
            .WithName("CriarProduto")
            .WithDescription("Cria um novo produto com validação completa")
            .WithSummary("Criar produto")
            .Accepts<CriarProdutoRequest>("application/json")
            .Produces<ProdutoResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .RequireAuthorization();

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

        group.MapDelete("/{id}", DeletarProduto)
            .WithName("DeletarProduto")
            .WithDescription("Deleta um produto (soft delete - marca como inativo)")
            .WithSummary("Deletar produto")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    private static async Task<IResult> ListarProdutos(
        IProdutoService produtoService,
        int page = 1,
        int pageSize = 20,
        string? categoria = null,
        string? search = null)
    {
        try
        {
            var resultado = await produtoService.ListarProdutosAsync(page, pageSize, categoria, search);
            return Results.Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ErrorResponse
            {
                Status = 400,
                Title = "Parâmetros inválidos",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/validation"
            });
        }
    }

    private static async Task<IResult> ObterProduto(
        int id,
        IProdutoService produtoService)
    {
        var produto = await produtoService.ObterProdutoAsync(id);
        if (produto is null)
        {
            return Results.NotFound(new ErrorResponse
            {
                Status = 404,
                Title = "Produto não encontrado",
                Detail = $"Produto com ID {id} não encontrado.",
                Type = "https://api.example.com/errors/not-found",
                Instance = $"/api/v1/produtos/{id}"
            });
        }

        return Results.Ok(produto);
    }

    private static async Task<IResult> CriarProduto(
        CriarProdutoRequest request,
        IProdutoService produtoService,
        IValidator<CriarProdutoRequest> validator)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return Results.UnprocessableEntity(new ErrorResponse
                {
                    Status = 422,
                    Title = "Validação falhou",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Type = "https://api.example.com/errors/validation"
                });
            }

            var produto = await produtoService.CriarProdutoAsync(request);
            return Results.Created($"/api/v1/produtos/{produto.Id}", produto);
        }
        catch (ValidationException ex)
        {
            return Results.UnprocessableEntity(new ErrorResponse
            {
                Status = 422,
                Title = "Validação falhou",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/validation"
            });
        }
    }

    private static async Task<IResult> AtualizarCompletoProduto(
        int id,
        CriarProdutoRequest request,
        IProdutoService produtoService,
        IValidator<CriarProdutoRequest> validator)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return Results.UnprocessableEntity(new ErrorResponse
                {
                    Status = 422,
                    Title = "Validação falhou",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Type = "https://api.example.com/errors/validation"
                });
            }

            var produto = await produtoService.AtualizarCompletoProdutoAsync(id, request);
            if (produto is null)
            {
                return Results.NotFound(new ErrorResponse
                {
                    Status = 404,
                    Title = "Produto não encontrado",
                    Detail = $"Produto com ID {id} não encontrado.",
                    Type = "https://api.example.com/errors/not-found",
                    Instance = $"/api/v1/produtos/{id}"
                });
            }

            return Results.Ok(produto);
        }
        catch (ValidationException ex)
        {
            return Results.UnprocessableEntity(new ErrorResponse
            {
                Status = 422,
                Title = "Validação falhou",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/validation"
            });
        }
    }

    private static async Task<IResult> AtualizarParcialProduto(
        int id,
        AtualizarProdutoRequest request,
        IProdutoService produtoService,
        IValidator<AtualizarProdutoRequest> validator)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return Results.UnprocessableEntity(new ErrorResponse
                {
                    Status = 422,
                    Title = "Validação falhou",
                    Detail = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    Type = "https://api.example.com/errors/validation"
                });
            }

            var produtoAtualizado = await produtoService.AtualizarProdutoAsync(id, request);
            if (produtoAtualizado is null)
            {
                return Results.NotFound(new ErrorResponse
                {
                    Status = 404,
                    Title = "Produto não encontrado",
                    Detail = $"Produto com ID {id} não encontrado.",
                    Type = "https://api.example.com/errors/not-found",
                    Instance = $"/api/v1/produtos/{id}"
                });
            }

            return Results.Ok(produtoAtualizado);
        }
        catch (ValidationException ex)
        {
            return Results.UnprocessableEntity(new ErrorResponse
            {
                Status = 422,
                Title = "Validação falhou",
                Detail = ex.Message,
                Type = "https://api.example.com/errors/validation"
            });
        }
    }

    private static async Task<IResult> DeletarProduto(
        int id,
        IProdutoService produtoService)
    {
        var deletado = await produtoService.DeletarProdutoAsync(id);
        if (!deletado)
        {
            return Results.NotFound(new ErrorResponse
            {
                Status = 404,
                Title = "Produto não encontrado",
                Detail = $"Produto com ID {id} não encontrado.",
                Type = "https://api.example.com/errors/not-found",
                Instance = $"/api/v1/produtos/{id}"
            });
        }

        return Results.NoContent();
    }
}

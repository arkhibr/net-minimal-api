using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ProdutosAPI.Produtos.Application.Repositories;
using ProdutosAPI.Produtos.Application.Services;
using ProdutosAPI.Produtos.Application.Validators;
using ProdutosAPI.Produtos.Infrastructure.Repositories;

namespace ProdutosAPI.Produtos.API.Extensions;

public static class ProdutosServiceExtensions
{
    /// <summary>
    /// Registra todos os serviços do feature Produtos no container de DI.
    /// Chamada em Program.cs: builder.Services.AddProdutos();
    /// </summary>
    public static IServiceCollection AddProdutos(this IServiceCollection services)
    {
        // Application services
        services.AddScoped<IProdutoService, ProdutoService>();

        // Repository (Infrastructure)
        services.AddScoped<IProdutoRepository, DapperProdutoRepository>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<CriarProdutoValidator>();

        return services;
    }
}

using FluentValidation;
using ProdutosAPI.Catalogo.Application.Repositories;
using ProdutosAPI.Catalogo.Application.Services;
using ProdutosAPI.Catalogo.Application.Validators;
using ProdutosAPI.Catalogo.Infrastructure.Queries;
using ProdutosAPI.Catalogo.Infrastructure.Repositories;

namespace ProdutosAPI.Catalogo.API.Extensions;

public static class CatalogoServiceExtensions
{
    public static IServiceCollection AddCatalogo(this IServiceCollection services)
    {
        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<IProdutoQueryRepository, DapperProdutoQueryRepository>();
        services.AddScoped<IProdutoCommandRepository, EfProdutoCommandRepository>();
        services.AddValidatorsFromAssemblyContaining<CriarProdutoValidator>();
        return services;
    }
}

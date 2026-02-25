using AutoMapper;
using ProdutosAPI.DTOs;
using ProdutosAPI.Models;

namespace ProdutosAPI.Common;

/// <summary>
/// Configuração de mapeamento AutoMapper
/// Mapeia entre entidades e DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeamento de Produto para ProdutoResponse
        CreateMap<Produto, ProdutoResponse>();

        // Mapeamento de CriarProdutoRequest para Produto
        CreateMap<CriarProdutoRequest, Produto>();

        // Mapeamento de AtualizarProdutoRequest para Produto (ignorando campos nulos)
        CreateMap<AtualizarProdutoRequest, Produto>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

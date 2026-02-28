using AutoMapper;
using ProdutosAPI.Produtos.DTOs;
using ProdutosAPI.Produtos.Models;

namespace ProdutosAPI.Shared.Common;

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
    }
}

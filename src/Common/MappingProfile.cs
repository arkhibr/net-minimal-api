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
    }
}

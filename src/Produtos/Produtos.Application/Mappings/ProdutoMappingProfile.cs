using AutoMapper;
using ProdutosAPI.Produtos.Application.DTOs;
using ProdutosAPI.Produtos.Domain;

namespace ProdutosAPI.Produtos.Application.Mappings;

public class ProdutoMappingProfile : Profile
{
    public ProdutoMappingProfile()
    {
        CreateMap<Produto, ProdutoResponse>();
    }
}

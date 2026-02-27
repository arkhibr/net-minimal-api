using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ProdutosAPI.Features.Pedidos.CreatePedido;
using ProdutosAPI.Features.Pedidos.ListPedidos;
using Xunit;

namespace ProdutosAPI.Tests.Integration.Pedidos;

public class ListPedidosTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ListPedidosTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var token = await AuthHelper.ObterTokenAsync(_client);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GET_Pedidos_SemAutenticacao_Retorna401()
    {
        var response = await _client.GetAsync("/api/v1/pedidos");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_Pedidos_Autenticado_Retorna200()
    {
        await AuthenticateAsync();

        // Create a pedido first so there's something to list
        await _client.PostAsJsonAsync("/api/v1/pedidos",
            new CreatePedidoCommand([new(1, 1)]));

        var response = await _client.GetAsync("/api/v1/pedidos");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ListPedidosResponse>();
        result.Should().NotBeNull();
        result!.Total.Should().BeGreaterThan(0);
    }
}

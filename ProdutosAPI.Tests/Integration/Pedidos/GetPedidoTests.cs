using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ProdutosAPI.Pedidos.Common;
using ProdutosAPI.Pedidos.CreatePedido;
using Xunit;

namespace ProdutosAPI.Tests.Integration.Pedidos;

public class GetPedidoTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public GetPedidoTests(ApiFactory factory)
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
    public async Task GET_Pedido_NaoExistente_Retorna404()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync("/api/v1/pedidos/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GET_Pedido_Existente_Retorna200ComDados()
    {
        await AuthenticateAsync();

        // Create a pedido first
        var createResponse = await _client.PostAsJsonAsync("/api/v1/pedidos",
            new CreatePedidoCommand([new(1, 2)]));
        createResponse.EnsureSuccessStatusCode();
        var criado = await createResponse.Content.ReadFromJsonAsync<PedidoResponse>();

        // Fetch it
        var response = await _client.GetAsync($"/api/v1/pedidos/{criado!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pedido = await response.Content.ReadFromJsonAsync<PedidoResponse>();
        pedido!.Id.Should().Be(criado.Id);
        pedido.Itens.Should().HaveCount(1);
        pedido.Itens.First().Quantidade.Should().Be(2);
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ProdutosAPI.Pedidos.AddItemPedido;
using ProdutosAPI.Pedidos.Common;
using ProdutosAPI.Pedidos.CreatePedido;
using Xunit;

namespace ProdutosAPI.Tests.Integration.Pedidos;

public class AddItemPedidoTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public AddItemPedidoTests(ApiFactory factory)
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
    public async Task POST_AdicionarItem_PedidoInexistente_Retorna404()
    {
        await AuthenticateAsync();

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos/99999/itens",
            new AddItemRequest(1, 1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_AdicionarItem_ProdutoValido_Retorna200()
    {
        await AuthenticateAsync();

        // Create a pedido first
        var create = await _client.PostAsJsonAsync("/api/v1/pedidos",
            new CreatePedidoCommand([new(1, 1)]));
        var pedido = await create.Content.ReadFromJsonAsync<PedidoResponse>();

        // Add another item
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/pedidos/{pedido!.Id}/itens",
            new AddItemRequest(2, 1));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<PedidoResponse>();
        updated!.Itens.Should().HaveCount(2);
    }
}

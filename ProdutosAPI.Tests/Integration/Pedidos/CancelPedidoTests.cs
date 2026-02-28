using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ProdutosAPI.Pedidos.CancelPedido;
using ProdutosAPI.Pedidos.Common;
using ProdutosAPI.Pedidos.CreatePedido;
using Xunit;

namespace ProdutosAPI.Tests.Integration.Pedidos;

public class CancelPedidoTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CancelPedidoTests(ApiFactory factory)
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
    public async Task POST_Cancelar_PedidoInexistente_Retorna404()
    {
        await AuthenticateAsync();

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos/99999/cancelar",
            new CancelPedidoRequest("Teste"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task POST_Cancelar_SemMotivo_Retorna400()
    {
        await AuthenticateAsync();

        var create = await _client.PostAsJsonAsync("/api/v1/pedidos",
            new CreatePedidoCommand([new(1, 1)]));
        var pedido = await create.Content.ReadFromJsonAsync<PedidoResponse>();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/pedidos/{pedido!.Id}/cancelar",
            new CancelPedidoRequest(""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Cancelar_ComMotivo_Retorna200ECancela()
    {
        await AuthenticateAsync();

        var create = await _client.PostAsJsonAsync("/api/v1/pedidos",
            new CreatePedidoCommand([new(1, 1)]));
        var pedido = await create.Content.ReadFromJsonAsync<PedidoResponse>();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/pedidos/{pedido!.Id}/cancelar",
            new CancelPedidoRequest("Desistência do cliente"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelado = await response.Content.ReadFromJsonAsync<PedidoResponse>();
        cancelado!.Status.Should().Be("Cancelado");
        cancelado.MotivoCancelamento.Should().Be("Desistência do cliente");
    }
}

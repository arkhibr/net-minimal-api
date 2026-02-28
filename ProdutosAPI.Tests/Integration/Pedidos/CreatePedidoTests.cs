using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using ProdutosAPI.Pedidos.Common;
using ProdutosAPI.Pedidos.CreatePedido;
using Xunit;

namespace ProdutosAPI.Tests.Integration.Pedidos;

public class CreatePedidoTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CreatePedidoTests(ApiFactory factory)
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
    public async Task POST_Pedidos_SemAutenticacao_Retorna401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/pedidos",
            new CreatePedidoCommand([new(1, 1)]));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task POST_Pedidos_SemItens_Retorna400()
    {
        await AuthenticateAsync();

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos",
            new CreatePedidoCommand([]));

        // Results.ValidationProblem returns 400 Bad Request in ASP.NET Core (not 422)
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Pedidos_ProdutoInexistente_Retorna400()
    {
        await AuthenticateAsync();

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos",
            new CreatePedidoCommand([new(99999, 1)]));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_Pedidos_ComProdutoValido_Retorna201()
    {
        await AuthenticateAsync();

        // Produto Id=1 exists in DbSeeder
        var response = await _client.PostAsJsonAsync("/api/v1/pedidos",
            new CreatePedidoCommand([new(1, 1)]));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var pedido = await response.Content.ReadFromJsonAsync<PedidoResponse>();
        pedido.Should().NotBeNull();
        pedido!.Status.Should().Be("Rascunho");
        pedido.Itens.Should().HaveCount(1);
        response.Headers.Location.Should().NotBeNull();
    }
}

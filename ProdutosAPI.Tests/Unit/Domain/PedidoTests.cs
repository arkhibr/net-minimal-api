using Xunit;
using FluentAssertions;
using ProdutosAPI.Pedidos.Domain;
using ProdutosAPI.Tests.Builders;
using ProdutosAPI.Produtos.Models;

namespace ProdutosAPI.Tests.Unit.Domain;

public class PedidoTests
{
    [Fact]
    public void Criar_RetornaPedidoEmRascunho()
    {
        var pedido = Pedido.Criar();
        pedido.Status.Should().Be(StatusPedido.Rascunho);
        pedido.Itens.Should().BeEmpty();
        pedido.Total.Should().Be(0m);
    }

    [Fact]
    public void AdicionarItem_ProdutoValido_AdicionaItem()
    {
        var pedido = Pedido.Criar();
        var produto = ProdutoBuilder.Padrao().ComPreco(50m).ComEstoque(10).Build();

        var result = pedido.AdicionarItem(produto, 2);

        result.IsSuccess.Should().BeTrue();
        pedido.Itens.Should().HaveCount(1);
        pedido.Total.Should().Be(100m);
    }

    [Fact]
    public void AdicionarItem_MesmoProdutoDuasVezes_MergeQuantidade()
    {
        var pedido = Pedido.Criar();
        var produto = ProdutoBuilder.Padrao().ComEstoque(10).Build();

        pedido.AdicionarItem(produto, 2);
        pedido.AdicionarItem(produto, 3);

        pedido.Itens.Should().HaveCount(1);
        pedido.Itens.First().Quantidade.Should().Be(5);
    }

    [Fact]
    public void AdicionarItem_ProdutoSemEstoque_RetornaFalha()
    {
        var pedido = Pedido.Criar();
        var produto = ProdutoBuilder.Padrao().ComEstoque(0).Build();

        var result = pedido.AdicionarItem(produto, 1);

        result.IsSuccess.Should().BeFalse();
        result.Error!.ToLower().Should().Contain("estoque");
    }

    [Fact]
    public void AdicionarItem_QuantidadeZero_RetornaFalha()
    {
        var pedido = Pedido.Criar();
        var produto = ProdutoBuilder.Padrao().ComEstoque(10).Build();

        var result = pedido.AdicionarItem(produto, 0);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void AdicionarItem_QuantidadeMaiorQue999_RetornaFalha()
    {
        var pedido = Pedido.Criar();
        var produto = ProdutoBuilder.Padrao().ComEstoque(10000).Build();

        var result = pedido.AdicionarItem(produto, 1000);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("999");
    }

    [Fact]
    public void AdicionarItem_MaisDe20ItensdistintosPedido_RetornaFalha()
    {
        var pedido = Pedido.Criar();
        for (int i = 1; i <= 20; i++)
        {
            var p = ProdutoBuilder.Padrao().ComEstoque(100).Build();
            p.SetIdForTesting(i);
            pedido.AdicionarItem(p, 1);
        }
        var extra = ProdutoBuilder.Padrao().Build();
        extra.SetIdForTesting(21);

        var result = pedido.AdicionarItem(extra, 1);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("20");
    }

    [Fact]
    public void AdicionarItem_PedidoConfirmado_RetornaFalha()
    {
        var pedido = Pedido.Criar();
        var produto = ProdutoBuilder.Padrao().ComPreco(50m).ComEstoque(10).Build();
        pedido.AdicionarItem(produto, 1);
        pedido.Confirmar();

        var result = pedido.AdicionarItem(produto, 1);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Confirmar_ComItensEValorSuficiente_Confirma()
    {
        var pedido = Pedido.Criar();
        var produto = ProdutoBuilder.Padrao().ComPreco(50m).ComEstoque(10).Build();
        pedido.AdicionarItem(produto, 1);

        var result = pedido.Confirmar();

        result.IsSuccess.Should().BeTrue();
        pedido.Status.Should().Be(StatusPedido.Confirmado);
        pedido.ConfirmadoEm.Should().NotBeNull();
    }

    [Fact]
    public void Confirmar_SemItens_RetornaFalha()
    {
        var pedido = Pedido.Criar();
        var result = pedido.Confirmar();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Confirmar_TotalAbaixoDoMinimo_RetornaFalha()
    {
        var pedido = Pedido.Criar();
        var produto = ProdutoBuilder.Padrao().ComPreco(1m).ComEstoque(10).Build();
        pedido.AdicionarItem(produto, 1);

        var result = pedido.Confirmar();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("10,00");
    }

    [Fact]
    public void Confirmar_PedidoJaConfirmado_RetornaFalha()
    {
        var pedido = Pedido.Criar();
        var produto = ProdutoBuilder.Padrao().ComPreco(50m).ComEstoque(10).Build();
        pedido.AdicionarItem(produto, 1);
        pedido.Confirmar();

        var result = pedido.Confirmar();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Cancelar_ComMotivo_Cancela()
    {
        var pedido = Pedido.Criar();
        var result = pedido.Cancelar("Desistência do cliente");

        result.IsSuccess.Should().BeTrue();
        pedido.Status.Should().Be(StatusPedido.Cancelado);
        pedido.MotivoCancelamento.Should().Be("Desistência do cliente");
        pedido.CanceladoEm.Should().NotBeNull();
    }

    [Fact]
    public void Cancelar_SemMotivo_RetornaFalha()
    {
        var pedido = Pedido.Criar();
        var result = pedido.Cancelar("");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Cancelar_PedidoJaCancelado_RetornaFalha()
    {
        var pedido = Pedido.Criar();
        pedido.Cancelar("Motivo inicial");
        var result = pedido.Cancelar("Segundo cancelamento");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void AdicionarItem_SnapshotDePrecoFixadoNoPedido()
    {
        var pedido = Pedido.Criar();
        var produto = ProdutoBuilder.Padrao().ComPreco(100m).ComEstoque(10).Build();
        pedido.AdicionarItem(produto, 1);

        produto.AtualizarPreco(200m);

        pedido.Itens.First().PrecoUnitario.Should().Be(100m);
    }
}
